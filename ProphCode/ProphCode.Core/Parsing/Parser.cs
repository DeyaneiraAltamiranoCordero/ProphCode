// ProphCode.Core/Parsing/Parser.cs
using ProphCode.Core.AST;
using ProphCode.Core.Compiler;
using ProphCode.Core.Lexing;
using ProphCode.Core.Types;

namespace ProphCode.Core.Parsing
{
    public sealed class Parser
    {
        private  List<Token> tokenList;
        private  List<Diagnostic> messageList;
        private int cursor; //movernos entre tokens

        public Parser(List<Token> tokens, List<Diagnostic> diagnostics)
        {
            tokenList = tokens;
            messageList = diagnostics;
        }

        private Token CurrentToken => cursor < tokenList.Count ? tokenList[cursor] : tokenList[^1];

        private Token ConsumeToken()
        {
            if (cursor < tokenList.Count) cursor++;
            return CurrentToken;
        }

        private Token ExpectToken(TokenKind expectedKind, string message)
        {
            if (CurrentToken.Kind == expectedKind)
                return ConsumeToken();

            messageList.Add(new Diagnostic(Severity.Error, CurrentToken.Line, CurrentToken.Col, message));
            // Devolvemos un token sintético para permitir continuar el parseo
            return new Token(expectedKind, "<missing>", CurrentToken.Line, CurrentToken.Col);
        }

        private void RecoverToStatementBoundary()
        {
            // Avanza hasta ';' o hasta 'disappear' o EOF
            while (CurrentToken.Kind != TokenKind.Semi &&
                   CurrentToken.Kind != TokenKind.KwDisappear &&
                   CurrentToken.Kind != TokenKind.Eof)
            {
                ConsumeToken();
            }
            if (CurrentToken.Kind == TokenKind.Semi)
                ConsumeToken(); // consumir ';' si estaba
        }

        public ProgramNode ParseProgram()
        {
            var programNode = new ProgramNode();

            //  abracadabra ->
            ExpectToken(TokenKind.KwAbracadabra, "Se esperaba 'abracadabra' al inicio del programa.");
            ExpectToken(TokenKind.Arrow, "Se esperaba '->' después de 'abracadabra'.");

            // Dentro de abracadabra-> ... disappear
            while (CurrentToken.Kind != TokenKind.KwDisappear && CurrentToken.Kind != TokenKind.Eof)
            {
                if (TypeHelpers.TryFromKeyword(CurrentToken.Kind, out var declaredTypeId))
                {
                    var varDeclStatement = ParseVariableDeclarationStatement(declaredTypeId);
                    if (varDeclStatement != null)
                        programNode.MainStatements.Add(varDeclStatement);
                }
                else if (CurrentToken.Kind == TokenKind.Ident)
                {
                    // Identificador suelto (p. ej., "jfoajfo")
                    var orphanIdentifierToken = CurrentToken;
                    ConsumeToken();
                    messageList.Add(new Diagnostic(
                        Severity.Error, orphanIdentifierToken.Line, orphanIdentifierToken.Col,
                        $"Identificador '{orphanIdentifierToken.Lexeme}' no declarado o sentencia inválida. Solo se permiten declaraciones."));
                    RecoverToStatementBoundary();
                }
                else
                {
                    messageList.Add(new Diagnostic(
                        Severity.Error, CurrentToken.Line, CurrentToken.Col,
                        $"Token inesperado '{CurrentToken.Lexeme}'. Se esperaba una declaración de variable."));
                    RecoverToStatementBoundary();
                }
            }

            //disappear
            ExpectToken(TokenKind.KwDisappear, "Falta 'disappear' para cerrar el flujo principal.");
            return programNode;
        }

        //Para validar las declaraciones de variables con -> y el terminal ;
        private Stmt? ParseVariableDeclarationStatement(TypeId declaredTypeId)
        {
            ConsumeToken();
  
            var nameToken = ExpectToken(TokenKind.Ident, "Se esperaba el nombre de la variable.");

            // Asignador: solo '->'
            if (CurrentToken.Kind == TokenKind.Arrow)
            {
                ConsumeToken();
            }
            else
            {
                messageList.Add(new Diagnostic(
                    Severity.Error, CurrentToken.Line, CurrentToken.Col,
                    "Se esperaba '->' en la declaración."));
            }

            // Verifica que el valor sea de un tokenKind
            var initializerToken = CurrentToken;
            if (initializerToken.Kind is TokenKind.IntLit or TokenKind.DecLit or TokenKind.TextLit or TokenKind.BoolLit)
            {
                ConsumeToken();
            }
            else
            {
                messageList.Add(new Diagnostic(
                    Severity.Error, initializerToken.Line, initializerToken.Col,
                    "Se esperaba un literal después del asignador."));
            }

            // Punto y coma
            ExpectToken(TokenKind.Semi, "Falta ';' al final de la declaración.");

            return new VarDeclStmt(declaredTypeId, nameToken, initializerToken);
        }
    }
}