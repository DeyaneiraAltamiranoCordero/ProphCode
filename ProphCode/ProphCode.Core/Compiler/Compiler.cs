// ProphCode.Core/Compiler.cs
using ProphCode.Core.AST;
using ProphCode.Core.Lexing;
using ProphCode.Core.Parsing;
using ProphCode.Core.Semantics;
using System.Linq;

namespace ProphCode.Core.Compiler
{
    public sealed class Compiler
    {
        public CompileResult Compile(string source)
        {
            var result = new CompileResult();

            // 1) Lexico: mensaje de UI a tokens
            var lexer = new Lexer();
            var tokens = lexer.Tokenize(source).ToList();

            // 2) Sintactico: pasa los toquens al arbol ATS para validaciones de estructura
            var parser = new Parser(tokens, result.Diagnostics);
            ProgramNode program = parser.ParseProgram();

            // 3) Semantico: valida que el arbol ATS tenga sentido, como por ejemplo tipos compatibles
            var sema = new SemanticAnalyzer(result.Diagnostics);
            sema.Analyze(program);

            return result;
        }
    }
}
