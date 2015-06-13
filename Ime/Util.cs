using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ime {
    public static class Util {
        public static T WithFileInZip<T>(
            string zipFile, 
            Func<StreamReader, T> f
        ) {
            using (var za = ZipFile.OpenRead(zipFile)) {
                var tgtFile = Path.GetFileName(zipFile.Replace(".zip", ".txt"));
                var e = za.GetEntry(tgtFile);
                using (var st = e.Open())
                    using (var sr = new StreamReader(st))
                        return f(sr);
            }
        }
    }
}
