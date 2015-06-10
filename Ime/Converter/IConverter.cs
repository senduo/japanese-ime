using System;
using System.Collections.Generic;
using Ime.Core;

namespace Ime.Converter {
    interface IConverter {
        Words Convert(WordGraph.T wg);
        Dictionary<string, double> Param { get; }
    }
}
