using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Ime.Core;

namespace Ime.Converter {
    public class Bigram : IConverter {
        public class Freqs {
            public Dictionary<Word, int> One =
                new Dictionary<Word, int>();

            public Dictionary<Tuple<Word, Word>, int> Two =
                new Dictionary<Tuple<Word, Word>, int>();
        }

        readonly Freqs freqs;
        readonly int freqOneSum;
        readonly int freqTwoSum;
        readonly int numVocabs;

        Dictionary<T, int> Zip2Dict<T>(
            string zipFile,
            Func<string, Tuple<T, int>> conv
        ) {
            if (!File.Exists(zipFile))
                throw new Exception(string.Format("Bigram source '{0}' is missing", zipFile));

            return Util.WithFileInZip(zipFile, sr => {
                var dict = new Dictionary<T, int>();
                string line;
                while ((line = sr.ReadLine()) != null) {
                    var x = conv(line);
                    dict[x.Item1] = x.Item2;
                }
                return dict;
            });
        }

        Word Tok2Word(string tok) {
            var xs = tok.Split('/');
            return new Word { 
                Chars = xs[0], 
                Reading = xs.Length == 1 ? "" : xs[1] 
            };
        }

        public Bigram(string unigramFile, string bigramFile) {
            this.freqs = new Freqs {
                One = Zip2Dict(unigramFile, line => {
                    var toks = line.Trim().Split(' ');
                    var n = int.Parse(toks[0]);
                    var x = Tok2Word(toks[1]);
                    return Tuple.Create(x, n);
                }),
                Two = Zip2Dict(bigramFile, line => {
                    var toks = line.Split(' ');
                    var n = int.Parse(toks[0]);
                    var x = Tuple.Create(
                        Tok2Word(toks[1]), 
                        Tok2Word(toks[2]) 
                    );
                    return Tuple.Create(x, n);
                }),
            };
            this.freqOneSum = this.freqs.One.Values.Sum();
            this.freqTwoSum = this.freqs.Two.Values.Sum();
            this.numVocabs = this.freqs.One.Keys.Count();
        }

        double SafeGet<T>(Dictionary<T, int> dict, T k) {
            if (!dict.ContainsKey(k)) return 0.0;
            return (double) dict[k];
        }

        // calculates P(curr|prev)
        double CalcProb(Word curr, Word prev) {
            var c2 = SafeGet(this.freqs.Two, Tuple.Create(prev, curr));
            var c1 = SafeGet(this.freqs.One, prev);

            var penalty = this.freqOneSum / c1;
            var denomi = c1 + penalty; 
            
            var prob = c1 == 0.0 ? 0.0 : c2 / denomi;
            var missingWordCoeff = +1.0 / this.freqOneSum;

            return prob + missingWordCoeff;
        }

        // calculate node probs and set prev flags
        void RunViterbiFwd(WordGraph.T wg) {
            foreach (var p in wg.EndLocMap.Skip(1) /* skips BOS */) {
                var endIdx = p.Key;

                foreach (var nd in p.Value) {
                    var wordLen = nd.Word.Chars == Const.EOS ? 1 : nd.Word.Reading.Length;
                    var begIdx = endIdx - wordLen + 1;
                    var prevs = wg.EndLocMap[begIdx - 1];

                    // find prev node w/ the best value
                    WordGraph.Node bestNd = null;
                    double bestVal = double.MinValue;

                    foreach (var prev in prevs) {
                        var curr = nd;
                        var prob = CalcProb(curr.Word, prev.Word);
                        var val = prev.Val + prob;
                        if (val > bestVal) {
                            bestVal = val;
                            bestNd = prev;
                        }
                    }
                    nd.Prev = bestNd;
                    nd.Val = bestVal;
                }
            }
        }

        Words RunViterbiBwd(WordGraph.T wg) { 
            var nd = wg.Eos;
            var xs = new LinkedList<Word>();

            while (nd != wg.Bos) {
                xs.AddFirst(nd.Word);
                nd = nd.Prev;
            }
            xs.RemoveLast();
            return new Words { Xs = xs.ToArray() };
        }

        public Words Convert(WordGraph.T wg) {
            RunViterbiFwd(wg);
            var words = RunViterbiBwd(wg);

            return words;
        }
    }
}
