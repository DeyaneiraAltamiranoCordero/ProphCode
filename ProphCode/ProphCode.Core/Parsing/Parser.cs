using ProphCode.Core.AST;
using ProphCode.Core.Lexing;
using ProphCode.Core.Runtime;
using System;
using System.Collections.Generic;

namespace ProphCode.Core.Parsing
{
    public sealed class Parser
    {
        private readonly IList<Token> _toks;
        private int _pos;
        //private readonly ExprParser _expr;


        private Expr ParseExpr()
        {
            var ep = new ExprParser(_toks, _pos);   // empieza a parsear desde _pos
            var e = ep.ParseExpression(0);
            _pos = ep.Position;                   // sincroniza la posición consumida
            return e;
        }

        public Parser(IList<Token> toks)
        {
            _toks = toks ?? throw new ArgumentNullException(nameof(toks));
          
        }

        public ProgramNode ParseProgram()
        {
            var prog = new ProgramNode();

            while (!Check(TokenKind.Eof))
            {
                // 1) Funciones
                if (Match(TokenKind.KwSpell))
                {
                    prog.Functions.Add(ParseFunctionDecl());
                    continue;
                }

                // 2) Bloque principal con llaves: abracadabra { ... } disappear
                if (Match(TokenKind.KwAbracadabra))
                {
                    ParseMainBlockInto(prog); // vuelca sentencias al Body
                    continue;
                }

                // 3) Sentencias top-level normales
                prog.Body.Add(ParseStatement());
            }

            return prog;
        }

        private FunctionDecl ParseFunctionDecl()
        {
            // spell <tipo> <nombre>(<params>) -> <bloque> endSpell
            var retType = ExpectIdentOrType("Se esperaba el tipo de retorno después de 'spell'.");
            var nameTok = Expect(TokenKind.Ident, "Se esperaba el nombre de la función.");

            Expect(TokenKind.LParen, "Se esperaba '(' en la declaración de función.");
            var pars = new List<ParamDecl>();
            if (!Check(TokenKind.RParen))
            {
                do
                {
                    var tname = ExpectIdentOrType("Se esperaba tipo de parámetro.");
                    var pname = Expect(TokenKind.Ident, "Se esperaba nombre de parámetro.");
                    pars.Add(new ParamDecl { TypeName = tname, Name = pname.Lexeme, Line = pname.Line, Col = pname.Col });
                } while (Match(TokenKind.Comma));
            }
            Expect(TokenKind.RParen, "Se esperaba ')'.");
            Expect(TokenKind.Arrow, "Se esperaba '->' antes del cuerpo de la función.");

            var body = ParseBlock();

            Expect(TokenKind.KwEndSpell, "Se esperaba 'endSpell' al terminar la función.");

            var f = new FunctionDecl
            {
                ReturnTypeName = retType,
                Name = nameTok.Lexeme,
                Body = body,
                Line = nameTok.Line,
                Col = nameTok.Col
            };
            f.Parameters.AddRange(pars);
            return f;
        }
        private void ParseMainBlockInto(ProgramNode prog)
        {
            // Requerimos: abracadabra { ... } disappear
            Expect(TokenKind.LBrace, "Se esperaba '{' después de 'abracadabra'.");
            var block = ParseBlockAfterLBrace(); // ya tienes este método

            Expect(TokenKind.KwDisappear, "Se esperaba 'disappear' para cerrar el bloque principal.");

            // volcamos el contenido del bloque principal en el Body del programa
            prog.Body.AddRange(block.Statements);
        }

        private Stmt ParseStatement()
        {
            if (Match(TokenKind.LBrace)) return ParseBlockAfterLBrace();

            // if / while / do-while / return / break
            if (Match(TokenKind.KwIfSpellSay)) return ParseIf();
            if (Match(TokenKind.KwEternalLoop)) return ParseWhile();
            if (Match(TokenKind.KwEternalLoopOnce)) return ParseDoWhile();
            if (Match(TokenKind.KwReturn)) return ParseReturn();
            if (Match(TokenKind.KwBreak)) { Expect(TokenKind.Semi, "Se esperaba ';' después de break."); return new BreakStmt(); }
            if (Match(TokenKind.KwAncestralLoop)) return ParseForAncestral();
            if (Match(TokenKind.KwDestinyChoose)) return ParseSwitch();


            // var decl: [prophecy] <tipo> ident [-> expr]? ;
            if (Check(TokenKind.KwProphecy) || IsTypeAhead())
                return ParseVarDecl();

            // asignación o expr-stmt
            var expr = ParseExpr();
            // si es asignación, la próxima debe ser '->'
            if (Match(TokenKind.Arrow))
            {
                // target fue lo que parseó expr (VarExpr o IndexExpr)
                var value = ParseExpr();
                Expect(TokenKind.Semi, "Se esperaba ';' al final de la asignación.");
                return new AssignStmt { Target = expr, Value = value, Line = GetPrev().Line, Col = GetPrev().Col };
            }
            else
            {
                Expect(TokenKind.Semi, "Se esperaba ';' al final de la expresión.");
                return new ExprStmt { Expr = expr, Line = GetPrev().Line, Col = GetPrev().Col };
            }
        }

