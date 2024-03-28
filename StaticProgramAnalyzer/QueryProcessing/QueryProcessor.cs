using StaticProgramAnalyzer.QueryProcessing.Predicates;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var predicates = GetPredicates(declarationDictionary, selects);
            var hits = _pkb.ProceduresTree
                .Concat(_pkb.ProceduresTree.SelectMany(p => p.GetChildren()))
                .Where(s => predicates.All(p => p.Evaluate(s)))
                .OrderBy(x => x.Source.LineNumber)
                .Select(s => s.ToString())
                .Distinct();
            var result = string.Join(", ", hits);// query result display
            return result;
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
                    token = token.Replace("<", "").Replace(">", "");

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

        private List<IPredicate> GetPredicates(Dictionary<string, string> declarationDictionary, string selects)
        {
            Queue<string> queue = new Queue<string>(
                selects.Split(' ',
                StringSplitOptions.RemoveEmptyEntries));

            var token = queue.Dequeue();
            if (token == "Select")
            {
                token = declarationDictionary[queue.Dequeue()];
                if (token == "procedure")
                {
                    return new List<IPredicate> { new IsProcedurePredicate() };
                }
                else if (token == "stmt")
                {
                    return new List<IPredicate> { new IsStatementPredicate() };
                }
                else if (token == "variable")
                {
                    return new List<IPredicate> { new IsVariablePredicate() };
                }
                else if (token == "while")
                {
                    return new List<IPredicate> { new IsWhilePredicate() };
                }
                else if (token == "if")
                {
                    return new List<IPredicate> { new IsIfTheElsePredicate() };
                }
                else if (token == "assign")
                {
                    return new List<IPredicate> { new IsAssignPredicate() };
                }
                else if (token == "while")
                {
                    return new List<IPredicate> { new IsWhilePredicate() };
                }
            }
            return new List<IPredicate>();
        }
    }
}