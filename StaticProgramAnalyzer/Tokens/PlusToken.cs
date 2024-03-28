using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class PlusToken : ExpressionToken
    {
        public ExpressionToken Left;
        public ExpressionToken Right;
        public PlusToken(string content) : base(content)
        {
        }
    }
}
