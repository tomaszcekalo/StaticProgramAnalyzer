using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class MinusToken : ExpressionToken
    {
        public ExpressionToken Left;
        public ExpressionToken Right;
        public MinusToken(string content) : base(content)
        {
        }
    }
}
