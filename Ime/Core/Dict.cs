using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ime.Core {
    public static class Dict {
        public class T {
            public Node Root { get; set; }
        }
        
        public class Node {
            // root node has ""
            public char Char { get; set; }
            
            public List<Node> Children = new List<Node>();
            public List<Word> Words = new List<Word>();

            public override string ToString() {
                return string.Format("'{0}'=[{1}] ({2})", Char, string.Join(" ;", Words), Children.Count);
            }
        }

        public static T Build(string dictZip) {
            return Util.WithFileInZip(dictZip, sr => {
                var nd = new Node();
                var words = DictFile2Words(sr);
                foreach (var word in words)
                    InsertWord2Tree(nd, word);
                return new T { Root = nd };
            });
        }

        static void InsertWord2Tree(Node tree, Word word) {
            var nd = tree;

            for(var i=0; i<word.Reading.Length; i++) {
                var c = word.Reading[i];

                var child = nd.Children.FirstOrDefault(x => x.Char == c);
                if (child == null) {
                    child = new Node { Char = c };
                    nd.Children.Add(child);
                }
                nd = child;
            }
            nd.Words.Add(word);
        }

        static Word[] DictFile2Words(StreamReader sr) {
            var lines = new List<Word>();
            string line;
            while((line = sr.ReadLine()) != null) {
                var toks = line.Split(' ');
                if (toks.Length != 2 
                    || (toks = toks[1].Split('/')).Length != 2) 
                    continue;
                lines.Add(new Word {
                    Chars = toks[0],
                    Reading = toks[1],
                });
            }
            return lines.ToArray(); 
        }
    }
}
