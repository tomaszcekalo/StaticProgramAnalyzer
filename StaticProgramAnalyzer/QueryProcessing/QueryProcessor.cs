using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing.Predicates;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StaticProgramAnalyzer.QueryProcessing
{
    public interface IQueryProcessor
    {
        string ProcessQuery(string declarations, string selects);

        Dictionary<string, string> GetDeclarations(string declarations);
    }

    public class QueryProcessor : IQueryProcessor
    {
        private readonly ProgramKnowledgeBase _pkb;
        private readonly IQueryResultProjector projector;
        private readonly char[] _whitespace = new char[] { ' ', '\t', '\n', '\r', ',' };

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
                value => _pkb.TokenList.Where(
                    token =>
                    value.Value.Evaluate(token)
                    ));
            if (selects.Contains('_'))
            {
                variableQueries.Add("_", _pkb.TokenList);
            }
            var variableNames = variableQueries.Keys.ToList();

            IEnumerable<Dictionary<string, IToken>> combinations = new List<Dictionary<string, IToken>>();
            if (variableNames.Count > 0)
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
                }));
            }
            //just because we can't be sure if there's no variable named "pattern"
            Regex regex = new Regex("pattern [^(]+\\([^,]+,[^,)]+(,[^)]+)?\\)");
            String withoutPattern = regex.Replace(selects, "");
            MatchCollection mc = regex.Matches(selects);
            IEnumerable<String> matches = mc.Select(x => x.Value);

            // here are conditions
            var conditionStrings = withoutPattern.Split(new string[]
            {
                " such that ",
                " with ",
                " and ",
            }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            conditionStrings.AddRange(matches);

            for (int i = 1; i < conditionStrings.Count; i++)
            {
                combinations = FilterByCondition(combinations, conditionStrings[i]);
            }
            // get only variables that are in select
            var variableToSelect = GetVariablesToSelect(selects, variablePredicates);

            if (variableToSelect.Contains("BOOLEAN"))
            {
                return projector.ProjectBoolean(combinations);
            }
            return projector.Project(combinations, variableToSelect);
        }

        public IEnumerable<Dictionary<string, IToken>> FilterByParameter(IEnumerable<Dictionary<string, IToken>> combinations, string condition)
        {
            var array = condition.Split('=');

            return combinations.Where(x => GetValueOrProperty(array[0], x) == GetValueOrProperty(array[1], x));
        }

        public string GetValueOrProperty(string selector, Dictionary<string, IToken> x)
        {
            selector = selector.Trim();
            if (int.TryParse(selector, out int result))
            {
                return result.ToString();
            }
            if (selector.StartsWith('"') && selector.EndsWith('"'))
            {
                return selector.Replace("\"", "");
            }
            var array = selector.Split('.');
            var pqlVar = array[0].Trim();
            var pqlProperty = array[1].Trim();

            if (pqlProperty == "procName")
            {
                var withProcname = x[pqlVar] as IHasProcedureName;
                return withProcname?.ProcedureName;
            }
            if (pqlProperty == "stmt#")
            {
                return (x[pqlVar] as StatementToken)?.StatementNumber.ToString();
            }
            if (pqlProperty == "varName")
            {
                var withVarName = x[pqlVar] as VariableToken;
                return withVarName?.VariableName;
            }
            if (pqlProperty == "value")
            {
                var withValue = x[pqlVar] as ConstantToken;
                return withValue?.Value.ToString();
            }
            throw new InvalidPropertyException("invalid property");
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
            if (condition.StartsWith("Next*"))
            {
                return NextStar(combinations, parametersArray[0], parametersArray[1]);
            }
            else if (condition.StartsWith("Next"))
            {
                return Next(combinations, parametersArray[0], parametersArray[1]);
            }
            if (condition.StartsWith("Calls*"))
            {
                return CallsStar(combinations, parametersArray[0], parametersArray[1]);
            }
            else if (condition.StartsWith("Calls"))
            {
                return Calls(combinations, parametersArray[0], parametersArray[1]);
            }
            if (condition.StartsWith("Follows*"))
            {
                return FollowsStar(combinations, parametersArray[0], parametersArray[1]);
            }
            else if (condition.StartsWith("Follows"))
            {
                return Follows(combinations, parametersArray[0], parametersArray[1]);
            }
            if (condition.StartsWith("Parent*"))
            {
                return ParentStar(combinations, parametersArray[0], parametersArray[1]);
            }
            else if (condition.StartsWith("Parent"))
            {
                return Parent(combinations, parametersArray[0], parametersArray[1]);
            }
            if (condition.StartsWith("Uses"))
            {
                return Uses(combinations, parametersArray[0], parametersArray[1]);
            }
            if (condition.StartsWith("Modifies"))
            {
                return Modifies(combinations, parametersArray[0], parametersArray[1]);
            }
            if (condition.StartsWith("pattern"))
            {
                var pqlVar = parameters[0].Split(' ')[1];
                if (parametersArray.Length > 2)
                {
                    return Pattern(combinations, pqlVar, parametersArray[0], parametersArray[1], parametersArray[2]);
                }
                else
                {
                    return Pattern(combinations, pqlVar, parametersArray[0], parametersArray[1]);
                }
            }

            return combinations;
        }

        public IEnumerable<Dictionary<string, IToken>> Next(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            left = left.Trim();
            right = right.Trim();
            var result = combinations.Where(x =>
            {
                var leftToken = x[left] as StatementToken;
                var rightToken = x[right] as StatementToken;
                if (leftToken != null && rightToken != null)
                {
                    return leftToken.Next.Contains(rightToken);
                }
                return false;
            });
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> NextStar(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            left = left.Trim();
            right = right.Trim();
            var result = combinations.Where(x =>
            {
                var leftToken = x[left] as StatementToken;
                var rightToken = x[right] as StatementToken;
                HashSet<StatementToken> validated = new HashSet<StatementToken>();

                if (leftToken != null && rightToken != null)
                {
                    var toValidate = leftToken.Next.ToHashSet();
                    while (toValidate.Count > 0)
                    {
                        var current = toValidate.First();
                        toValidate.Remove(current);
                        if (current == rightToken)
                        {
                            return true;
                        }
                        if (validated.Contains(current))
                        {
                            continue;
                        }
                        validated.Add(current);
                        toValidate.UnionWith(current.Next);
                    }
                }
                return false;
            });
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> Follows(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            left = left.Trim();
            right = right.Trim();
            var result = combinations.Where(x =>
            {
                var leftToken = x[left];
                var rightToken = x[right];
                if (leftToken is StatementToken leftStatement && rightToken is StatementToken rightStatement)
                {
                    return (leftToken.Parent as IDeterminesFollows).Follows(leftStatement, rightStatement);
                }
                return false;
            });
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> FollowsStar(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            left = left.Trim();
            right = right.Trim();
            var result = combinations.Where(x =>
            {
                var leftToken = x[left];
                var rightToken = x[right];
                if (leftToken is StatementToken && rightToken is StatementToken)
                {
                    var leftStatement = leftToken as StatementToken;
                    var rightStatement = rightToken as StatementToken;
                    return (leftToken.Parent as IDeterminesFollows).FollowsStar(leftStatement, rightStatement);
                }
                return false;
            });
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> Pattern(IEnumerable<Dictionary<string, IToken>> combinations, string pqlVariable, string left, string right, string rightestRight = null)
        {
            pqlVariable = pqlVariable.Trim();
            left = left.Replace("\"", "").Trim();
            right = right.Replace("\"", "").Trim();
            bool exactMatch = !(right.StartsWith("_") && right.EndsWith("_") && right.Length > 1);
            if (exactMatch == false)
            {
                right = right.Substring(1, right.Length - 2);
            }
            if (rightestRight != null)
            {
                rightestRight = rightestRight.Replace("\"", "").Trim();
            }
            Parser parser = new Parser();
            KnowledgeBuilder kb = new KnowledgeBuilder(parser);
            return combinations.Where(x =>
            {
                var token = x[pqlVariable];
                if (token is WhileToken)
                {
                    if (left == "_" || (token as WhileToken).VariableName.Equals(left))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (token is IfThenElseToken)
                {
                    if (left == "_" || (token as IfThenElseToken).VariableName.Equals(left))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (token is AssignToken)
                {
                    var at = token as AssignToken;
                    if (left == "_" && right == "_")
                    {
                        return true;
                    }
                    bool variableMatch = left == "_" || at.Left.VariableName.Equals(left);
                    if (variableMatch)
                    {
                        bool astMatch = right == "_";
                        if (astMatch == false)
                        {
                            var pqlAst = kb.BuildAssignTokenFromString(right);
                            if (exactMatch)
                            {
                                return at.EqualsTree(pqlAst);
                            }
                            else
                            {
                                return at.ContainsTree(pqlAst);
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new Exception("Unsupported token");
                }
            });
        }

        public IEnumerable<Dictionary<string, IToken>> ParentStar(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            //if second parameter is a line number
            if (int.TryParse(right, out int statementNumber))
            {
                //parents of statements at that line number
                return combinations.Where(c =>
                {
                    var children = c[left].GetDescentands();
                    return children.OfType<StatementToken>()
                        .Any(x => x.StatementNumber == statementNumber
                            && x.Parent == c[left]);
                });
            }
            left = left.Trim();
            right = right.Trim();
            var result = combinations.Where(x =>
            {
                //for all parents of right
                var parent = x[right].Parent;
                while (parent != null)
                {
                    if (parent is WhileToken || parent is IfThenElseToken)
                    {
                        if (parent == x[left])
                            return true;
                    }
                    parent = parent.Parent;
                }
                return false;
            });
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> Modifies(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            right = right.Trim().Replace("\"", "");
            var assignments = combinations.Where(x => _pkb.AllModifies[right].Contains(x[left]));
            return assignments;
        }

        public ProcedureToken GetFinalParent(IToken x, string left)
        {
            if (x.Parent is null && x is ProcedureToken)
            {
                return x as ProcedureToken;
            }
            if (x.Parent is ProcedureToken)
            {
                return x.Parent as ProcedureToken;
            }
            return GetFinalParent(x.Parent, left);
        }

        public IEnumerable<Dictionary<string, IToken>> Uses(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            right = right.Trim().Replace("\"", "");
            var result = combinations.Where(x => _pkb.AllUses[right].Contains(x[left]));
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> Parent(
            IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            left = left.Trim();
            right=right.Trim();
            //if second parameter is a line number
            if (int.TryParse(right, out int lineNumber))
            {
                //parents of statements at that line number
                return combinations.Where(c =>
                {
                    var children = c[left].GetChildren();
                    return children is null ? false : children
                        .Any(x => x.StatementNumber == lineNumber
                            && x.Parent == c[left]);
                });
            }
            var result = combinations.Where(x =>
                x[right]?.Parent == x[left]);
            return result;
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
            left = left.Trim();
            bool rightHasQuotes = right.StartsWith("\"") && right.EndsWith("\"");
            if (left.StartsWith("\"") && left.EndsWith("\""))
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
                var leftToken = x[left];
                var descendantsThatCall = leftToken.GetDescentands()
                    .OfType<CallToken>();
                if (rightHasQuotes)
                {
                    return descendantsThatCall
                        .Any(call => call.ProcedureName == right.Replace("\"", ""));
                }
                return descendantsThatCall
                    .Any(call => right == "_" || call.ProcedureName == x[right].ToString());
            });
        }

        public List<string> GetVariablesToSelect(string selects, Dictionary<string, IPredicate> variableQueries)
        {
            List<string> result = new List<string>();
            Queue<string> queue = new Queue<string>(
                selects
                .Replace(",", ", ")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
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
                    else throw new Exception($"Invalid variable name {token}");
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
            else if (type == "constant")
            {
                return new IsConstantPredicate();
            }
            return null;// TODO : throw exception or choose what to do
        }
    }
}