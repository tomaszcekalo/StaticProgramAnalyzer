using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public abstract class StatementToken : IHasParentToken
    {
        public StatementToken(IToken parent, ParserToken source)
        {
            Parent = parent;
            Source = source;
        }

        public IToken Parent { get; }

        public abstract IEnumerable<IToken> GetDescentands();

        public abstract IEnumerable<IToken> GetChildren();

        public ParserToken Source { get; set; }
    }
}