using StaticProgramAnalyzer.Parsing;

namespace StaticProgramAnalyzer.Console
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var logFilePath = "log.txt";
            //var processor = new SuperProcessor();
            //processor.LoadFromFile(args[0]);
            using (StreamWriter logFileWriter = new StreamWriter(logFilePath, append: true))
            {
                logFileWriter.WriteLine(DateTime.Now.ToString());
                logFileWriter.WriteLine($"{args.Length} arguments");

                foreach (var arg in args)
                {
                    logFileWriter.WriteLine(arg);

                }
                //logFileWriter.WriteLine(Console.ReadLine());
                System.Console.WriteLine("Ready");
                while (true)
                {
                    var declatarions = System.Console.ReadLine();
                    logFileWriter.WriteLine($"{declatarions} ");
                    var select = System.Console.ReadLine();
                    logFileWriter.WriteLine($"{select} ");
                    logFileWriter.Flush();
                    System.Console.WriteLine($"{Guid.NewGuid()}");
                }
            }
        }
        //        static void Main(string[] args)
        //        {
        //            var parser = new Parser();
        //            var sample = @"procedure First {
        //x = 2;
        //z = 3;
        //call Second; }
        //procedure Second {
        //x = 0;
        //i = 5;
        //while i {
        //x = x + 2 * y;
        //call Third;
        //i = i - 1; }
        //if x then {
        //x = x + 1; }
        //else {
        //z = 1; }
        //z = z + x + i;
        //y = z + 2;
        //x = x * y + z; }
        //procedure Third {
        //z = 5;
        //v = z; }";
        //            var sample2 = @"procedure Main {
        //x = x+y+z;
        //while i {
        //y = x+i*2; } }";
        //            var lines = sample2.Split(Environment.NewLine);
        //            var tokens = parser.Parse(lines);
        //            var treeBuilder = new TreeBuilding.TreeBuilder(parser);
        //            var tree = treeBuilder.BuildTree(tokens);
        //            foreach(var procedure in tree)
        //            {
        //                System.Console.WriteLine(procedure);
        //            }

        //            //foreach (var token in tokens)
        //            //{
        //            //    System.Console.WriteLine(token);
        //            //}
        //            System.Console.WriteLine("Hello, World!");
        //        }
    }
}
