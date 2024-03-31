using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class VariableToken : RefToken
    {
        public VariableToken(string content, Int64 testValue = 0) : base(content)
        {
            TestValue = testValue;
        }
    }
}
