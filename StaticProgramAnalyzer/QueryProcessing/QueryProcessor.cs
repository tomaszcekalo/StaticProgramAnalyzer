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
        private readonly IQueryResultProjector projector;
        private char[] _whitespace = new char[] { ' ', '\t', '\n', '\r', ',' };

        public QueryProcessor(ProgramKnowledgeBase pkb, IQueryResultProjector projector)
        {
            this._pkb = pkb;
            this.projector = projector;
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
                value => _pkb.TokenList.Where(token => value.Value.Evaluate(token)).ToList());
            variableQueries.Add("_", _pkb.TokenList.ToList());
            var variableNames = variableQueries.Keys.ToList();

            IEnumerable<Dictionary<string, IToken>> combinations =new List<Dictionary<string, IToken>>();
            if(variableNames.Count>0)
            {
                var firstKey = variableNames[0];
                combinations = variableQueries[firstKey]
                   .Select(x => new Dictionary<string, IToken>()
                   {
                    {firstKey, x},
                   });
            }
            // adding all from declaration
            for (var i = 1; i < variableNames.Count; i++)
            {
                var varName = variableNames[i];
                combinations = combinations.SelectMany(fir => variableQueries[varName].Select(sec =>
                {
                    var newDict = fir.ToDictionary(x => x.Key, y => y.Value);
                    newDict.Add(varName, sec);
                    return newDict;
                })).ToList();
            }

            // here are conditions
            var conditionStrings = selects.Split(new string[]
            {
                " such that ",
                " with ",
                " and ",
            }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
            for (int i = 1; i < conditionStrings.Count; i++)
            {
                combinations = FilterByCondition(combinations, conditionStrings[i]);
            }
            // get only variables that are in select
            var variableToSelect = GetVariablesToSelect(selects, variablePredicates);

            if(variableToSelect.Contains("BOOLEAN"))
            {
                return projector.ProjectBoolean(combinations);
            }
            return projector.Project(combinations, variableToSelect);
        }

        public IEnumerable<Dictionary<string, IToken>> FilterByParameter(IEnumerable<Dictionary<string, IToken>> combinations, string condition)
        {
            var array = condition.Split('=');
            var leftArray = array[0].Split('.', StringSplitOptions.RemoveEmptyEntries);
            var pqlVar = leftArray[0].Trim();
            var pqlProperty = leftArray[1].Trim();
            var value = array[1].Trim().Replace("\"", "");// TODO we probably want to compare with other parameters

            if (pqlProperty == "procName")
            {
                return combinations.Where(x =>
                {
                    var withProcname = x[pqlVar] as IHasProcedureName;
                    return withProcname?.ProcedureName == value.Trim();
                });
            }
            if (pqlProperty == "stmt#")
            {
                var lineNumber = int.Parse(array[1].Trim());
                return combinations.Where(x => x[pqlVar].Source.LineNumber == lineNumber);
            }
            if (pqlProperty == "varName")
            {
                return combinations.Where(x =>
                {
                    var withVarName = x[pqlVar] as VariableToken;
                    return withVarName?.VariableName == array[1].Trim();
                });
            }
            return combinations;// todo throw exception?
        }

        public IEnumerable<Dictionary<string, IToken>> FilterByCondition(
            IEnumerable<Dictionary<string, IToken>> combinations,
            string condition)
        {
            var parameters = condition.Split(new char[] { '(', ')' },
                StringSplitOptions.RemoveEmptyEntries);
            var parametersArray = parameters.Last().Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (condition.Contains("="))
            {
                return FilterByParameter(combinations, condition);
            }

            if (condition.StartsWith("Calls*"))
            {
                return CallsStar(combinations, parametersArray[0], parametersArray[1]);
            }
            else if (condition.StartsWith("Calls"))
            {
                return Calls(combinations, parametersArray[0], parametersArray[1]);
            }
            if (condition.StartsWith("Parent*"))
            {
                //TODO ParentStart
            }
            else if (condition.StartsWith("Parent"))
            {
                return Parent(combinations, parametersArray[0], parametersArray[1]);
            }
            if(condition.StartsWith("Uses"))
            {
                return Uses(combinations, parametersArray[0], parametersArray[1]);
            }
            if(condition.StartsWith("Modifies"))
            {
                return Modifies(combinations, parametersArray[0], parametersArray[1]);
            }

            return combinations;
        }


        private IEnumerable<Dictionary<string, IToken>> Modifies(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            right = right.Trim().Replace("\"", "");
            var assignments=
             combinations.Where(x =>
            {
                var leftToken = x[left];
                var descendantsThatModifies = leftToken.GetDescentands()
                    .OfType<ModifyVariableToken>();
                return descendantsThatModifies
                    .Any(modifies => modifies.VariableName == right);
            }).ToList();
            var assignmentProcedures = assignments
                .Select(x=> x[left])
                .OfType<IHasParentToken>()
                .Select(x => GetFinalParent(x, left).ProcedureName)
                .ToList();
            var calls = combinations.Where(x => x[left] is CallToken)
                .Where(x => assignmentProcedures.Contains((x[left] as CallToken).ProcedureName));

            return assignments.Concat(calls);
        }

        private ProcedureToken GetFinalParent(IHasParentToken x, string left)
        {
            if(x.Parent is ProcedureToken)
            {
                return x.Parent as ProcedureToken;
            }
            return GetFinalParent(x.Parent as IHasParentToken, left);
        }

        private IEnumerable<Dictionary<string, IToken>> Uses(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            right = right.Trim().Replace("\"","");
            return combinations.Where(x =>
            {
                var leftToken = x[left.Trim()];
                var descendantsThatUse = leftToken.GetDescentands()
                    .OfType<UseVariableToken>();
                return descendantsThatUse
                    .Any(use => use.VariableName == right);
            }).ToList();
        }

        public IEnumerable<Dictionary<string, IToken>> Parent(
            IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            //if second parameter is a line number
            if (int.TryParse(right, out int lineNumber))
            {
                //parents of statements at that line number
                return combinations.Where(c =>
                {
                    var children = c[left].GetDescentands();
                    return children.OfType<IHasParentToken>()
                        .Any(x => x.Source.LineNumber == lineNumber
                            && x.Parent == c[left]);
                }).ToList();
            }
            return combinations.Where(x =>
                (x[right.Trim()] as StatementToken)?.Parent == x[left.Trim()]
            );
        }

        public IEnumerable<Dictionary<string, IToken>> CallsStar(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            right = right.Trim();
            bool rightHasQuotes = right.StartsWith("\"") && right.EndsWith("\"");

            return combinations.Where(x =>
                _pkb.AllCalls[(x[left] as IHasProcedureName).ProcedureName].Contains((x[right] as IHasProcedureName).ProcedureName));


        }
        public IEnumerable<Dictionary<string, IToken>> Calls(
            IEnumerable<Dictionary<string, IToken>> combinations,
            string left,
            string right)
        {
            right = right.Trim();
            bool rightHasQuotes = right.StartsWith("\"") && right.EndsWith("\"");
            if(left.StartsWith("\"") && left.EndsWith("\""))
            {
                var called = _pkb.CallsDirectly[left.Replace("\"", "")];
                return combinations.Where(x =>
                {
                    var rightToken = x[right];
                    return called.Contains(rightToken.ToString());
                });
            }
            return combinations.Where(x =>
            {
                var leftToken = x[left.Trim()];
                var descendantsThatCall = leftToken.GetDescentands()
                    .OfType<CallToken>();
                if(rightHasQuotes)
                {
                    return descendantsThatCall
                        .Any(call => call.ProcedureName == right.Replace("\"",""));
                }
                return descendantsThatCall
                    .Any(call => call.ProcedureName == x[right].ToString());
            }).ToList();
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