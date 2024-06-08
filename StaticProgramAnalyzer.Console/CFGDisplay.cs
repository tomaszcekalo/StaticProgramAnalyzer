using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticProgramAnalyzer.Console
{
    internal class CFGDisplay
    {
        internal void Display(IEnumerable<IToken> tokenList)
        {
            foreach (var token in tokenList.OfType<StatementToken>())
            {
                foreach (var next in token.Next)
                {
                    System.Console.WriteLine(
                        $"{token.StatementNumber.ToString()} --> {next.StatementNumber.ToString()}");
                }
            }
        }
    }
}