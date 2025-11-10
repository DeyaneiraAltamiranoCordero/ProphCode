using System;
using System.Collections.Generic;

namespace ProphCode.Core.AST
{
    // =========================
    //        NODO BASE
    // =========================
    public abstract class Node
    {
        public int Line { get; set; }
        public int Col { get; set; }
    }

    // =========================
    //          RAÍZ
    // =========================
    // Permite tanto funciones como sentencias top-level (scripts).
    public sealed class ProgramNode : Node
    {
        public List<FunctionDecl> Functions { get; } = new List<FunctionDecl>();
        public List<Stmt> Body { get; } = new List<Stmt>();
    }

    // =========================
    //       DECLARACIONES
    // =========================
    public sealed class FunctionDecl : Node
    {
        public string ReturnTypeName { get; set; }  // "int","dec","text","bool","silence"(void), etc.
        public string Name { get; set; }
        public List<ParamDecl> Parameters { get; } = new List<ParamDecl>();
        public BlockStmt Body { get; set; }
    }

    public sealed class ParamDecl : Node
    {
        public string TypeName { get; set; }
        public string Name { get; set; }
    }

    // =========================
    //        SENTENCIAS
    // =========================
    public abstract class Stmt : Node { }

    // Declaración:  int x;  |  prophecy int x -> 3;
    public sealed class VarDeclStmt : Stmt
    {
        public bool IsConst { get; set; }        // 'prophecy'
        public string TypeName { get; set; }     // "int","dec","text","bool","list","vec","char"
        public string Name { get; set; }
        public Expr Init { get; set; }           // puede ser null
    }

    // Asignación:  x -> 5;  ó  a[i] -> 7;
    public sealed class AssignStmt : Stmt
    {
        // Target admite VarExpr o IndexExpr (l-value general).
        public Expr Target { get; set; }
        public Expr Value { get; set; }
    }

    // Sentencia de retorno:  return expr;  (o sin expr si retorno 'silence')
    public sealed class ReturnStmt : Stmt
    {
        public Expr Value { get; set; }          // null si no hay expresión
    }

    // Sentencia de ruptura en bucles / switch
    public sealed class BreakStmt : Stmt { }

    // Bloque: { ... }
    public sealed class BlockStmt : Stmt
    {
        public List<Stmt> Statements { get; } = new List<Stmt>();
    }

    // If / Elif / Else
    public sealed class IfStmt : Stmt
    {
        public Expr Cond { get; set; }
        public BlockStmt Then { get; set; }
        public List<ElseIfClause> Elifs { get; } = new List<ElseIfClause>();
        public BlockStmt Else { get; set; }      // null si no hay else
    }

    public sealed class ElseIfClause : Node
    {
        public Expr Cond { get; set; }
        public BlockStmt Then { get; set; }
    }

    // While: eternal_loop (cond) { ... }
    public sealed class WhileStmt : Stmt
    {
        public Expr Cond { get; set; }
        public BlockStmt Body { get; set; }
    }

    // Do-While: eternal_loop_once { ... } (cond)
    public sealed class DoWhileStmt : Stmt
    {
        public BlockStmt Body { get; set; }
        public Expr Cond { get; set; }
    }

    // For ancestral: ancestral_loop (str init; end cond; igm update) { ... }
    // init puede ser VarDeclStmt o AssignStmt; update típicamente AssignStmt.
    public sealed class ForAncestralStmt : Stmt
    {
        public Stmt Init { get; set; }           // VarDeclStmt o AssignStmt
        public Expr Cond { get; set; }
        public Stmt Update { get; set; }         // AssignStmt
        public BlockStmt Body { get; set; }
    }

    // Switch: destiny_choose (expr) { path v: { ...; break; } ... hidden_path: { ... } }
    public sealed class SwitchStmt : Stmt
    {
        public Expr Expr { get; set; }
        public List<CaseClause> Cases { get; } = new List<CaseClause>();
        public BlockStmt Default { get; set; }   // null si no hay hidden_path
    }

    public sealed class CaseClause : Node
    {
        public Expr Match { get; set; }          // valor a comparar
        public BlockStmt Body { get; set; }
    }

    // Sentencia "expresión;" para permitir foo(); o reveal(...);
    public sealed class ExprStmt : Stmt
    {
        public Expr Expr { get; set; }
    }

    // =========================
    //        EXPRESIONES
    // =========================
    public abstract class Expr : Node { }

    // Literales
    public sealed class IntLitExpr : Expr { public int Value { get; set; } }
    public sealed class DecLitExpr : Expr { public double Value { get; set; } }
    public sealed class TextLitExpr : Expr { public string Value { get; set; } }
    public sealed class CharLitExpr : Expr { public char Value { get; set; } }
    public sealed class BoolLitExpr : Expr { public bool Value { get; set; } }
    public sealed class NullLitExpr : Expr { }

    // Variable
    public sealed class VarExpr : Expr
    {
        public string Name { get; set; }
    }

    // Unaria: anti expr  |  -expr
    public sealed class UnaryExpr : Expr
    {
        // Usa el lexema tal cual: "anti" o "-"
        public string Op { get; set; }
        public Expr Right { get; set; }
    }

    // Binaria: a + b, a beyone b, a and b, etc.
    public sealed class BinaryExpr : Expr
    {
        // Usa el lexema tal cual: "+","-","*","/","%","==","!=",
        // "beyone","beyoneq","under","undereq","and","or"
        public string Op { get; set; }
        public Expr Left { get; set; }
        public Expr Right { get; set; }
    }

    // Llamada:  foo(a, b, c)
    public sealed class CallExpr : Expr
    {
        public string FuncName { get; set; }
        public List<Expr> Args { get; } = new List<Expr>();
    }

    // Indexación:  a[i]  (encadenable: a[i][j])
    public sealed class IndexExpr : Expr
    {
        public Expr Target { get; set; }         // VarExpr o IndexExpr
        public Expr Index { get; set; }
    }

    // (Opcional futuro) Acceso a miembro: obj.prop o obj.metodo()
    // public sealed class MemberAccessExpr : Expr
    // {
    //     public Expr Target { get; set; }
    //     public string MemberName { get; set; }
    // }
}
