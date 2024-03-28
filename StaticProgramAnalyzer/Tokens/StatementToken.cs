using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public abstract class StatementToken : IToken
    {
        public StatementToken(IToken parent)
        {
            Parent = parent;
        }

        public IToken Parent { get; }

        public abstract IEnumerable<IToken> GetChildren();
        public ParserToken Source { get; set; }
    }
}