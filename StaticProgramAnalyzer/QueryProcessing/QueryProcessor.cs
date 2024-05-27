using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing.Predicates;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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
        private static Dictionary<String, AssignToken> astDict = new Dictionary<string, AssignToken>();
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
            //Regex regex = new Regex("pattern [^(]+\\([^,]+,[^,)]+(,[^)]+)?\\)");
            //Regex regex = new Regex("pattern [^(]+\\([ ]*((_?(\\\"?[^\\\"?]+\\\"?)?_?))[ ]*,([ ]*((_?(\\\"?[^\\\"?]+\\\"?)?_?))[ ]*,?[ ]*)+[ ]*\\)");
            String withoutPattern = selects;
            List<String> matches = new List<String>();
            while (withoutPattern.IndexOf("pattern") >= 0)
            {
                string pat = "pattern";
                int patternStart = withoutPattern.IndexOf(pat);
                int indexNow = patternStart + pat.Length;
                int bracketsOpen = 0;
                while ((withoutPattern.ElementAt(indexNow) == ')' && bracketsOpen == 1) == false)
                {
                    if (withoutPattern.ElementAt(indexNow) == '(')
                    {
                        ++bracketsOpen;
                    }
                    else if (withoutPattern.ElementAt(indexNow) == ')')
                    {
                        --bracketsOpen;
                    }
                    ++indexNow;
                }
                matches.Add(withoutPattern.Substring(patternStart, indexNow - patternStart + 1));
                withoutPattern = withoutPattern.Remove(patternStart, indexNow - patternStart + 1);
            }
            /*
            String withoutPattern = regex.Replace(selects, "");
            MatchCollection mc = regex.Matches(selects);
            IEnumerable<String> matches = mc.Select(x => x.Value);
            */

            // here are conditions
            // there's no pattern already
            var conditionStrings = withoutPattern
                .Replace(" pattern ", " and pattern ")
                .Split(new string[]
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
                combinations = FilterByCondition(combinations, conditionStrings[i], variablePredicates);
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
            foreach (var x in combinations)
            {
                var zero = GetValueOrProperty(array[0], x);
                var one = GetValueOrProperty(array[1], x);
                var areEqual = zero == one;
            }
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
            if (array.Length == 1)
                return (x[pqlVar] as StatementToken).StatementNumber.ToString();
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
            string condition,
            Dictionary<string, IPredicate> variablePredicates)
        {
            int startB = condition.IndexOf('(');
            int stopB = condition.LastIndexOf(')');
            //var parameters2 = condition.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            String[] parameters;
            if (startB >= 0 && stopB >= 0)
            {
                parameters = new[] { condition.Substring(0, startB), condition.Substring(startB + 1, (stopB - startB - 1)) };//, condition.Substring(stopB+1)};
            }
            else
            {
                parameters = new[] { condition };
            }
            var parametersArray = parameters.Last().Split(',', StringSplitOptions.RemoveEmptyEntries);
            parametersArray[0] = parametersArray[0].Trim();
            if (parametersArray.Length > 1)
                parametersArray[1] = parametersArray[1].Trim();

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
            if (left == "_" && right == "_")
            {
                return combinations;//todo: add proper checks
            }
            if (int.TryParse(left, out int leftValue))
            {
                var leftToken = _pkb.TokenList.OfType<StatementToken>()
                    .FirstOrDefault(x => x.StatementNumber == leftValue);
                if (leftToken is null)
                    return new List<Dictionary<string, IToken>>();
                return combinations.Where(x =>
                {
                    var rightToken = x[right] as StatementToken;
                    if (leftToken != null && rightToken != null)
                    {
                        return leftToken.Next.Contains(rightToken);
                    }
                    return false;
                });
            }
            if (int.TryParse(right, out int rightValue))
            {
                var rightToken = _pkb.TokenList.OfType<StatementToken>()
                    .FirstOrDefault(x => x.StatementNumber == rightValue);
                return combinations.Where(x => x[left] is StatementToken st && st.Next.Contains(rightToken));
            }
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
            if (int.TryParse(left, out int leftValue))
            {
                var leftToken = _pkb.TokenList.OfType<StatementToken>()
                    .FirstOrDefault(x => x.StatementNumber == leftValue);
                if (leftToken is null)
                    return new List<Dictionary<string, IToken>>();
                return combinations.Where(x =>
                {
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
            }
            if (int.TryParse(right, out int rightValue))
            {
                return combinations.Where(x =>
                {
                    var leftToken = x[left] as StatementToken;
                    HashSet<StatementToken> validated = new HashSet<StatementToken>();

                    if (leftToken != null)
                    {
                        var toValidate = leftToken.Next.OfType<StatementToken>().ToHashSet();
                        while (toValidate.Count > 0)
                        {
                            var current = toValidate.First();
                            toValidate.Remove(current);
                            if (current.StatementNumber == rightValue)
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
            }
            return combinations.Where(x =>
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
        }

        public IEnumerable<Dictionary<string, IToken>> Follows(IEnumerable<Dictionary<string, IToken>> combinations, string left, string right)
        {
            if (left == "_" && right == "_")
                return combinations;
            bool isRightNumber = int.TryParse(right, out int rightNumber);
            bool isLeftNumber = int.TryParse(left, out int leftNumber);
            if (isLeftNumber && !isRightNumber)
            {
                //get right tokens which are followed by token with statement number equal to lineNumber
                return combinations.Where(x =>
                {
                    var rightToken = x[right];
                    if (rightToken is StatementToken rightStatement)
                    {
                        return (rightToken.Parent as IDeterminesFollows).Follows(leftNumber, rightStatement);
                    }
                    return false;
                });
            }
            if (isRightNumber && !isLeftNumber)
            {
                //get left tokens which are following token with statement number equal to lineNumber
                return combinations.Where(x =>
                {
                    var leftToken = x[left];
                    if (leftToken is StatementToken leftStatement)
                    {
                        return (leftToken.Parent as IDeterminesFollows).Follows(leftStatement, rightNumber);
                    }
                    return false;
                });
            }
            if (isRightNumber && isLeftNumber)
            {
                if (_pkb.TokenList.OfType<IDeterminesFollows>().Any(x => x.Follows(leftNumber, rightNumber)))
                {
                    return combinations;
                }
                return new List<Dictionary<string, IToken>>();
            }
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
            bool isRightNumber = int.TryParse(right, out int rightNumber);
            bool isLeftNumber = int.TryParse(left, out int leftNumber);

            if (isLeftNumber && !isRightNumber)
            {
                //get right tokens which are followed by token with statement number equal to lineNumber
                return combinations.Where(x =>
                {
                    var rightToken = x[right];
                    if (rightToken is StatementToken rightStatement)
                    {
                        return (rightToken.Parent as IDeterminesFollows).FollowsStar(leftNumber, rightStatement);
                    }
                    return false;
                });
            }
            if (isRightNumber && !isLeftNumber)
            {
                //get left tokens which are following token with statement number equal to lineNumber
                return combinations.Where(x =>
                {
                    var leftToken = x[left];
                    if (leftToken is StatementToken leftStatement)
                    {
                        return (leftToken.Parent as IDeterminesFollows).FollowsStar(leftStatement, rightNumber);
                    }
                    return false;
                });
            }
            if (isRightNumber && isLeftNumber)
            {
                if (_pkb.TokenList.OfType<IDeterminesFollows>().Any(x => x.FollowsStar(leftNumber, rightNumber)))
                {
                    return combinations;
                }
                return new List<Dictionary<string, IToken>>();
            }
            var result = combinations.Where(x =>
            {
                var leftToken = x[left];
                var rightToken = x[right];
                if (leftToken is StatementToken leftStatement && rightToken is StatementToken rightStatement)
                {
                    return (leftToken.Parent as IDeterminesFollows).FollowsStar(leftStatement, rightStatement);
                }
                return false;
            });
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> Pattern(IEnumerable<Dictionary<string, IToken>> combinations, string pqlVariable, string left, string right, string rightestRight = null)
        {
            pqlVariable = pqlVariable.Trim();

            left = left.Replace(" ", "");
            right = right.Replace(" ", "");

            if (left.IndexOf('\"') == -1 && left!="_")
            {
                var comb = combinations.First();
                if (comb != null)
                {
                    left = (comb[left] as VariableToken).VariableName;
                    //comb[left]
                    ///["left"].VariableName;
                }
            }

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
                    return (left == "_" || (token as WhileToken).VariableName.Equals(left));
                }
                else if (token is IfThenElseToken)
                {
                    return (left == "_" || (token as IfThenElseToken).VariableName.Equals(left));
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
                            AssignToken pqlAst;
                            if (astDict.TryGetValue(right, out pqlAst) == false)
                            {
                                pqlAst = kb.BuildAssignTokenFromString(right);
                                astDict.TryAdd(right, pqlAst);
                            }
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
            if (left == "_" && right == "_")
                return combinations;
            //if second parameter is a line number
            bool isRightNumber = int.TryParse(right, out int rightStatementNumber);
            bool isLeftNumber = int.TryParse(left, out int leftStatementNumber);
            if (isLeftNumber && !isRightNumber)
            {
                //parents of statements at that line number
                return combinations.Where(x =>
                {
                    var parent = x[right].Parent;
                    while (parent != null)
                    {
                        if (parent is WhileToken || parent is IfThenElseToken)
                        {
                            if (parent is StatementToken st && st.StatementNumber == leftStatementNumber)
                                return true;
                        }
                        parent = parent.Parent;
                    }
                    return false;
                });
            }
            else if (isRightNumber && !isLeftNumber)
            {
                return combinations.Where(c =>
                {
                    return c[left].GetDescentands().Any(x => x is StatementToken st && st.StatementNumber == rightStatementNumber);
                });
            }
            else if (isLeftNumber && isRightNumber)
            {
                return combinations;//TODO
            }
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

        public IEnumerable<Dictionary<string, IToken>> Modifies(
            IEnumerable<Dictionary<string, IToken>> combinations,
            string left,
            string right)
        {
            bool isRightNumber = int.TryParse(right, out int rightLineNumber);
            bool isLeftNumber = int.TryParse(left, out int leftLineNumber);
            bool isLeftText = left.StartsWith("\"");
            bool isRightText = right.StartsWith("\"");
            right = right.Replace("\"", "");
            left = left.Replace("\"", "");

            if (isLeftNumber && !isRightNumber)
            {
                return combinations
                    .Where(x => _pkb.AllModifies.ContainsKey(isRightText ? right : x[right].ToString()))
                    .Where(x => _pkb.AllModifies[isRightText ? right : x[right].ToString()].Any(s => s is StatementToken st && st.StatementNumber == leftLineNumber));
            }
            else if (isLeftText)
            {
                return combinations
                    .Where(x => _pkb.AllModifies.ContainsKey(isRightText ? right : x[right].ToString()))
                    .Where(x => _pkb.AllModifies[isRightText ? right : x[right].ToString()].Any(s => s is IHasProcedureName pn && pn.ProcedureName == left));
            }
            right = right.Replace("\"", "");
            var assignments = combinations
                .Where(x => _pkb.AllModifies.ContainsKey(isRightText ? right : x[right].ToString()))
                .Where(x => _pkb.AllModifies[isRightText ? right : x[right].ToString()].Contains(x[left]));
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

        public IEnumerable<Dictionary<string, IToken>> Uses(
            IEnumerable<Dictionary<string, IToken>> combinations,
            string left,
            string right)
        {
            bool isRightText = right.StartsWith('"') && right.EndsWith('"');
            bool isLeftText = left.StartsWith('"') && left.EndsWith('"');
            bool isLeftNumber = int.TryParse(left, out int leftStatementNumber);
            right = right.Replace("\"", "");
            left = left.Replace("\"", "");

            if (isLeftNumber)
            {
                return combinations.Where(x =>
                {
                    var variableName = isRightText ? right : (x[right] as VariableToken).VariableName;

                    if (_pkb.AllUses.ContainsKey(variableName))
                        return _pkb.AllUses[variableName].Any(c => c is StatementToken st && st.StatementNumber == leftStatementNumber);

                    return false;
                });
            }

            if (isRightText)
            {
                if (isLeftText)
                {
                    if (_pkb.AllUses.ContainsKey(right) && _pkb.AllUses[right].OfType<IHasProcedureName>().Any(x => x.ProcedureName == left))
                        return combinations;
                    return new List<Dictionary<string, IToken>>();
                }
                if (!_pkb.AllUses.ContainsKey(right))
                    return new List<Dictionary<string, IToken>>();
                var result = combinations.Where(x => _pkb.AllUses[right].Contains(x[left]));
                return result;
            }
            if (isLeftText)
            {
                return combinations.Where(x => x[right] is VariableToken vt
                && _pkb.AllUses.ContainsKey(vt.VariableName)
                && _pkb.AllUses[vt.VariableName].OfType<IHasProcedureName>().Any(a => a.ProcedureName == left));
            }
            return combinations.Where(x =>
            {
                if (x[right] is VariableToken vt)
                {
                    if (_pkb.AllUses.ContainsKey(vt.VariableName))
                        return _pkb.AllUses[vt.VariableName].Contains(x[left]);
                }
                return false;
            });
        }

        public IEnumerable<Dictionary<string, IToken>> Parent(
            IEnumerable<Dictionary<string, IToken>> combinations,
            string left,
            string right)
        {
            //if second parameter is a line number
            if (left == "_" && right == "_")
                return combinations;
            bool isRightNumber = int.TryParse(right, out int rightLineNumber);
            bool isLeftNumber = int.TryParse(left, out int leftLineNumber);
            if (isLeftNumber && !isRightNumber)
            {
                return combinations.Where(c =>
                {
                    if (c[right].Parent is not null && c[right].Parent is StatementToken st)
                        return st.StatementNumber == leftLineNumber;
                    return false;
                });
            }
            else if (!isLeftNumber && isRightNumber)
            {
                return combinations.Where(c =>
                {
                    var children = c[left].GetChildren();
                    return children is null ? false : children
                        .Any(x => x.StatementNumber == rightLineNumber
                            && x.Parent == c[left]);
                });
            }
            else if (isLeftNumber && isRightNumber)
            {
                if (_pkb.TokenList.Any(x => x is StatementToken st
                && st.StatementNumber == leftLineNumber
                && st.Parent is not null
                && st.Parent is StatementToken parentStatement
                && parentStatement.StatementNumber == rightLineNumber))
                {
                    return combinations;
                }
                return new List<Dictionary<string, IToken>>();
            }

            var result = combinations.Where(x => x[right]?.Parent == x[left])
                .Where(x => x[left] is WhileToken || x[left] is IfThenElseToken);
            return result;
        }

        public IEnumerable<Dictionary<string, IToken>> CallsStar(
            IEnumerable<Dictionary<string, IToken>> combinations,
            string left,
            string right)
        {
            if (left == "_" && right == "_")
            {
                if(_pkb.AllCalls.Any())
                {
                    return combinations;
                }
                return new List<Dictionary<string, IToken>>();
            }

                bool rightHasQuotes = right.StartsWith('"') && right.EndsWith('"');
            bool leftHasQuotes = left.StartsWith('"') && left.EndsWith('"');
            right = right.Replace("\"", "");
            left = left.Replace("\"", "");

            return combinations.Where(x =>
            {
                var leftProcName = leftHasQuotes ? left : (x[left] as IHasProcedureName)?.ProcedureName;
                var rightProcName = rightHasQuotes ? right : (x[right] as IHasProcedureName)?.ProcedureName;
                if (leftProcName is not null && rightProcName is not null)
                {
                    return _pkb.AllCalls[leftProcName].Contains(rightProcName);

                }
                return false;
            });
        }

        public IEnumerable<Dictionary<string, IToken>> Calls(
            IEnumerable<Dictionary<string, IToken>> combinations,
            string left,
            string right)
        {
            bool rightHasQuotes = right.StartsWith('"') && right.EndsWith('"');
            bool leftHasQuotes = left.StartsWith('"') && left.EndsWith('"');
            right = right.Replace("\"", "");
            left = left.Replace("\"", "");

            if (leftHasQuotes)
            {
                if (rightHasQuotes)
                {
                    if (_pkb.CallsDirectly[left].Contains(right))
                        return combinations;
                    return new List<Dictionary<string, IToken>>();
                }
                var called = _pkb.CallsDirectly[left];
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
                        .Any(call => call.ProcedureName == right);
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
            else if (type == "prog_line")
            {
                return new IsStatementPredicate();
            }
            else if (type == "call")
            {
                return new IsCallPredicate();
            }
            throw new Exception("Unrecognized type!");
        }
    }
}