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
            return Chars;
        }

        public override int GetHashCode() {
            return Chars.GetHashCode();
        }

        public override bool Equals(object obj) {
            var other = obj as Word;
            if (other == null) return false;
            return other.Chars == Chars;
        }
    }
}
