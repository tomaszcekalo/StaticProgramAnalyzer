using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class RefToken : ExpressionToken
    {
        public RefToken(string content) : base(content)
        {
        }
    }
}
