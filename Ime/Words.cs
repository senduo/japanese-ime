using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ime {
    public class Words {
        public Word[] Xs { get; set; }

        public override string ToString() {
            return string.Join("|", Xs.Select(x => x.Chars));
        }
    }
}
