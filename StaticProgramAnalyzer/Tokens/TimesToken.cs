using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class TimesToken : ExpressionToken
    {
        public ExpressionToken Left;
        public ExpressionToken Right;
        public TimesToken(string content) : base(content)
        {
        }
    }
}