        private BlockStmt ParseBlock()
        {
            Expect(TokenKind.LBrace, "Se esperaba '{' para abrir bloque.");
            return ParseBlockAfterLBrace();
        }

        private BlockStmt ParseBlockAfterLBrace()
        {
            var b = new BlockStmt();
            while (!Check(TokenKind.RBrace))
            {
                if (Check(TokenKind.Eof)) Throw("Bloque no cerrado con '}'.");
                b.Statements.Add(ParseStatement());
            }
            Expect(TokenKind.RBrace, "Se esperaba '}' para cerrar bloque.");
            return b;
        }

        private Stmt ParseIf()
        {
            Expect(TokenKind.LParen, "Se esperaba '(' en if_spell_say.");
            var cond = ParseExpr();
            Expect(TokenKind.RParen, "Se esperaba ')' en if_spell_say.");
            var thenB = ParseBlock();

            var node = new IfStmt { Cond = cond, Then = thenB };
            // elif*
            while (Match(TokenKind.KwIfFailSay))
            {
                Expect(TokenKind.LParen, "Se esperaba '(' en if_fail_say.");
                var c = ParseExpr();
                Expect(TokenKind.RParen, "Se esperaba ')' en if_fail_say.");
                var tb = ParseBlock();
                node.Elifs.Add(new ElseIfClause { Cond = c, Then = tb });
            }
            // else?
            if (Match(TokenKind.KwIfFail))
                node.Else = ParseBlock();
            return node;
        }

        private Stmt ParseWhile()
        {
            Expect(TokenKind.LParen, "Se esperaba '(' en eternal_loop.");
            var cond = ParseExpr();
            Expect(TokenKind.RParen, "Se esperaba ')' en eternal_loop.");
            var body = ParseBlock();
            return new WhileStmt { Cond = cond, Body = body };
        }

        private Stmt ParseDoWhile()
        {
            // eternal_loop_once { ... } (cond);
            var body = ParseBlock();
            Expect(TokenKind.LParen, "Se esperaba '(' en eternal_loop_once.");
            var cond = ParseExpr();
            Expect(TokenKind.RParen, "Se esperaba ')' en eternal_loop_once.");
            Expect(TokenKind.Semi, "Se esperaba ';' al final de eternal_loop_once.");
            return new DoWhileStmt { Body = body, Cond = cond };
        }

        private Stmt ParseReturn()
        {
            // return <expr>? ;
            if (Match(TokenKind.Semi))
                return new ReturnStmt(); // sin valor

            var e = ParseExpr();
            Expect(TokenKind.Semi, "Se esperaba ';' después de return.");
            return new ReturnStmt { Value = e };
        }
        private Stmt ParseVarDecl()
        {
            bool isConst = Match(TokenKind.KwProphecy);
            var typeName = ExpectIdentOrType("Se esperaba un tipo después de 'prophecy' o al inicio de declaración.");
            var nameTok = Expect(TokenKind.Ident, "Se esperaba nombre de variable.");

            Expr init = null;
            if (Match(TokenKind.Arrow))
                init = ParseExpr();

            Expect(TokenKind.Semi, "Se esperaba ';' al final de la declaración.");
            return new VarDeclStmt
            {
                IsConst = isConst,
                TypeName = typeName,
                Name = nameTok.Lexeme,
                Init = init,
                Line = nameTok.Line,
                Col = nameTok.Col
            };
        }


        // ===== helpers =====
        private Token Current => _toks[_pos];
        private Token GetPrev() => _toks[Math.Max(0, _pos - 1)];

        private bool Match(TokenKind k)
        {
            if (Current.Kind == k) { _pos++; return true; }
            return false;
        }
        private bool Check(TokenKind k) => Current.Kind == k;

        private Token Expect(TokenKind k, string msg)
        {
            if (Current.Kind != k) Throw(msg);
            var t = Current; _pos++; return t;
        }

