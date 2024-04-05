using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class VariableToken : RefToken
    {
        public VariableToken(IToken parent, string name, Int64 testValue = 0) : base(name)
        {
            Parent = parent;
            VariableName = name;
            TestValue = testValue;
            UsesVariables.Add(name);
            FakeExpression = name;
        }

        public string VariableName { get; set; }


        public override string ToString()
        {
            return VariableName;
        }

    }
}
