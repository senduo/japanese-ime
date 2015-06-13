using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ime {
    public class Word {
        public string Chars { get; set; }
        public string Reading { get; set; }

        public override string ToString() {
            return string.Format("{0}/{1}", Chars, Reading);
        }

        public override int GetHashCode() {
            var chars = Chars == null ? "" : Chars;
            var reading = Reading == null ? "" : Reading;

            var r = 19;
            r = r * 37 + chars.GetHashCode();
            r = r * 37 + reading.GetHashCode();
            return r;
        }

        public override bool Equals(object obj) {
            var other = obj as Word;
            if (other == null) return false;
            return 
                other.Chars == Chars
                && other.Reading == Reading;
        }
    }
}
