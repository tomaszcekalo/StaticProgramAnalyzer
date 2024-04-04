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
        List<IToken> Variables { get; set; }
        public AssignToken(IToken parent, ParserToken source, int statementNumber) : base(parent, source, statementNumber)
        {
            /*
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
            */
        }
        public override IEnumerable<IToken> GetChildren()
        {
            return Variables;
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return Variables;
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
            return String.Format("{0}={1}", Left.VariableName, Right.Content);
        }
    }
}
