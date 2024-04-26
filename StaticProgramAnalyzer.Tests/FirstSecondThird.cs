using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;
using System.Diagnostics;

namespace StaticProgramAnalyzer.Tests
{
    [TestClass]
    public class FirstSecondThird
    {
        private readonly string _program =
            @"procedure First {
        x = 2;
        z = 3;
        call Second; }
        procedure Second {
        x = 0;
        i = 5;
        while i {
        x = x + 2 * y;
        call Third;
        i = i - 1; }
        if x then {
        x = x + 1; }
        else {
        z = 1; }
        z = z + x + i;
        y = z + 2;
        x = x * y + z; }
        procedure Third {
        z = 5;
        v = z; }";


        //----------------------------------------------
        private Parser parser;

        private string[] lines;
        private List<ParserToken> tokens;
        private KnowledgeBuilder treeBuilder;

        [TestInitialize]
        public void Initialize()
        {
            parser = new Parser();

            lines = _program.Split(Environment.NewLine);
            tokens = parser.Parse(lines);
            treeBuilder = new KnowledgeBuilder(parser);
            //var pro = treeBuilder.GetProcedures(tokens);
        }

        [TestMethod]
        public void Procedures()
        {
            //Arrange
            //Act
            var procedures = treeBuilder.GetPKB(tokens);
            //Assert
            Assert.AreEqual(3, procedures.ProceduresTree.Count);
            Assert.AreEqual("First", procedures.ProceduresTree[0].ProcedureName);
            Assert.AreEqual("Second", procedures.ProceduresTree[1].ProcedureName);
            Assert.AreEqual("Third", procedures.ProceduresTree[2].ProcedureName);
        }

