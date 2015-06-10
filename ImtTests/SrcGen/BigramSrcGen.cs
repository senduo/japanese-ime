using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ime.Converter;

namespace Ime.SrcGen {
    [TestClass]
    public class BigramSrcGen {
        void AddFreq<T>(Dictionary<T, int> dict, T v) {
            if (dict.ContainsKey(v))
                dict[v] = dict[v] + 1;
            else
                dict[v] = 1;
        }

        string[][] ToSentences(IEnumerable<string> toks) {
            var sentences = new List<string[]>();
            var sentence = new List<string>();

            foreach (var tok in toks) {
                sentence.Add(tok);
                if (tok == "。") {
                    sentences.Add(sentence.ToArray());
                    sentence.Clear();
                }
            }
            if (sentence.Count() > 0)
                sentences.Add(sentence.ToArray());
            return sentences.ToArray();
        }

        Bigram.Freqs AddFile2Freq(Bigram.Freqs freq, string file) {
            using (var sr = new StreamReader(file)) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    line = line.Trim();
                    if (!line.EndsWith("。")) continue;   // only interested in sentences

                    var toks = line.Trim().Split(' ')
                        .Select(x => x.Split('/').First())
                        .Where(x => x != "");

                    foreach (var sentence in ToSentences(toks)) {
                        var lineToks =
                            new[] { Const.BOS }
                            .Concat(sentence)
                            .Concat(new[] { Const.EOS })
                            .ToArray();

                        for (var i = 0; i < lineToks.Length - 1; i++) {
                            AddFreq(freq.One, lineToks[i]);
                            AddFreq(freq.Two, new Tuple<string, string>(
                                lineToks[i], lineToks[i + 1]));
                        }
                        AddFreq(freq.One, lineToks.Last());
                    }
                }
            }
            return freq;
        }

        [TestMethod]
        public void GenBigramSrc() {
            var files = Directory.GetFiles(@"..\..\..", "*.txt");

            var freq = files.Aggregate(new Bigram.Freqs(), 
                (acc, file) => AddFile2Freq(acc, file));

            var uni = freq.One.Select(p => string.Format("{0} {1}", p.Key, p.Value));
            File.WriteAllLines("unigram-freq.txt", uni);

            var bi = freq.Two.Select(p =>
                string.Format("{0} {1} {2}", p.Key.Item1, p.Key.Item2, p.Value));
            File.WriteAllLines("bigram-freq.txt", bi);
        }
    }
}
