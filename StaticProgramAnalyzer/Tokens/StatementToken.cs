using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public abstract class StatementToken : IToken
    {
        public StatementToken(IToken parent, ParserToken source, int statementNumber)
        {
            this.Parent = parent;
            Source = source;
            StatementNumber = statementNumber;
        }


        public abstract IEnumerable<IToken> GetDescentands();

        public abstract IEnumerable<IToken> GetChildren();

        public IToken Parent { get; }
        public ParserToken Source { get; set; }
        public int StatementNumber { get; set; }
        public override string ToString()
        {
            //return StatementNumber.ToString();
            return Source.LineNumber.ToString();
        }
    }
}