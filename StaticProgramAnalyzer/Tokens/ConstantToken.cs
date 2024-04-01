using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace StaticProgramAnalyzer.Tokens
{
    public class ConstantToken : RefToken
    {
        public ConstantToken(string content) : base(content)
        {
            TestValue = int.Parse(content);
            UsesConstants.Add(content);
        }
    }
}
