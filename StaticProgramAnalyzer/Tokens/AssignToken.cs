using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class AssignToken : StatementToken
    {
        public AssignToken(IToken parent, ParserToken source, string fakeExpression) : base(parent, source)
        {
            Variables= new List<IToken>()
            {
                new ModifyVariableToken(this, source.Content)
                {
                    Source = source
                }
            };
            FakeExpression = fakeExpression;
            StringBuilder sb = new StringBuilder();
            foreach (var c in fakeExpression)
            {
                if (char.IsLetter(c))
                {
                    sb.Append(c);
                }
                
                else
                {
                    if (sb.Length > 0)
                    {
                        Variables.Add(new UseVariableToken(this, sb.ToString())
                        {
                            Source = source
                        });
                    }
                    sb.Clear();
                }
            }
        }

        public string VariableName { get; internal set; }
        public string FakeExpression { get; internal set; }
        List<IToken> Variables { get; set; }

        public override IEnumerable<IToken> GetChildren()
        {
            return Variables;
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return Variables;
        }
        public override string ToString()
        {
            return Source.LineNumber.ToString();
        }
    }
}
