using ProphCode.Core.Lexing;
using ProphCode.Core.Types;
using System;
using System.Collections.Generic;

namespace ProphCode.Core.AST
{
    public abstract class Node { }

    public sealed class ProgramNode : Node
    {
        public List<Stmt> MainStatements { get; } = new();
    }

    public abstract class Stmt : Node { }

    public sealed class VarDeclStmt : Stmt
    {
        public TypeId Type { get; }
        public Token NameToken { get; }    // para línea/columna
        public Token LiteralToken { get; } // por ahora solo literal como inicializador

        public VarDeclStmt(TypeId type, Token nameTok, Token literalTok)
        {
            Type = type; NameToken = nameTok; LiteralToken = literalTok;
        }
    }
}