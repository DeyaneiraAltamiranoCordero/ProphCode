using System;
using System.Collections.Generic;

namespace ProphCode.Core.AST
{
    public abstract class Node
    {
        public int Line { get; set; }
        public int Col { get; set; }
    }

    public sealed class ProgramNode : Node
    {
        public List<FunctionDecl> Functions { get; } = new List<FunctionDecl>();
        public List<Stmt> Body { get; } = new List<Stmt>();
    }
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

    //        Sentencias

    public abstract class Stmt : Node { }

    public sealed class VarDeclStmt : Stmt
    {
        public bool IsConst { get; set; }       
        public string TypeName { get; set; }     // "int","dec","text","bool","list","vec","char"
        public string Name { get; set; }
        public Expr Init { get; set; }           //null
    }

    public sealed class AssignStmt : Stmt
    {

        public Expr Target { get; set; }
        public Expr Value { get; set; }
    }


    public sealed class ReturnStmt : Stmt
    {
        public Expr Value { get; set; }          
    }

    
    public sealed class BreakStmt : Stmt { }

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

    // While
    public sealed class WhileStmt : Stmt
    {
        public Expr Cond { get; set; }
        public BlockStmt Body { get; set; }
    }

    // Do-While
    public sealed class DoWhileStmt : Stmt
    {
        public BlockStmt Body { get; set; }
        public Expr Cond { get; set; }
    }

    // For
    public sealed class ForAncestralStmt : Stmt
    {
        public Stmt Init { get; set; }         
        public Expr Cond { get; set; }
        public Stmt Update { get; set; }         // AssignStmt
        public BlockStmt Body { get; set; }
    }

    // Switch
    public sealed class SwitchStmt : Stmt
    {
        public Expr Expr { get; set; }
        public List<CaseClause> Cases { get; } = new List<CaseClause>();
        public BlockStmt Default { get; set; } 
    }

    public sealed class CaseClause : Node
    {
        public Expr Match { get; set; }          // valor a comparar
        public BlockStmt Body { get; set; }
    }

    public sealed class ExprStmt : Stmt
    {
        public Expr Expr { get; set; }
    }

    public abstract class Expr : Node { }

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


    public sealed class UnaryExpr : Expr
    {
        public string Op { get; set; }
        public Expr Right { get; set; }
    }

    // Operación: a + b, a beyone b, a and b, etc.
    public sealed class BinaryExpr : Expr
    {
        public string Op { get; set; }
        public Expr Left { get; set; }
        public Expr Right { get; set; }
    }

    public sealed class CallExpr : Expr
    {
        public string FuncName { get; set; }
        public List<Expr> Args { get; } = new List<Expr>();
    }

    public sealed class IndexExpr : Expr
    {
        public Expr Target { get; set; }        
        public Expr Index { get; set; }
    }

}
