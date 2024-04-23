using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;

namespace StaticProgramAnalyzer.Tests
{
    [TestClass]
    public class SimpleTestProc
    {
        private string _program =
            @"procedure Test {
y = a+b*c;
y = a+b;
y = b+a;
y = (a+b*c)*(e*d)*f+k;
}
";

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
            Assert.AreEqual(processor.ProcessQuery("assign a1;", "Select a1 pattern a1(_,\"(a+b*c)\")"), "1");
            Assert.AreEqual(processor.ProcessQuery("assign a1;", "Select a1 pattern a1(_,_\"(a+b*c)\"_)"), "1, 4");
        }

    }
}
