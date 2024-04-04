using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class AssignToken : StatementToken
    {
        public AssignToken(IToken parent, ParserToken source, string fakeExpression, int statementNumber) : base(parent, source, statementNumber)
        {
            VariablesAndConstants= new List<IToken>()
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
                if (char.IsLetter(c) || char.IsDigit(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        var text = sb.ToString();
                        if(int.TryParse(text, out var result))
                        {
                            VariablesAndConstants.Add(new ConstantToken()
                            {
                                Value = result,
                                Parent=this,
                                Source=source
                            });
                        }
                        else
                        {
                            VariablesAndConstants.Add(new UseVariableToken(this, sb.ToString())
                            {
                                Source = source
                            });
                        }
                    }
                    sb.Clear();
                }
            }
        }

        public string VariableName { get; internal set; }
        public string FakeExpression { get; internal set; }
        List<IToken> VariablesAndConstants { get; set; }

        public override IEnumerable<IToken> GetChildren()
        {
            return VariablesAndConstants;
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return VariablesAndConstants;
        }
    }
}
