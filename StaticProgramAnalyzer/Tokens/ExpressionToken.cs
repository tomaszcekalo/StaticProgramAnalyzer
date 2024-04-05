using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ExpressionToken : IToken
    {
        public String Content;
        public String FakeExpression;
        public Int64 TestValue = 0;
        public HashSet<String> UsesVariables = new HashSet<string>();
        public HashSet<String> UsesConstants = new HashSet<string>();
        public ExpressionToken(string content) : base()
        {
            Content = content;
        }
        public IToken Parent { get; set; }
        public ParserToken Source { get; set; }

        public IEnumerable<IToken> GetDescentands()
        {
            return new List<IToken>();
        }

        public IEnumerable<IToken> GetChildren()
        {
            return new List<IToken>();
        }
    }
}
