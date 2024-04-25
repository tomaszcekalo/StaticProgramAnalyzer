using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticProgramAnalyzer.Tests
{
    [TestClass]
    public class Figures
    {
        string _program =
            @"procedure Circle {
t = 1;
a = t + 10;
d = t * a + 2;
call Triangle;
b = t + a;
call Hexagon;
b = t + a;
if t then {
k = a - d;
while c {
d = d + t;
c = d + 1; }
a = d + t; }
else {
a = d + t;
call Hexagon;
c = c - 1; }
call Rectangle; }
procedure Rectangle {
while c {
t = d + 3 * a + c;
call Triangle;
c = c + 20; }
d = t; }
procedure Triangle {
while d {
if t then {
d = t + 2; }
else {
a = t * a + d + k * b; }}
c = t + k + d; }
procedure Hexagon {
t = a + t; }";

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
        }

        [TestMethod]
        public void FollowsT1()
        {
            //Act
            Assert.AreEqual(processor.ProcessQuery("assign a;", "Select a such that Follows(1, a)"), "2");
        }
        [TestMethod]
        public void FollowsT2()
        {
            //Act
            Assert.AreEqual(processor.ProcessQuery("assign a;", "Select a such that Follows(a, 2)"), "1");
        }
    }

}