        [TestMethod]
        public void SelectProcedure()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p ");
            //Assert
            Assert.AreEqual("First, Second, Third", result);
        }

        [TestMethod]
        public void SelectAssign()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a ");
            //Assert
            Assert.AreEqual("1, 2, 4, 5, 7, 9, 11, 12, 13, 14, 15, 16, 17", result);
        }

        [TestMethod]
        public void SelectVariable()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("variable v;", "Select v ");
            //Assert
            Assert.AreEqual("x, z, i, y, v", result);
        }

        [TestMethod]
        public void SelectWhile()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("while w;", "Select w ");
            //Assert
            Assert.AreEqual("6", result);
        }

        [TestMethod]
        public void CallsWithDiscardOnTheRight()
        {
            //Q1. Which procedures call at least one procedure?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that Calls(p, _)");
            //Assert
            Assert.AreEqual("First, Second", result);
        }

        [TestMethod]
        public void Calls()
        {
            //Q1. Which procedures call at least one procedure?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p, q;", "Select p such that Calls(p, q)");
            //Assert
            Assert.AreEqual("First, Second", result);
        }

        [TestMethod]
        public void CallsWithDiscardOnTheLeft()
        {
            //Q2. Which procedures are called by at least one other procedure?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure q;", "Select q such that Calls(_, q)");
            //Assert
            Assert.AreEqual("Second, Third", result);
        }

        [TestMethod]
        public void CallsTouple()
        {
            //Q3. Find all pairs of procedures p and q such that p calls q.
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p, q;", "Select <p, q> such that Calls(p, q)");
            //Assert
            Assert.AreEqual("First Second, Second Third", result);
        }
        [TestMethod]
        public void CallsStarTouple()
        {
            //Q3. Find all pairs of procedures p and q such that p calls q directly or indirectly.
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p, q;", "Select <p, q> such that Calls*(p, q)");
            //Assert
            Assert.AreEqual("First Second, First Third, Second Third", result);
        }

        [TestMethod]
        public void FindProcedureFirst()
        {
            //Q4. Find procedure named “First”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that p.procName = \"First\"");
            //Assert
            Assert.AreEqual("First", result);
        }

        [TestMethod]
        public void FindProcedureSecond()
        {
            //Q4. Find procedure named “Second”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that p.procName = \"Second\"");
            //Assert
            Assert.AreEqual("Second", result);
        }

        [TestMethod]
        public void FindProcedureThird()
        {
            //Q4. Find procedure named “Third”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that p.procName = \"Third\"");
            //Assert
            Assert.AreEqual("Third", result);
        }

        [TestMethod]
        public void FindCalledFromSecond()
        {
            //Q5. Find all procedures that are called by “Second”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p, q;", "Select q such that Calls(p, q) with p.procName=\"Second\"");
            //Assert
            Assert.AreEqual("Third", result);
        }

        [TestMethod]
        public void FindCalledFromSecondShortForm()
        {
            //Q5. Find all procedures that are called by “Second”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure q;", "Select q such that Calls(\"Second\", q) ");
            //Assert
            Assert.AreEqual("Third", result);
        }

        [TestMethod]
        public void FindCallingSecond()
        {
            //Q6. Find all procedures that call “Second” and modify the variable named "X"
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that Calls(p, \"Second\") and Modifies(p, \"x\") ");
            //Assert
            Assert.AreEqual("First", result);
        }

        [TestMethod]
        public void FindProgLine()
        {
            //Q12. Which statements contain a statement (at stmt#=”n”) that can be executed after line 10?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("prog_line n; stmt s;", "Select s such that Next* (10, n) and Parent* (s, n) ");
            //Assert
            Assert.AreEqual("10", result);
        }

        [TestMethod]
        public void FindStatementNumberIsEqualTosomeConstant()
        {
            //Q14. Find all statements whose statement number is equal to some constant.
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s; constant c;", "Select s with s.stmt# = c.value");
            //Assert
            Assert.AreEqual("1, 2, 3, 5", result);
        }

        //[TestMethod]
        //public void FindFollows10()
        //{
        //    //Q16. Find statements that follow 10:
        //    //Arrange
        //    var pkb = treeBuilder.GetPKB(tokens);
        //    var processor = new QueryProcessor(pkb, new QueryResultProjector());
        //    //Act
        //    var result = processor.ProcessQuery("prog_line n; stmt s;", "Select s such that Follows* (n, s) with n = 10");
        //    //Assert
        //    Assert.AreEqual("13, 14, 15", result);
        //}

        [TestMethod]
        public void FindPatternX()
        {
            //Patterns are specified using relational notation so they look the same as conditions in a such that clause.
            //Think about a node in the AST as a relationship among its children.
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a pattern a (\"x\", _)");
            //Assert
            Assert.AreEqual("1, 4, 7, 11, 15", result);
        }

        [TestMethod]
        public void FindModifiesX()
        {
            //The two queries below yield the same result for SIMPLE programs. Notice also that this might
            //not be the case in other languages. Why?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a such that Modifies (a, \"x\")");
            //Assert
            Assert.AreEqual("1, 4, 7, 11, 15", result);
        }

        [TestMethod] 
        public void FindBooleanCalls()
        {
            //If there is a procedure that calls some other procedure in the program, the result is TRUE;
            //otherwise, the result is FALSE.:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("", "Select BOOLEAN such that Calls (_, _)");
            //Assert
            Assert.AreEqual("true", result);
        }
        [TestMethod]
        public void FindParent7()
        {
            //If there is a procedure that calls some other procedure in the program, the result is TRUE;
            //otherwise, the result is FALSE.:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Parent(s,7)");
            //Assert
            Assert.AreEqual("6", result);
        }
        [TestMethod]
        public void FindParent8()
        {
            //If there is a procedure that calls some other procedure in the program, the result is TRUE;
            //otherwise, the result is FALSE.:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Parent(s,8)");
            //Assert
            Assert.AreEqual("6", result);
        }
        [TestMethod]
        public void FindParent9()
        {
            //If there is a procedure that calls some other procedure in the program, the result is TRUE;
            //otherwise, the result is FALSE.:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Parent(s,9)");
            //Assert
            Assert.AreEqual("6", result);
        }
        [TestMethod]
        public void FindParent11()
        {
            //If there is a procedure that calls some other procedure in the program, the result is TRUE;
            //otherwise, the result is FALSE.:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Parent(s,11)");
            //Assert
            Assert.AreEqual("10", result);
        }
        [TestMethod]
        public void FindParent12()
        {
            //If there is a procedure that calls some other procedure in the program, the result is TRUE;
            //otherwise, the result is FALSE.:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Parent(s,12)");
            //Assert
            Assert.AreEqual("10", result);
        }
        [TestMethod]
        public void StatementUsesX()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Uses(s,\"x\")");
            //Assert
            Assert.AreEqual("3, 6, 7, 10, 11, 13, 15", result);
        }
        [TestMethod]
        public void StatementUsesZ()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Uses(s,\"z\")");
            //Assert
            Assert.AreEqual("3, 6, 8, 13, 14, 15, 17", result);
        }
        [TestMethod]
        public void StatementUsesI()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Uses(s,\"i\")");
            //Assert
            Assert.AreEqual("3, 6, 9, 13", result);
        }
        [TestMethod]
        public void StatementUsesY()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Uses(s,\"y\")");
            //Assert
            Assert.AreEqual("3, 6, 7, 15", result);
        }
        [TestMethod]
        public void StatementModifiesX()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Modifies(s,\"x\")");
            //Assert
            Assert.AreEqual("1, 3, 4, 6, 7, 10, 11, 15", result);
        }
        [TestMethod]
        public void StatementModifiesY()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Modifies(s,\"y\")");
            //Assert
            Assert.AreEqual("3, 14", result);
        }
        [TestMethod]
        public void StatementModifiesZ()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Modifies(s,\"z\")");
            //Assert
            Assert.AreEqual("2, 3, 6, 8, 10, 12, 13, 16", result);
        }
        [TestMethod]
        public void StatementModifiesV()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Modifies(s,\"v\")");
            //Assert
            Assert.AreEqual("3, 6, 8, 17", result);
        }
        [TestMethod]
        public void StatementModifiesI()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s;", "Select s such that Modifies(s,\"i\")");
            //Assert
            Assert.AreEqual("3, 5, 6, 9", result);
        }

        [TestMethod]
        public void SelectPairsOfVariablesAndProceduresNamedTheSame()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("variable v; procedure p;", "Select p with p.procName = v.varName");
            //Assert
            Assert.AreEqual("none", result);
        }
        [TestMethod]
        public void S2ParentStar()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s, s2;", "Select s such that Parent*(s, s2)");
            //Assert
            Assert.AreEqual("6, 10", result);
        }
        [TestMethod]
        public void WhileIfParent()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("while w;if if;", "Select w such that Parent (if, w)");
            //Assert
            Assert.AreEqual("none", result);
        }
        [TestMethod]
        public void ThreeWhiles()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("while w1, w2, w3;", "Select <w1, w2, w3> such that Parent* (w1, w2) and Parent* (w2, w3)");
            //Assert
            Assert.AreEqual("none", result);
        }
        [TestMethod]
        public void UsesVariable()
        {
            //Arrange
            var pkb=treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result=processor.ProcessQuery("variable v; assign a;", "Select v such that Uses(a, v)");
            //Assert
            Assert.AreEqual("x, z, i, y", result);
        }
        //[TestMethod]
        //public void FollowsAllPossible()
        //{
        //    //Arrange
        //    var pkb = treeBuilder.GetPKB(tokens);
        //    var processor = new QueryProcessor(pkb, new QueryResultProjector());
        //    //Act
        //    var result = processor.ProcessQuery("stmt s1, s2;", "Select <s1, s2> such that Follows(s1,s2)");
        //    //Assert
        //    Assert.AreEqual("1 2, 2 3, 4 5, 5 6, 7 8, 8 9, 6 10, 10 13, 13 14, 14 15, 16 17", result);
        //}
        //[TestMethod]
        //public void FollowsStarAllPossible()
        //{
        //    //Arrange
        //    var pkb = treeBuilder.GetPKB(tokens);
        //    var processor = new QueryProcessor(pkb, new QueryResultProjector());
        //    //Act
        //    var result = processor.ProcessQuery("stmt s1, s2;", "Select <s1, s2> such that Follows*(s1,s2)");
        //    //Assert
        //    Assert.AreEqual("1 2, 1 3, 2 3, 4 5, 4 6, 5 6, 7 8, 7 9, 8 9, 4 10, 5 10, 6 10, 4 13, 5 13, 6 13, 10 13, 4 14, 5 14, 6 14, 10 14, 13 14, 4 15, 5 15, 6 15, 10 15, 13 15, 14 15, 16 17", result);
        //}
        //[TestMethod]
        //public void FollowsThrees()
        //{
        //    //Arrange
        //    var pkb = treeBuilder.GetPKB(tokens);
        //    var processor = new QueryProcessor(pkb, new QueryResultProjector());
        //    //Act
        //    var result = processor.ProcessQuery("stmt s1, s2, s3;", "Select <s1, s2, s3> such that Follows(s1, s2) and Follows(s2, s3)");
        //    //Assert
        //    Assert.AreEqual("1 2 3, 4 5 6, 5 6 10, 7 8 9, 6 10 13, 10 13 14, 13 14 15", result);
        //}
        [TestMethod]
        public void FollowsFours()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s1, s2, s3, s4;", "Select <s1,s2,s3,s4> such that Follows(s1,s2) and Follows(s2,s3) and Follows(s3,s4)");
            //Assert
            Assert.AreEqual("4 5 6 10, 5 6 10 13, 6 10 13 14, 10 13 14 15", result);
        }
        [TestMethod]
        public void FollowsFives()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("stmt s1, s2, s3, s4, s5;", "Select <s1,s2,s3,s4,s5> such that Follows(s1,s2) and Follows(s2,s3) and Follows(s3,s4) and Follows(s4,s5)");
            //Assert
            Assert.AreEqual("4 5 6 10 13, 5 6 10 13 14, 6 10 13 14 15", result);
        }
        //[TestMethod]
        //public void FollowsSixes()
        //{
        //    //Arrange
        //    var pkb = treeBuilder.GetPKB(tokens);
        //    var processor = new QueryProcessor(pkb, new QueryResultProjector());
        //    //Act
        //    var result = processor.ProcessQuery("stmt s1, s2, s3, s4, s5, s6;", "Select <s1,s2,s3,s4,s5,s6> such that Follows(s1,s2) and Follows(s2,s3) and Follows(s3,s4) and Follows(s4,s5) and Follows(s5,s6)");
        //    //Assert
        //    Assert.AreEqual("4 5 6 10 13 14, 5 6 10 13 14 15", result);
        //}
        //[TestMethod]
        //public void FollowsSevens()
        //{
        //    //Arrange
        //    var pkb = treeBuilder.GetPKB(tokens);
        //    var processor = new QueryProcessor(pkb, new QueryResultProjector());
        //    //Act
        //    var result = processor.ProcessQuery("stmt s1, s2, s3, s4, s5, s6, s7;", "Select <s1,s2,s3,s4,s5,s6,s7> such that Follows(s1,s2) and Follows(s2,s3) and Follows(s3,s4) and Follows(s4,s5) and Follows(s5,s6) and Follows(s6,s7)");
        //    //Assert
        //    Assert.AreEqual("4 5 6 10 13 14 15", result);
        //}
        //[TestMethod]
        //public void NextPairs()
        //{
        //    //Arrange
        //    var pkb = treeBuilder.GetPKB(tokens);
        //    var processor = new QueryProcessor(pkb, new QueryResultProjector());
        //    //Act
        //    var result = processor.ProcessQuery("stmt s1, s2;", "Select <s1,s2> such that Next(s1,s2)");
        //    //Assert
        //    Assert.AreEqual("1 2, 2 3, 4 5, 5 6, 9 6, 6 7, 7 8, 8 9, 6 10, 10 11, 10 12, 11 13, 12 13, 13 14, 14 15, 16 17", result);
        //}
        [TestMethod]
        public void FollowsT12()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a such that Follows(1, 2)");
            //Assert
            Assert.AreEqual("1, 2, 4, 5, 7, 9, 11, 12, 13, 14, 15, 16, 17", result);
        }
        [TestMethod]
        public void FollowsStar1()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a such that Follows*(1, a)");
            //Assert
            Assert.AreEqual("2", result);
        }
        [TestMethod]
        public void FollowsStar2()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a such that Follows*(a, 2)");
            //Assert
            Assert.AreEqual("1", result);
        }
        [TestMethod]
        public void ModifiesW()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that Modifies (p, \"w\")");
            //Assert
            Assert.AreEqual("none", result);
        }
        [TestMethod]
        public void ProgLine1()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("prog_line n; stmt s;", "Select s such that Follows* (n, s) with n = 8");
            //Assert
            Assert.AreEqual("9", result);
        }
        [TestMethod]
        public void ProgLine8()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            //Act
            var result = processor.ProcessQuery("prog_line n; stmt s;", "Select s such that Follows* (n, s) with n = 1");
            //Assert
            Assert.AreEqual("2, 3", result);
        }
    }
}