        private string ExpectIdentOrType(string msg)
        {
            // acepta: Ident o tipos reservados (KwInt/KwDec/KwText/KwBool/KwChar/KwList/KwVec)
            if (Check(TokenKind.Ident)) { var t = Current; _pos++; return t.Lexeme; }
            if (Check(TokenKind.KwInt) || Check(TokenKind.KwDec) || Check(TokenKind.KwText) ||
                Check(TokenKind.KwBool) || Check(TokenKind.KwChar) || Check(TokenKind.KwList) || Check(TokenKind.KwVec) ||
                Check(TokenKind.KwSilence))
            {
                var t = Current; _pos++; return t.Lexeme;
            }
            Throw(msg); return null!;
        }

        private bool IsTypeAhead()
        {
            return Check(TokenKind.KwInt) || Check(TokenKind.KwDec) || Check(TokenKind.KwText) ||
                   Check(TokenKind.KwBool) || Check(TokenKind.KwChar) || Check(TokenKind.KwList) ||
                   Check(TokenKind.KwVec);
        }

        private void Throw(string message)
        {
            var t = Current;
            throw new Exception($"[Parse] {message} En {t.Line}:{t.Col} (token '{t.Lexeme}').");
        }

        private Stmt ParseForAncestral()
        {
            // ancestral_loop (str init; end cond; igm update) { ... }
            Expect(TokenKind.LParen, "Se esperaba '(' en ancestral_loop.");

            // str init;
            Expect(TokenKind.KwStr, "Se esperaba 'str' en ancestral_loop.");
            // init puede ser VarDeclStmt o AssignStmt o ExprStmt (aceptamos asignación)
            // Reutilizamos ParseVarDecl si empieza por 'prophecy' o tipo; si no, lo tratamos como asignación/expr.
            Stmt init;
            if (Check(TokenKind.KwProphecy) || IsTypeAhead())
            {
                init = ParseVarDecl();
            }
            else
            {
                var target = ParseExpr();
                if (!Match(TokenKind.Arrow))
                    Throw("Se esperaba '->' en la inicialización del ancestral_loop.");
                var value = ParseExpr();
                Expect(TokenKind.Semi, "Se esperaba ';' tras la inicialización de ancestral_loop.");
                init = new AssignStmt { Target = target, Value = value };
            }

            // end cond;
            Expect(TokenKind.KwEnd, "Se esperaba 'end' en ancestral_loop.");
            var cond = ParseExpr();
            Expect(TokenKind.Semi, "Se esperaba ';' tras la condición de ancestral_loop.");

            // igm update)
            Expect(TokenKind.KwIgm, "Se esperaba 'igm' en ancestral_loop.");
            // actualización: fuerza asignación (x -> x + 1)
            var upTarget = ParseExpr();
            if (!Match(TokenKind.Arrow))
                Throw("Se esperaba '->' en la actualización de ancestral_loop.");
            var upValue = ParseExpr();

            Expect(TokenKind.RParen, "Se esperaba ')' para cerrar ancestral_loop.");

            var body = ParseBlock();

            return new ForAncestralStmt
            {
                Init = init,
                Cond = cond,
                Update = new AssignStmt { Target = upTarget, Value = upValue },
                Body = body
            };
        }


        private Stmt ParseSwitch()
        {
            // destiny_choose (expr) { path <valor>: { ... } ... hidden_path: { ... } }
            Expect(TokenKind.LParen, "Se esperaba '(' en destiny_choose.");
            var expr = ParseExpr();
            Expect(TokenKind.RParen, "Se esperaba ')' en destiny_choose.");

            Expect(TokenKind.LBrace, "Se esperaba '{' para abrir destiny_choose.");

            var cases = new List<CaseClause>();
            BlockStmt defBlock = null;

            while (!Check(TokenKind.RBrace))
            {
                if (Match(TokenKind.KwPath))
                {
                    // path <valor> : { ... }
                    var matchExpr = ParseExpr();
                    Expect(TokenKind.Colon, "Se esperaba ':' después de path <valor>.");
                    var body = ParseBlock();
                    cases.Add(new CaseClause { Match = matchExpr, Body = body });
                    continue;
                }
                if (Match(TokenKind.KwHiddenPath))
                {
                    // hidden_path : { ... }
                    Expect(TokenKind.Colon, "Se esperaba ':' después de hidden_path.");
                    defBlock = ParseBlock();
                    continue;
                }

                Throw("Se esperaba 'path' o 'hidden_path' dentro de destiny_choose.");
            }

            Expect(TokenKind.RBrace, "Se esperaba '}' para cerrar destiny_choose.");

            var sw = new SwitchStmt
            {
                Expr = expr,
                Default = defBlock
            };
            sw.Cases.AddRange(cases);   // ← sin Also()

            return sw;
        }




    }
}
