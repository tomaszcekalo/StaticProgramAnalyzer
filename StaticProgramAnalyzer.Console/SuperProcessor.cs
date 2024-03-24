
//namespace StaticProgramAnalyzer.Console
//{
//    public class SuperProcessor
//    {
//        private string[] _lines;

//        public SuperProcessor()
//        {
//        }

//        public void LoadFromFile(string filename)
//        {
//            if (File.Exists(filename))
//            {
//                string text = File.ReadAllText(filename);
//                LoadFromString(text);
//            }
//        }

//        public void LoadFromString(string str)
//        {
//            this._lines = str.Split(System.Environment.NewLine);

//            for (int i = 0; i < this._lines.Length; i++)
//            {
//                if (this._lines[i].Contains("procedure"))
//                {
//                    Procedures.Add(ProcessProcedure(i));
//                }
//            }
//        }

//        private List<CFGNode> Procedures = new List<CFGNode>();

//        public CFGNode ProcessProcedure(int i)
//        {
//            string procedureName = _lines[i]
//                .Replace("procedure", "")
//                .Replace("{", "")
//                .Trim();
//            return ProcessNested(i + 1);
//        }

//        public CFGNode ProcessNested(int start)
//        {
//            int i = start;
//            List<int> linesOfCode = new List<int>();
//            while (true)
//            {
//                if (_lines[i].Contains("while")
//                    || _lines[i].Contains("if")
//                    || _lines[i].Contains("else"))
//                {
//                    Console.WriteLine(String.Join(",", linesOfCode));
//                    Console.WriteLine($"Start: {start} Line {i} is: {_lines[i]}");
//                    return new CFGNode()
//                    {
//                        LinesOfCode = linesOfCode,
//                        EndsOn = linesOfCode.Last(),
//                        Children = new List<CFGNode>()
//                        {
//                            new CFGNode()
//                            {
//                                LinesOfCode=new List<int> {i},
//                                EndsOn = i
//                            },
//                            ProcessNested(i+1)
//                        }
//                    };
//                }
//                linesOfCode.Add(i);
//                if (this._lines[i].TrimEnd().EndsWith('}'))
//                {
//                    Console.WriteLine(String.Join(",", linesOfCode));
//                    Console.WriteLine($"Start: {start} Line {i} is: {_lines[i]}");
//                    return new CFGNode()
//                    {
//                        LinesOfCode = linesOfCode,
//                        EndsOn = i
//                    };
//                }
//                i++;
//            }
//        }

//        /*
//         * means it's 0 or more
//         + means it's 1 or more
//program : procedure+
//procedure : stmtLst
//stmtLst : stmt+
//stmt : assign | call | while | if
//assign : variable expr
//expr : plus | minus| times | ref
//plus : expr expr
//minus : expr expr
//times : expr expr
//ref : variable | constant
//while: variable stmtLst
//if : variable stmtLst stmtLst
//        //*/
//    }
//}