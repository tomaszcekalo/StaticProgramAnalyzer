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
            Next = new List<StatementToken>();
        }

        public abstract IEnumerable<IToken> GetDescentands();

        public abstract IEnumerable<StatementToken> GetChildren();

        public IToken Parent { get; set; }
        public ParserToken Source { get; set; }
        public int StatementNumber { get; set; }

        public override string ToString()
        {
            return StatementNumber.ToString();
        }

        public List<StatementToken> Next { get; set; }

        public virtual void AddNext(StatementToken next)
        {
            Next.Add(next);
        }
    }
}