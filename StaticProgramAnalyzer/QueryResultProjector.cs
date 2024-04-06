using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer
{
    public interface IQueryResultProjector
    {
        string Project(IEnumerable<Dictionary<string, IToken>> combinations, List<string> variableToSelect);
        string ProjectBoolean(IEnumerable<Dictionary<string, IToken>> combinations);
    }
    public class QueryResultProjector : IQueryResultProjector
    {
        public string Project(IEnumerable<Dictionary<string, IToken>> combinations, List<string> variableToSelect)
        {
            IEnumerable<(Dictionary<string, IToken> x, StringBuilder sb)> output = combinations.Select(x =>
            {
                var sb = new StringBuilder();
                sb.Append(x[variableToSelect[0]].ToString());
                return (x, sb);
            });

            for (int i = 1; i < variableToSelect.Count; i++)
            {
                var varName = variableToSelect[i];
                output = output.Select(x =>
                {
                    x.sb.Append(" ");
                    x.sb.Append(x.x[varName].ToString());
                    return x;
                });
            }
            // build strings (variables, or variable tuples)
            var outputs = output.Select(x => x.sb.ToString()).Distinct();
            if(outputs.All(x=> int.TryParse(x, out _)))
            {
                outputs = outputs.OrderBy(x => int.Parse(x));
            }
            if(outputs.Any())
            {
                // join all of them together
                var resultString = string.Join(", ", outputs);

                return resultString;
            }
            return "none";
        }

        public string ProjectBoolean(IEnumerable<Dictionary<string, IToken>> combinations)
        {
            return combinations.Any().ToString().ToLower();
        }
    }
}
