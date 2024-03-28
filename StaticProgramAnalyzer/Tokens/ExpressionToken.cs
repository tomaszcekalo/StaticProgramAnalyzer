using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ExpressionToken
    {
        String Content;

        public ExpressionToken(string content)
        {
            Content = content;
        }
    }
}
