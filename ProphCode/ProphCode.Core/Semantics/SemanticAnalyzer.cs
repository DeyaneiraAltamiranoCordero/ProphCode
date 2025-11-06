using ProphCode.Core.AST;
using ProphCode.Core.Compiler;
using ProphCode.Core.Types;

namespace ProphCode.Core.Semantics
{
    public sealed class SemanticAnalyzer
    {
        private readonly IList<Diagnostic> _diags;
        private readonly Dictionary<string, TypeId> _locals = new(StringComparer.Ordinal);

        public SemanticAnalyzer(IList<Diagnostic> diagnostics) => _diags = diagnostics;

        public void Analyze(ProgramNode program)
        {
            _locals.Clear();

            foreach (var s in program.MainStatements)
            {
                if (s is VarDeclStmt variableDeclaration)
                {
                    // Verifica si el tipo 
                    if (!TypeHelpers.LiteralMatches(variableDeclaration.Type, variableDeclaration.LiteralToken.Kind))
                    {
                        _diags.Add(new Diagnostic(
                            Severity.Error, variableDeclaration.LiteralToken.Line, variableDeclaration.LiteralToken.Col,
                            $"Tipo incompatible: '{variableDeclaration.NameToken.Lexeme}' es {TypeHelpers.ToKeyword(variableDeclaration.Type)} " +
                            $"pero el literal no coincide."));
                    }

                    // 2) duplicado
                    if (_locals.ContainsKey(variableDeclaration.NameToken.Lexeme))
                    {
                        _diags.Add(new Diagnostic(
                            Severity.Error, variableDeclaration.NameToken.Line, variableDeclaration.NameToken.Col,
                            $"La variable '{variableDeclaration.NameToken.Lexeme}' ya fue declarada en este ámbito."));
                    }
                    else
                    {
                        _locals[variableDeclaration.NameToken.Lexeme] = variableDeclaration.Type;
                    }
                }
            }
        }
    }
}
