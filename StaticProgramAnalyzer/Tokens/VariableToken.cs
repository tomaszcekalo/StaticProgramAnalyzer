using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public abstract class VariableToken : IHasParentToken
    {
        public VariableToken(IToken parent, string name)
        {
            Parent = parent;
            VariableName = name;
        }

        public string VariableName { get; set; }
        public IToken Parent { get; }

        public IEnumerable<IToken> GetDescentands()
        {
            return new List<IToken>();
        }

        public override string ToString()
        {
            return VariableName;
        }

        public IEnumerable<IToken> GetChildren()
        {
            return new List<IToken>();
        }

        public ParserToken Source { get; set; }
    }
}
