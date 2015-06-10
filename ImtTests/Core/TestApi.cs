using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ime.Core {
    [TestClass]
    public class TestApi {
        const string Dir = @"..\..\..\Resources\";
        readonly Api api = new Api(
            Dir + "naist-jdic-utf8.zip",
            Dir + "unigram-freq.zip",
            Dir + "bigram-freq.zip"
        );

        [TestMethod]
        public void ConvertByBigram() {
            var res1 = api.BuildWordGraph("キョウハイイテンキ");
            var res2 = api.ConvertByBigram(res1.Val);
        }
    }
}
