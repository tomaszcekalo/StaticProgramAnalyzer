using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace StaticProgramAnalyzer.Tokens
{
    internal class ConstantToken : RefToken
    {
        public int TestValue { get; set; }

        public ConstantToken(string content) : base(content)
        {
            TestValue = int.Parse(content);
            UsesConstants.Add(content);
            FakeExpression = content;
        }
    }
    
    
}
