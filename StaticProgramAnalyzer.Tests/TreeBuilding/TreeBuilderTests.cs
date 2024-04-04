using Microsoft.VisualStudio.TestTools.UnitTesting;
using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;
using StaticProgramAnalyzer.Tokens;
using StaticProgramAnalyzer.TreeBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace StaticProgramAnalyzer.TreeBuilding.Tests
{
    [TestClass()]
    public class TreeBuilderTests
    {
        [TestMethod()]
        public void BuildAssignmentStatementTest()
        {
            string testProgram = @"procedure Test {
y = a+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+(((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k-((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g))+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k;
}";
            var parser = new Parser();
            var tokens = parser.Parse(testProgram.Split("\r\n"));
            var tb = new KnowledgeBuilder(parser);
            var pkb = tb.GetPKB(tokens);

            var at = (pkb.ProceduresTree[0].StatementList[0] as AssignToken);
            Assert.AreEqual(at.Right.TestValue, 11972706660);
        }
        struct AssigmentCheck
        {
            public int numberOfassigment;
            public List<String> trueTree;
            public List<String> falseTree;
        };

        [TestMethod()]
        public void PatternCheckingTest()
        {
            string testProgram = @"procedure Test {
y = a+b*c;
y = a+b;
y = b+a;
y = (a+b*c)*(e*d)*f+k;
}";
            var parser = new Parser();
            var tokens = parser.Parse(testProgram.Split("\r\n"));
            var tb = new KnowledgeBuilder(parser);
            var pkb = tb.GetPKB(tokens);
            var assigmentList = pkb.ProceduresTree[0].StatementList.OfType<AssignToken>().ToList();
        
        List<AssigmentCheck> assigmentChecks = new List<AssigmentCheck>()
            {
                new AssigmentCheck
                {
                    numberOfassigment = 0,
                    trueTree = new List<string> { "a+b*c", "b*c", "a", "b", "c" },
                    falseTree = new List<string> { "a+b" }
                },
                new AssigmentCheck
                {
                    numberOfassigment = 1,
                    trueTree = new List<string> { "a", "b", "a+b" },
                    falseTree = new List<string> { "b+a" }
                },
                new AssigmentCheck
                {
                    numberOfassigment = 2,
                    trueTree = new List<string> { "a", "b", "b+a" },
                    falseTree = new List<string> { "a+b" }
                },
                new AssigmentCheck
                {
                    numberOfassigment = 3,
                    trueTree = new List<string> { "a", "b", "c", "d", "e", "f", "b*c", "a+b*c", "(a+b*c)", "e*d", "(a+b*c)*(e*d)*f" },
                    falseTree = new List<string> { "(e*d)*f", "b*c*e", "d*f" }
                }
            };

            foreach(var asch in assigmentChecks)
            {
                foreach (var tree in asch.trueTree) {
                    Assert.IsTrue(
                        assigmentList[asch.numberOfassigment].ContainsTree(
                            tb.BuildAssignmentStatement(new ProcedureToken(), new ParserToken() { Content= "y" }, 
                                new Queue<ParserToken>(parser.Parse(["=" + tree + ";"]))
                            ) as AssignToken
                        )
                    );
                }
                foreach (var tree in asch.falseTree)
                {
                    Assert.IsFalse(
                        assigmentList[asch.numberOfassigment].ContainsTree(
                            tb.BuildAssignmentStatement(new ProcedureToken(), new ParserToken() { Content = "y" },
                                new Queue<ParserToken>(parser.Parse(["=" + tree + ";"]))
                            ) as AssignToken
                        )
                    );
                }
            }
            QueryResultProjector qrp = new QueryResultProjector();
            QueryProcessor qp = new QueryProcessor(pkb, qrp);
            Assert.IsTrue(qp.ProcessQuery("stmt a1", "Select a1 such that Uses(a1, \"k\")")=="4");
            Assert.IsFalse(qp.ProcessQuery("stmt a1", "Select a1 such that Uses(a1, \"k\")") == "1");
        }

    }
}