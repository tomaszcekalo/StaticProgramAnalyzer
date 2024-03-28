using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class VariableToken : RefToken
    {
        public VariableToken(string content) : base(content)
        {
        }
    }
}
