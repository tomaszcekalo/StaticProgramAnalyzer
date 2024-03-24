using System;

namespace StaticProgramAnalyzer
{
    public class Class1
    {
        // SPA=Static Program Analyzer
        // PKB=Program Knowledge Base
        // AST=Abstract Syntax Tree
        // CFG=Control Flow Graph
        // QPS=Query Processing Subsystem
        // PQL=Program Query Language

/* Abstract syntax of SIMPLE
Meta symbols:
a* - repetition 0 or more times of a
a+ - repetition 1 or more times of a
‘|’ means or
Lexical tokens:
LETTER : A-Z | a-z -- capital or small letter
DIGIT : 0-9
NAME, VAR : LETTER (LETTER | DIGIT)*
procedure, variable and attribute names are strings of letters,
digits, starting with a letter
INTEGER : DIGIT+ -- constants are sequences of digits
//*/

/* Abstract syntax of SIMPLE
program : procedure+
procedure : stmtLst
stmtLst : stmt+
stmt : assign | call | while | if
assign : variable expr
expr : plus | minus| times | ref
plus : expr expr
minus : expr expr
times : expr expr
ref : variable | constant
while: variable stmtLst
if : variable stmtLst stmtLst
//*/

/* Concrete Syntax Grammar
(CSG)
program : procedure+
procedure : ‘procedure’ proc_name ‘{‘ stmtLst ‘}’
stmtLst : stmt+
stmt : call | while | if | assign
call : ‘call’ proc_name ‘;’
while : ‘while’ var_name ‘{‘ stmtLst ‘}’
if : ‘if’ var_name ‘then’ ‘{‘ stmtLst ‘}’ ‘else’ ‘{‘ stmtLst‘}’
assign : var_name ‘=’ expr ‘;’
expr : expr ‘+’ term | expr ‘-’ term | term
term : term ‘*’ factor | factor
factor : var_name| const_value |‘(’expr‘)’
var_name : NAME
proc_name : NAME
const_value : INTEGER
//*/

/* Abstract Syntax Grammar
(ASG)
program : procedure+
procedure : stmtLst
stmtLst : stmt+
stmt : assign | call | while | if
assign : variable expr
expr : plus | minus| times |
ref
plus : expr expr
minus : expr expr
times : expr expr
ref : variable | constant
while: variable stmtLst
if : variable stmtLst stmtLst
//*/
}
}
