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
            public Dictionary<string, int> One =
                new Dictionary<string, int>();

            public Dictionary<Tuple<string, string>, int> Two =
                new Dictionary<Tuple<string, string>, int>();
        }

        readonly Freqs freqs;
        readonly int freqOneSum;
        readonly int numVocabs;

        // TODO fix arbitrary smoothing coeffs
        Dictionary<string, double> param = new Dictionary<string, double> {
            { "lam2", 0.99 },
            { "lam1", 0.009 },
            { "lam0", 0.001 },
        };

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

        public Bigram(string uniFreqZip, string biFreqZip) {
            this.freqs = new Freqs {
                One = Zip2Dict(uniFreqZip, line => {
                    var toks = line.Split(' ');
                    return Tuple.Create(toks[0], int.Parse(toks[1]));
                }),
                Two = Zip2Dict(biFreqZip, line => {
                    var toks = line.Split(' ');
                    var x = Tuple.Create(toks[0], toks[1]);
                    return Tuple.Create(x, int.Parse(toks[2]));
                }),
            };
            this.freqOneSum = this.freqs.One.Values.Sum();
            this.numVocabs = this.freqs.One.Keys.Count();
        }

        double SafeGet<T>(Dictionary<T, int> dict, T k) {
            if (!dict.ContainsKey(k)) return 0.0;
            return (double) dict[k];
        }

        // calculates P(curr|prev)
        double CalcProb(string curr, string prev) {
            // using liniear interpolation
            // P(wi|wi-1) = λPml(wi|wi-1) + λPml(wi) + λ1/V    // V = # of vocabularies
            var c2 = SafeGet(this.freqs.Two, Tuple.Create(prev, curr));
            var c1 = SafeGet(this.freqs.One, curr);
            var p2 = c1 == 0.0 ? 0.0 : c2 / c1;
            var p1 = c1 / this.freqOneSum;
            var p0 = 1.0 / this.numVocabs;

            var prob = 
                this.Param["lam2"] * p2 
                + this.Param["lam1"] * p1 
                + this.Param["lam0"] * p0;

            return prob;
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
                        var prob = CalcProb(curr.Word.Chars, prev.Word.Chars);
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

        Words RunViterviBwd(WordGraph.T wg) { 
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
            var words = RunViterviBwd(wg);

            return words;
        }

        public Dictionary<string, double> Param {
            get { return param; }
        }
    }
}
