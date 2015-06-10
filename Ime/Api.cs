using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Ime.Core;
using Ime.Converter;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace Ime {
    public class Api {
        public class Result<T> {
            public T Val { get; set; }
            public bool IsErr { get; set; }
            public string ErrMsg { get; set; }
            public override string ToString() {
                return IsErr ? "Err: " + ErrMsg : Val.ToString();
            }
        }

        Dict.T dict;
        Bigram bigram;
        
        public Api(string dictFile, string uniFreqFile, string biFreqFile) {
            if (!File.Exists(dictFile))
                throw new Exception(string.Format("dict file '{0}' is missing", dictFile));

            this.dict = Dict.Build(dictFile);
            this.bigram = new Bigram(uniFreqFile, biFreqFile);
        }

        Result<T> Try<T>(Func<T> f) {
            try {
                return new Result<T> { Val = f() };
            }
            catch (Exception ex) {
                return new Result<T> {
                    ErrMsg = ex.Message,
                    IsErr = true
                };
            }
        }

        public Result<string> NormalizeInput(string s) {
            return Try(() => {
                var expr = @"^[\p{IsHiragana}|\p{IsKatakana}]*$";
                if (!Regex.IsMatch(s, expr))
                    throw new Exception("Expected hira/katakana, but got: " + s);
                return Strings.StrConv(s, VbStrConv.Katakana, 0x411);
            });
        }

        public Result<WordGraph.T> BuildWordGraph(string s) {
            return Try(() => WordGraph.Build(s, dict.Root));
        }

        public Result<Words> ConvertByBigram(WordGraph.T wg) {
            return Try(() => bigram.Convert(wg));
        }

        public Dictionary<string, double> BigramParams {
            get { return bigram.Param; }
        }
    }
}
