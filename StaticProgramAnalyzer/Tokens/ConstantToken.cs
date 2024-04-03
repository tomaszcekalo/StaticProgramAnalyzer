using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class ConstantToken : IToken
    {

        public int Value { get; set; }
        public IToken Parent { get; }
        public ParserToken Source { get; set; }

        public IEnumerable<IToken> GetDescentands()
        {
            return new List<IToken>();
        }

        public IEnumerable<IToken> GetChildren()
        {
            return new List<IToken>();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
    
}
