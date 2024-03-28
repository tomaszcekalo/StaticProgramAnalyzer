using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class AssignToken : StatementToken
    {
        public AssignToken(IToken parent, ParserToken token, string fakeExpression) : base(parent)
        {
            Variables= new List<IToken>()
            {
                new VariableToken(this, token.Content)
                {
                    Source = token
                }
            };
            Source = token;
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
                        Variables.Add(new VariableToken(this, sb.ToString())
                        {
                            Source = token
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
        public override string ToString()
        {
            return Source.LineNumber.ToString();
        }

        //public override string ToString()
        //{
        //    var sb= new StringBuilder();
        //    var assignNodeName= $"assign_{VariableName}_{FakeExpression.GetHashCode()}";
        //    sb.AppendLine($"{assignNodeName}(assign)");
        //    sb.AppendLine($"{assignNodeName} --> {assignNodeName}_{VariableName}({VariableName})");

        //    return sb.ToString();
        //}
    }
}
