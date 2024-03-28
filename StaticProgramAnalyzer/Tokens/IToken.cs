using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public interface IToken
    {
        public IEnumerable<IToken> GetChildren();
        public ParserToken Source { get; set; }
    }
}