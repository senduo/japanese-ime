using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ime.Core {
    public static class WordGraph {
        public class T {
            public string SrcStr { get; set; }
            public Node Bos { get; set; }
            public Node Eos { get; set; }
            public SortedDictionary<int,List<Node>> EndLocMap { get; set; }
        }

        public class Node {
            public Word Word { get; set; }
            public double Val { get; set; }
            public Node Prev { get; set; }
            
            public override string ToString() {
                return string.Format("[{0}/{1}]", Word, Val);
            }
        }

        static readonly Node Bos = new Node { 
            Word = new Word { Chars = Const.BOS, Reading = "" } 
        };
        static readonly Node Eos = new Node {
            Word = new Word { Chars = Const.EOS, Reading = "" } 
        };

        static SortedDictionary<int, List<Node>> GenEndLocMap(string s, Dict.Node dict) {
            var m = new SortedDictionary<int, List<Node>>();

            m[0] = new List<Node> { Bos };
            m[s.Length + 1] = new List<Node> { Eos };

            for (var b = 0; b < s.Length; b++) {
                var nd = dict;
                for (var l = 1; l <= s.Length - b; l++) {
                    var ss = s.Substring(b, l);
                    nd = nd.Children.FirstOrDefault(x => x.Char == ss[l - 1]);
                    if (nd == null) break;

                    var idx = b + l;    // 1-based end index
                    if (!m.ContainsKey(idx))
                        m[idx] = new List<Node>();
                    var nodes = nd.Words.Select(word => new Node { Word = word });
                    m[idx].AddRange(nodes);
                }
            }
            return m;
        }

        public static T Build(string s, Dict.Node dict) {
           return new T {
                SrcStr = s,
                Bos = Bos,
                Eos = Eos,
                EndLocMap = GenEndLocMap(s, dict),
            };
        }
    }
}



