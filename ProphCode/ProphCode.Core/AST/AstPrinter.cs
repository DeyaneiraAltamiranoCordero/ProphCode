using System.Text;

namespace ProphCode.Core.AST
{
    public static class AstPrinter
    {
        public static string Print(ProgramNode program)
        {
            var sb = new StringBuilder();
            PrintProgram(program, sb, 0);
            return sb.ToString();
        }

        private static void PrintProgram(ProgramNode p, StringBuilder sb, int ind)
        {
            Ind(sb, ind).AppendLine("Program");

            // Funciones top-level(si si)
            if (p.Functions is not null && p.Functions.Count > 0)
            {
                Ind(sb, ind + 1).AppendLine("Functions");
                foreach (var f in p.Functions)
                    PrintFunction(f, sb, ind + 2);
            }

            // Sentencias top-level(si no)
            if (p.Body is not null && p.Body.Count > 0)
            {
                Ind(sb, ind + 1).AppendLine("TopLevel");
                foreach (var s in p.Body)
                    PrintStmt(s, sb, ind + 2);
            }
        }

        private static void PrintFunction(FunctionDecl f, StringBuilder sb, int ind)
        {
            Ind(sb, ind).Append("Function ");
            sb.Append(f.ReturnTypeName).Append(' ').Append(f.Name).Append('(');
            for (int i = 0; i < f.Parameters.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var p = f.Parameters[i];
                sb.Append(p.TypeName).Append(' ').Append(p.Name);
            }
            sb.AppendLine(")");
            PrintBlock(f.Body, sb, ind + 1);
        }

        private static void PrintStmt(Stmt s, StringBuilder sb, int ind)
        {
            switch (s)
            {
                case BlockStmt b:
                    PrintBlock(b, sb, ind);
                    break;

                case VarDeclStmt v:
                    {
                        var line = new StringBuilder();
                        line.Append("VarDecl ").Append(v.TypeName).Append(' ').Append(v.Name);
                        if (v.IsConst) line.Append(" (prophecy)");
                        if (v.Init != null)
                            line.Append(" = ").Append(PrintExprInline(v.Init));
                        Ind(sb, ind).AppendLine(line.ToString());
                        break;
                    }

                case AssignStmt a:
                    Ind(sb, ind)
                        .Append("Assign ")
                        .Append(PrintLValueInline(a.Target))
                        .Append(" <- ")
                        .AppendLine(PrintExprInline(a.Value));
                    break;

                // return
                case ReturnStmt r:
                    Ind(sb, ind).Append("Return");
                    if (r.Value != null)
                        sb.Append(' ').Append(PrintExprInline(r.Value));
                    sb.AppendLine();
                    break;

                // breack de switch
                case BreakStmt:
                    Ind(sb, ind).AppendLine("Break");
                    break;

                // condicional if
                case IfStmt iff:
                    Ind(sb, ind).Append("If cond: ").AppendLine(PrintExprInline(iff.Cond));
                    PrintBlock(iff.Then, sb, ind + 1);
                    foreach (var e in iff.Elifs)
                    {
                        Ind(sb, ind).Append("ElseIf cond: ").AppendLine(PrintExprInline(e.Cond));
                        PrintBlock(e.Then, sb, ind + 1);
                    }
                    if (iff.Else != null)
                    {
                        Ind(sb, ind).AppendLine("Else");
                        PrintBlock(iff.Else, sb, ind + 1);
                    }
                    break;

                //  While 
                case WhileStmt w:
                    Ind(sb, ind).Append("While cond: ").AppendLine(PrintExprInline(w.Cond));
                    PrintBlock(w.Body, sb, ind + 1);
                    break;

                //  Do-While 
                case DoWhileStmt dw:
                    Ind(sb, ind).AppendLine("Do");
                    PrintBlock(dw.Body, sb, ind + 1);
                    Ind(sb, ind).Append("While cond: ").AppendLine(PrintExprInline(dw.Cond));
                    break;

                // For 
                case ForAncestralStmt fa:
                    Ind(sb, ind).AppendLine("ForAncestral");
                    Ind(sb, ind + 1).Append("Init: ");
                    // Imprime init en una línea
                    PrintStmtOneLine(fa.Init, sb); sb.AppendLine();
                    Ind(sb, ind + 1).Append("Cond: ").AppendLine(PrintExprInline(fa.Cond));
                    Ind(sb, ind + 1).Append("Update: ");
                    PrintStmtOneLine(fa.Update, sb); sb.AppendLine();
                    PrintBlock(fa.Body, sb, ind + 1);
                    break;

                //  Switch
                case SwitchStmt sw:
                    Ind(sb, ind).Append("Switch expr: ").AppendLine(PrintExprInline(sw.Expr));
                    foreach (var c in sw.Cases)
                    {
                        Ind(sb, ind + 1).Append("Case ").Append(PrintExprInline(c.Match)).AppendLine(":");
                        PrintBlock(c.Body, sb, ind + 2);
                    }
                    if (sw.Default != null)
                    {
                        Ind(sb, ind + 1).AppendLine("Default:");
                        PrintBlock(sw.Default, sb, ind + 2);
                    }
                    break;
                    //Otro caso
                case ExprStmt es:
                    Ind(sb, ind).Append("Expr ").AppendLine(PrintExprInline(es.Expr));
                    break;        

                default:
                    Ind(sb, ind).AppendLine("<Stmt?>");
                    break;
            }
        }

