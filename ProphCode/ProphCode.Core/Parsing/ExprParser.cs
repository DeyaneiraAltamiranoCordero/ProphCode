using System;
using System.Collections.Generic;
using System.Globalization;
using ProphCode.Core.Lexing;
using ProphCode.Core.AST;

namespace ProphCode.Core.Parsing
{
    // Parser de EXPRESIONES con llamadas e indexación (postfijos)
    public sealed class ExprParser
    {
        private readonly IList<Token> _toks;
        private int _pos;

        public int Position => _pos;

        public ExprParser(IList<Token> tokens)
        {
            _toks = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _pos = 0;
        }

        
        public ExprParser(IList<Token> tokens, int startPos)
        {
            _toks = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _pos = startPos;
        }

        public Expr ParseExpressionOrThrowFull()
        {
            var expr = ParseExpression(0);
            if (Current.Kind != TokenKind.Eof)
                Throw($"Sobran tokens después de la expresión (en '{Current.Lexeme}').");
            return expr;
        }

        public Expr ParseExpression(int minPrec)
        {
            var left = ParseUnary();

            while (true)
            {
                // Primero, maneja postfijos (llamada e indexación) con mayor precedencia que cualquier binario
                var post = ParseOptionalPostfix(left);
                if (!ReferenceEquals(post, left))
                {
                    left = post;
                    continue; // volver a intentar más postfijos (encadenados)
                }

                // Luego, operadores binarios por precedencia
                if (!TryGetBinaryOperator(out string opLexeme, out int prec) || prec < minPrec)
                    break;

                Advance(); // consumir operador

                // asociatividad izquierda
                int nextMinPrec = prec + 1;
                var right = ParseExpression(nextMinPrec);

                left = new BinaryExpr { Op = opLexeme, Left = left, Right = right };
            }

            return left;
        }

        private Expr ParseUnary()
        {
            if (Match(TokenKind.KwAnti))
            {
                var right = ParseUnary();
                return new UnaryExpr { Op = "anti", Right = right };
            }
            if (Match(TokenKind.Minus))
            {
                var right = ParseUnary();
                return new UnaryExpr { Op = "-", Right = right };
            }
            // (opcional) unario '+'
            // if (Match(TokenKind.Plus)) return ParseUnary();

            return ParsePrimaryWithPostfix(); // Primary + posibles postfijos inmediatos
        }

        // Primary + bucle de postfijos (para soportar f(...), a[i], a[i][j], f(...)(...) si algún día)
        private Expr ParsePrimaryWithPostfix()
        {
            var expr = ParsePrimaryAtom();
            return ParsePostfixLoop(expr);
        }

        private Expr ParseOptionalPostfix(Expr expr)
            => (Current.Kind == TokenKind.LParen || Current.Kind == TokenKind.LBracket)
                ? ParsePostfixLoop(expr)
                : expr;

        private Expr ParsePostfixLoop(Expr expr)
        {
            while (true)
            {
                if (Match(TokenKind.LParen))
                {
                    var args = new List<Expr>();
                    if (Current.Kind != TokenKind.RParen)
                    {
                        do { args.Add(ParseExpression(0)); }
                        while (Match(TokenKind.Comma));
                    }
                    Expect(TokenKind.RParen, "Se esperaba ')' para cerrar la lista de argumentos.");

                    if (expr is VarExpr v)
                    {
                        var call = new CallExpr { FuncName = v.Name };
                        call.Args.AddRange(args);        // <- clave
                        expr = call;
                    }
                    else
                    {
                        Throw("Se esperaba nombre de función antes de '('.");
                    }
                    continue;
                }


                if (Match(TokenKind.LBracket))
                {
                    // indexación: a[expr]
                    var idx = ParseExpression(0);
                    Expect(TokenKind.RBracket, "Se esperaba ']' para cerrar el índice.");
                    expr = new IndexExpr { Target = expr, Index = idx };
                    continue;
                }

                break;
            }
            return expr;
        }

        private Expr ParsePrimaryAtom()
        {
            var t = Current;
            switch (t.Kind)
            {
                case TokenKind.IntLit:
                    Advance();
                    return new IntLitExpr { Value = int.Parse(t.Lexeme, CultureInfo.InvariantCulture) };

                case TokenKind.DecLit:
                    Advance();
                    return new DecLitExpr { Value = double.Parse(t.Lexeme, CultureInfo.InvariantCulture) };

                case TokenKind.TextLit:
                    Advance();
                    return new TextLitExpr { Value = t.Lexeme };

                case TokenKind.CharLit:
                    Advance();
                    if (t.Lexeme?.Length != 1)
                        Throw("Literal char mal formado.");
                    return new CharLitExpr { Value = t.Lexeme[0] };

                case TokenKind.BoolLit:
                    Advance();
                    return new BoolLitExpr { Value = string.Equals(t.Lexeme, "lumus", StringComparison.Ordinal) };

                case TokenKind.NullLit:
                    Advance();
                    return new NullLitExpr();

                // Identificadores Y ciertos keywords que actúan como nombres de función (reveal, scry, invoke)
                case TokenKind.Ident:
                case TokenKind.KwReveal:
                case TokenKind.KwScry:
                case TokenKind.KwInvoke:
                    {
                        Advance();
                        return new VarExpr { Name = t.Lexeme };
                    }

                case TokenKind.LParen:
                    Advance(); // (
                    var inner = ParseExpression(0);
                    Expect(TokenKind.RParen, "Se esperaba ')' para cerrar la expresión entre paréntesis.");
                    return inner;

                default:
                    Throw($"Se esperaba una expresión, pero llegó '{t.Kind}' (lexema '{t.Lexeme}').");
                    return null!; // unreachable
            }
        }

        // ------------ Operadores binarios y precedencias ------------
        private bool TryGetBinaryOperator(out string opLexeme, out int prec)
        {
            var t = Current;
            opLexeme = t.Lexeme;
            switch (t.Kind)
            {
                // * / %
                case TokenKind.Star:
                case TokenKind.Slash:
                case TokenKind.Percent:
                    prec = 60; return true;

                // + -
                case TokenKind.Plus:
                case TokenKind.Minus:
                    prec = 50; return true;

                // comparadores palabra: beyone/beyoneq/under/undereq
                case TokenKind.Beyone:
                case TokenKind.BeyoneEq:
                case TokenKind.Under:
                case TokenKind.UnderEq:
                    prec = 40; return true;

                // igualdad == !=
                case TokenKind.EqEq:
                case TokenKind.BangEq:
                    prec = 35; return true;

                // and / or
                case TokenKind.KwAnd:
                    prec = 30; return true;
                case TokenKind.KwOr:
                    prec = 20; return true;

                default:
                    prec = -1; return false;
            }
        }

        // ------------ utilidades de recorrido ------------
        private Token Current => _toks[_pos];

        private Token Advance()
        {
            if (_pos < _toks.Count) _pos++;
            return _toks[_pos - 1];
        }

        private bool Match(TokenKind kind)
        {
            if (Current.Kind == kind) { Advance(); return true; }
            return false;
        }

        private void Expect(TokenKind kind, string messageIfNot)
        {
            if (Current.Kind != kind)
                Throw(messageIfNot);
            Advance();
        }

        private void Throw(string message)
        {
            var t = Current;
            throw new Exception($"[Parse-Expr] {message} En {t.Line}:{t.Col}.");
        }
    }
}
