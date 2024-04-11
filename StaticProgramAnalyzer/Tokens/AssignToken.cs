using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class AssignToken : StatementToken
    {
        public ModifyVariableToken Left;
        public ExpressionToken Right;
        internal HashSet<string> UsesConstants;
        internal HashSet<string> UsesVariables;
        internal HashSet<string> Modifies;

        public string FakeExpression { get; internal set; }
        List<IToken> VariablesAndConstants { get; set; }
        public AssignToken() :base(null, null, 0){ }
        public AssignToken(IToken parent, ParserToken source, string fakeExpression, int statementNumber) : base(parent, source, statementNumber)
        {
            VariablesAndConstants = new List<IToken>()
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
                        if (int.TryParse(text, out var result))
                        {
                            VariablesAndConstants.Add(new ConstantToken(text)
                            {
                                Value = result,
                                Parent = this,
                                Source = source
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
        public override IEnumerable<StatementToken> GetChildren()
        {
            //return VariablesAndConstants;
            return null;
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return VariablesAndConstants;
        }
        //checks if provided tree exists in the assigment tree
        public bool ContainsTree(AssignToken checkTree)
        {
            return Right.Content.Contains(checkTree.Right.Content);
        }
        //checks if the assigment tree equals provided one
        public bool EqualsTree(AssignToken checkTree)
        {
            return Right.Content.Equals(checkTree.Right.Content);
        }
        public override string ToString()
        {
            //return String.Format("{0}={1}", Left.VariableName, Right.Content);
            return StatementNumber.ToString();
        }
        /*
        internal void SetVariablesAndConstants()
        {
            VariablesAndConstants.Add()
            foreach (var c in UsesConstants){
                VariablesAndConstants.Add(new UseVariableToken(this, c)
                {
                    Source = this.Source
                });
            }
            foreach (var c in UsesVariables)
            {
                VariablesAndConstants.Add(new UseVariableToken(this, c)
                {
                    Source = this.Source
                });
            }
        }
        */
    }
}
