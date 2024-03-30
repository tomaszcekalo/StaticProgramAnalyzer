using StaticProgramAnalyzer.QueryProcessing.Predicates;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing
{
    public interface IQueryProcessor
    {
        string ProcessQuery(string declarations, string selects);

        Dictionary<string, string> GetDeclarations(string declarations);
    }

    public class QueryProcessor : IQueryProcessor
    {
        private ProgramKnowledgeBase _pkb;
        private char[] _whitespace = new char[] { ' ', '\t', '\n', '\r', ',' };

        public QueryProcessor(ProgramKnowledgeBase pkb)
        {
            this._pkb = pkb;
        }

        public Dictionary<string, string> GetDeclarations(string declarations)
        {
            string[] declarationArray = declarations.Split(';', StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> typeDictionary = declarationArray
                .Select(x => new
                {
                    row = x.Split(_whitespace, StringSplitOptions.RemoveEmptyEntries)
                })
                .Select(x => new
                {
                    type = x.row[0],
                    ids = x.row.Skip(1)
                })
                .SelectMany(x => x.ids.Select(y => new { x.type, id = y }))
                .ToDictionary(x => x.id, x => x.type);
            return typeDictionary;
        }


        public string ProcessQuery(string declarations, string selects)
        {
            var declarationDictionary = GetDeclarations(declarations.Trim());
            var variablePredicates = declarationDictionary.ToDictionary(x => x.Key, x => GetTypePredicate(x.Value));

            var variableQueries = variablePredicates.ToDictionary(
                key => key.Key,
                value => _pkb.TokenList.Where(token => value.Value.Evaluate(token)));
            var variableNames = declarationDictionary.Keys.ToList();

            var firstKey = variableNames[0];
            var a = variableQueries[firstKey]
                .Select(x => new Dictionary<string, IToken>()
                {
                    {firstKey, x},
                });
            // adding all from declaration
            for( var i = 1; i<variableNames.Count; i++)
            {
                var varName= variableNames[i];
                a = a.SelectMany(fir => variableQueries[varName].Select(sec =>
                {
                    var newDict = fir.ToDictionary(x => x.Key, y => y.Value);
                    newDict.Add(varName, sec);
                    return newDict;
                })).ToList();
            }
            var b = a.ToList();
            // selects
            var variables = GetVariablesToSelect(selects, variablePredicates);

            var output = b.Select(x =>
            {
                var sb = new StringBuilder();
                sb.Append(x[variableNames[0]].ToString());
                return (x, sb);
            });

            for(int i=1; i<variableNames.Count; i++)
            {
                var varName = variableNames[i];
                output = output.Select(x =>
                {
                    x.sb.Append(" ");
                    x.sb.Append(x.x[varName].ToString());
                    return x;
                });
            }

            var outputs = output.Select(x => x.sb.ToString()).Distinct();
            var resultString = string.Join(", ", outputs);

            return resultString;
        }

        public void Calls(Dictionary<string, IEnumerable<Tokens.IToken>> variableQueries, string callerVariableName, string calledVariableName)
        {
            var calls = variableQueries[callerVariableName].Select(x => new
            {
                caller = x,
                called = x.GetChildren().OfType<Tokens.CallToken>().Where(child => variableQueries[calledVariableName].Any(bb => bb.ToString() == child.ProcedureName))
            }).Where(x => x.called.Any());
            variableQueries[callerVariableName] = calls.Select(x => x.caller);
            variableQueries[calledVariableName] = calls.SelectMany(x => x.called);
        }

        public List<string> GetVariablesToSelect(string selects, Dictionary<string, IPredicate> variableQueries)
        {
            List<string> result = new List<string>();
            Queue<string> queue = new Queue<string>(selects.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var token = queue.Dequeue();
            bool getAnother = true;
            if (token == "Select")
            {
                while (getAnother)
                {
                    token = queue.Dequeue();
                    token = token.Replace("<", "")
                        .Replace(">", "");

                    if (token.EndsWith(','))
                    {
                        token = token.Substring(0, token.Length - 1);
                    }
                    else
                    {
                        getAnother = false;
                    }
                    if (variableQueries.Keys.Contains(token) || token == "BOOLEAN")
                    {
                        result.Add(token);
                    }
                    else throw new Exception("Invalid variable name");
                }
            }
            return result;
        }

        public IPredicate GetTypePredicate(string type)
        {
            if (type == "procedure")
            {
                return new IsProcedurePredicate();
            }
            else if (type == "stmt")
            {
                return new IsStatementPredicate();
            }
            else if (type == "variable")
            {
                return new IsVariablePredicate();
            }
            else if (type == "while")
            {
                return new IsWhilePredicate();
            }
            else if (type == "if")
            {
                return new IsIfTheElsePredicate();
            }
            else if (type == "assign")
            {
                return new IsAssignPredicate();
            }
            else if (type == "while")
            {
                return new IsWhilePredicate();
            }
            return null;// TODO : throw exception or choose what to do
        }

    }
}