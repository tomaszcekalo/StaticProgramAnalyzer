using StaticProgramAnalyzer.QueryProcessing;

namespace StaticProgramAnalyzer.Tests
{
    [TestClass]
    public class QueryProcessorUnitTests
    {
        [TestMethod]
        public void GetProcedure()
        {
            //Arrange
            var testedSystem = new QueryProcessor(null);
            //Act
            var result = testedSystem.GetDeclarations("Procedure p;");
            //Assert
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result["p"] == "Procedure");
        }

        [TestMethod]
        public void GetProceduresTogether()
        {
            //Arrange
            var testedSystem = new QueryProcessor(null);
            //Act
            var result = testedSystem.GetDeclarations("Procedure p1, p2;");
            //Assert
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result["p1"] == "Procedure");
            Assert.IsTrue(result["p2"] == "Procedure");
        }

        [TestMethod]
        public void GetProceduresSeperate()
        {
            //Arrange
            var testedSystem = new QueryProcessor(null);
            //Act
            var result = testedSystem.GetDeclarations("Procedure p1;Procedure p2;");
            //Assert
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result["p1"] == "Procedure");
            Assert.IsTrue(result["p2"] == "Procedure");
        }

        [TestMethod]
        public void GetTypesGrouped()
        {
            //Arrange
            var testedSystem = new QueryProcessor(null);
            //Act
            var result = testedSystem.GetDeclarations("Procedure p1, p2;Assign a1, a2;");
            //Assert
            Assert.IsTrue(result.Count == 4);
            Assert.IsTrue(result["p1"] == "Procedure");
            Assert.IsTrue(result["p2"] == "Procedure");
            Assert.IsTrue(result["a1"] == "Assign");
            Assert.IsTrue(result["a2"] == "Assign");
        }
    }
}