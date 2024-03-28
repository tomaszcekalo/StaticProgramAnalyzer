using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class VariableToken : IToken
    {
        public VariableToken(IToken parent, string name)
        {
            Name = name;
        }

        public string Name { get; internal set; }

        public IEnumerable<IToken> GetChildren()
        {
            return new List<IToken>();
        }

        public override string ToString()
        {
            return Name;
        }
        public ParserToken Source { get; set; }
    }
}