        private static void PrintBlock(BlockStmt b, StringBuilder sb, int ind)
        {
            Ind(sb, ind).AppendLine("Block");
            foreach (var st in b.Statements)
                PrintStmt(st, sb, ind + 1);
        }

        // Para imprimir sentencias simples (init/update del for) en una sola línea
        private static void PrintStmtOneLine(Stmt s, StringBuilder sb)
        {
            switch (s)
            {
                case VarDeclStmt v:
                    {
                        sb.Append("VarDecl ").Append(v.TypeName).Append(' ').Append(v.Name);
                        if (v.IsConst) sb.Append(" (prophecy)");
                        if (v.Init != null) sb.Append(" = ").Append(PrintExprInline(v.Init));
                        break;
                    }
                case AssignStmt a:
                    {
                        sb.Append("Assign ")
                          .Append(PrintLValueInline(a.Target))
                          .Append(" <- ")
                          .Append(PrintExprInline(a.Value));
                        break;
                    }
                case ExprStmt es:
                    {
                        sb.Append("Expr ").Append(PrintExprInline(es.Expr));
                        break;
                    }
                default:
                    sb.Append("<Stmt?>");
                    break;
            }
        }


        public static string PrintExpr(Expr e) => PrintExprInline(e);

        private static string PrintExprInline(Expr e)
        {
            switch (e)
            {
                case IntLitExpr x: return $"Int({x.Value})";
                case DecLitExpr d: return $"Dec({d.Value})";
                case TextLitExpr x: return $"Text(\"{Escape(x.Value)}\")";
                case CharLitExpr c: return $"Char('{EscapeChar(c.Value)}')";
                case BoolLitExpr b: return b.Value ? "Bool(lumus)" : "Bool(nox)";
                case NullLitExpr: return "Null";

                case VarExpr v: return $"Var({v.Name})";

                case UnaryExpr u: return $"{u.Op} {PrintExprInline(u.Right)}";

                case BinaryExpr b: return $"({PrintExprInline(b.Left)} {b.Op} {PrintExprInline(b.Right)})";

                case CallExpr call:
                    {
                        var sb = new StringBuilder();
                        sb.Append("Call ").Append(call.FuncName).Append('(');
                        for (int i = 0; i < call.Args.Count; i++)
                        {
                            if (i > 0) sb.Append(", ");
                            sb.Append(PrintExprInline(call.Args[i]));
                        }
                        sb.Append(')');
                        return sb.ToString();
                    }

                case IndexExpr idx:
                    {
                        // Soporta encadenado: a[i][j] => Index(Index(Var(a), i), j)
                        return $"{PrintExprInline(idx.Target)}[{PrintExprInline(idx.Index)}]";
                    }

                default: return "<Expr?>";
            }
        }

        // Imprime el valor 
        private static string PrintLValueInline(Expr target)
        {
            return target switch
            {
                VarExpr v => $"Var({v.Name})",
                IndexExpr i => PrintExprInline(i),
                _ => $"<LValue:{PrintExprInline(target)}>"
            };
        }


        private static string Escape(string s)
        {
            if (s == null) return "";
            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private static string EscapeChar(char ch)
        {
            return ch switch
            {
                '\\' => "\\\\",
                '\'' => "\\'",
                '\r' => "\\r",
                '\n' => "\\n",
                '\t' => "\\t",
                _ => ch.ToString()
            };
        }
        private static StringBuilder Ind(StringBuilder sb, int ind)
            => sb.Append(' ', ind * 2);
    }
}
