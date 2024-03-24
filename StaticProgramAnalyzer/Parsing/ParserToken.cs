using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Parsing
{
    public class ParserToken
    {
        public string Content { get; set; }
        public int LineNumber { get; set; }
        public int Position { get; set; }

        public override string ToString()
        {
            return $"{Content} at line {LineNumber} position {Position}";
        }
    }
}
