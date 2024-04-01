using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ExpressionToken
    {
        public String Content;
        public Int64 TestValue = 0;
        public HashSet<String> UsesVariables = new HashSet<string>();
        public HashSet<String> UsesConstants = new HashSet<string>();
        public ExpressionToken(string content)
        {
            Content = content;
        }
    }
}
