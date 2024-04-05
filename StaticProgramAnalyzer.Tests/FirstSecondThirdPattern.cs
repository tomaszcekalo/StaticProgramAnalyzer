using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;

namespace StaticProgramAnalyzer.Tests
{
    [TestClass]
    public class FirstSecondThirdPattern
    {
        private string _program =
            @"procedure First {
        x = 2;
        z = 3;
        y = (x + 1) * y;
        call Second; }
        procedure Second {
        x = 0;
        i = 5;
        y = x + 1 * y;
        y = x + 1;
        k = x + 1 * y;
        k = x + 1;
        k = x + 1 + c;
        while x {
        d = d + 2;}
        while k {
        x = x + y;}
        if k then {
        l = l + 1;}
        else {
        l = l + 2;}
        while i {
        x = x + 2 * y;
        call Third;
        i = i - 1; }
        if x then {
        x = x + 1; }
        else {
        z = 1; 
        y = x + 1 + p + 7;}
        z = z + x + i;
        y = z + 2;
        x = x * y + z; }
        procedure Third {
        z = 5 + x + 1;
        t = x + 1;
        v = z; }";

        //----------------------------------------------
        private Parser parser;

        private string[] lines;
        private List<ParserToken> tokens;
        private KnowledgeBuilder treeBuilder;
        private ProgramKnowledgeBase pkb;
        private QueryProcessor processor;

        [TestInitialize]
        public void Initialize()
        {
            parser = new Parser();

            lines = _program.Split(Environment.NewLine);
            tokens = parser.Parse(lines);
            treeBuilder = new KnowledgeBuilder(parser);
            pkb = treeBuilder.GetPKB(tokens);
            processor = new QueryProcessor(pkb, new QueryResultProjector());
            //var pro = treeBuilder.GetProcedures(tokens);
        }

        [TestMethod]
        public void PatternTestWhileExact()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("while w;", "Select w pattern w(\"x\",_)").Equals("12"));
        }

        [TestMethod]
        public void PatternTestWhileAll()
        {
            //Act
            var result = processor.ProcessQuery("while w;", "Select w pattern w(_,_)");
            Assert.IsTrue(result.Equals("12, 14, 19"));
        }

        [TestMethod]
        public void PatternTestIfExact()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("if ifstm;", "Select ifstm pattern ifstm(\"k\",_,_)").Equals("16"));
        }

        [TestMethod]
        public void PatternTestAssignOnlyVariable()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("assign a;", "Select a pattern a(\"x\",_)").Equals("1, 5, 15, 20, 24, 29"));
        }
        [TestMethod]
        public void PatternTestAssignExactVariableNotExactExpr()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("assign a;", "Select a pattern a(\"y\",_\"x+1\"_)").Equals("3, 8, 26"));
        }

        [TestMethod]
        public void PatternTestAssignExactVariableExactExpr()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("assign a;", "Select a pattern a(\"y\",\"x+1\")").Equals("8"));
        }

        [TestMethod]
        public void PatternTestIfAll()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("if ifstm;", "Select ifstm pattern ifstm(_,_,_)").Equals("16, 23"));
        }

        [TestMethod]
        public void PatternTestAssignAll()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("assign a;", "Select a pattern a(_,_)").Equals("1, 2, 3, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 18, 20, 22, 24, 25, 26, 27, 28, 29, 30, 31, 32"));
        }

        [TestMethod]
        public void PatternTestAssignNotExactVariableNotExactExpr()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("assign a;", "Select a pattern a(_,_\"x+1\"_)").Equals("3, 8, 10, 11, 24, 26, 31"));
        }

        [TestMethod]
        public void PatternTestAssignNotExactVariableExactExpr()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("assign a;", "Select a pattern a(_,\"x+1\")").Equals("8, 10, 24, 31"));

        }
        [TestMethod]
        public void PatternTestAssignMulti()
        {
            //Act
            Assert.IsTrue(processor.ProcessQuery("assign a;", "Select a pattern a(\"y\",_\"x+1\"_) and pattern a(\"y\",_\"7\"_)").Equals("26"));
        }
    }
}