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
                .Select(s => s.ToString())
                .Distinct();
            var result = string.Join(", ", hits);// query result display
            return result;
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
                else if (token == "assign" || token == "variable")
                {
                    return new List<IPredicate> { new IsAssignPredicate() };
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