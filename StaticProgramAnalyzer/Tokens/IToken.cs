using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public interface IToken
    {
        public IEnumerable<IToken> GetDescentands();
        public IEnumerable<IToken> GetChildren();
        public ParserToken Source { get; set; }
        public IToken Parent { get; }
    }
}