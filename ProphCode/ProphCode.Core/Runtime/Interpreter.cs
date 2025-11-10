using ProphCode.Core.AST;
using System;
using System.Collections.Generic;
using static ProphCode.Core.Runtime.PcValue;

namespace ProphCode.Core.Runtime
{
    public sealed class Interpreter
    {
        private readonly ProgramNode _prog;

        private readonly Dictionary<string, FunctionDecl> _funcs = new(StringComparer.Ordinal);

        public Interpreter(ProgramNode prog)
        {
            _prog = prog ?? throw new ArgumentNullException(nameof(prog));
            foreach (var f in _prog.Functions)
                _funcs[f.Name] = f;  // última definición gana (simple)
        }

      
        public void Run()
        {
            var global = new Env();

            // Ejecutar sentencias top-level (incluye lo que volcaste desde abracadabra)
            foreach (var s in _prog.Body)
                ExecStmt(s, global);
        }

        // ===== Sentencias =====
        private void ExecStmt(Stmt s, Env env)
        {
            switch (s)
            {
                case BlockStmt b:
                    {
                        var inner = new Env(env);
                        foreach (var st in b.Statements) ExecStmt(st, inner);
                        break;
                    }
                case VarDeclStmt v:
                    {
                        var init = v.Init != null ? EvalExpr(v.Init, env) : PcValue.Null();
                        env.Declare(v.Name, init, v.IsConst);
                        break;
                    }
                case AssignStmt a:
                    {
                        // MVP: asignación a variable (no índices aún)
                        if (a.Target is VarExpr ve)
                        {
                            var val = EvalExpr(a.Value, env);
                            env.Assign(ve.Name, val);
                        }
                        else
                            throw new Exception("[Runtime] MVP: asignación sólo a variables (no índices).");
                        break;
                    }
                case IfStmt iff:
                    {
                        if (AsBool(EvalExpr(iff.Cond, env))) ExecStmt(iff.Then, env);
                        else
                        {
                            bool done = false;
                            foreach (var e in iff.Elifs)
                                if (AsBool(EvalExpr(e.Cond, env))) { ExecStmt(e.Then, env); done = true; break; }
                            if (!done && iff.Else != null) ExecStmt(iff.Else, env);
                        }
                        break;
                    }
                case WhileStmt w:
                    {
                        while (AsBool(EvalExpr(w.Cond, env)))
                        {
                            try { ExecStmt(w.Body, env); }
                            catch (BreakSignal) { break; }
                        }
                        break;
                    }

                case DoWhileStmt dw:
                    {
                        do
                        {
                            try { ExecStmt(dw.Body, env); }
                            catch (BreakSignal) { break; }
                        }
                        while (AsBool(EvalExpr(dw.Cond, env)));
                        break;
                    }


                case ReturnStmt r:
                    {
                        // Lanza la señal para cortar la ejecución del cuerpo actual
                        var rv = r.Value != null ? EvalExpr(r.Value, env) : PcValue.Null();
                        throw new ReturnSignal(rv);
                    }

                case ExprStmt es:
                    {
                        _ = EvalExpr(es.Expr, env);
                        break;
                    }

                case ForAncestralStmt fa:
                    {
                        // for especial: el init/cond/update viven en un ámbito hijo
                        var inner = new Env(env);
                        // Init puede ser VarDecl o Assign; reutilizamos ExecStmt
                        ExecStmt(fa.Init, inner);

                        while (AsBool(EvalExpr(fa.Cond, inner)))
                        {
                            try
                            {
                                ExecStmt(fa.Body, inner);
                            }
                            catch (BreakSignal)
                            {
                                break;
                            }
                            // update es siempre Assign en tu parser
                            ExecStmt(fa.Update, inner);
                        }
                        break;
                    }


                case SwitchStmt sw:
                    {
                        var key = EvalExpr(sw.Expr, env);

                        bool executed = false;
                        foreach (var c in sw.Cases)
                        {
                            var m = EvalExpr(c.Match, env);
                            if (Eq(key, m))      // reutilizamos tu helper Eq(...)
                            {
                                try { ExecStmt(c.Body, env); }
                                catch (BreakSignal) { /* salir del switch */ }
                                executed = true;
                                break;
                            }
                        }

                        if (!executed && sw.Default != null)
                        {
                            try { ExecStmt(sw.Default, env); }
                            catch (BreakSignal) { /* salir del switch */ }
                        }

                        break;
                    }


                case BreakStmt:
                    throw new BreakSignal();

                // Ignoramos Return/Break/Switch/For por ahora (los añadimos luego)
                default:
                    throw new Exception("[Runtime] Sentencia no soportada aún en el MVP.");
            }
        }

        // ===== Expresiones =====
        private PcValue EvalExpr(Expr e, Env env)
        {
            switch (e)
            {
                case IntLitExpr i: return PcValue.Int(i.Value);
                case DecLitExpr d: return PcValue.Dec(d.Value);
                case TextLitExpr t: return PcValue.Text(t.Value ?? "");
                case CharLitExpr c: return PcValue.Text(c.Value.ToString());
                case BoolLitExpr b: return PcValue.Bool(b.Value);
                case NullLitExpr: return PcValue.Null();

                case VarExpr v:
                    {
                        if (!env.TryGet(v.Name, out var cell))
                            throw new Exception($"[Runtime] Variable '{v.Name}' no declarada.");
                        return cell.Value;
                    }

                case UnaryExpr u:
                    {
                        var r = EvalExpr(u.Right, env);
                        return u.Op switch
                        {
                            "anti" => PcValue.Bool(!AsBool(r)),
                            "-" => IsNumber(r) ? (r.Kind == PcValue.K.Int ? PcValue.Int(-r.AsInt) : PcValue.Dec(-r.AsDec))
                                                  : throw new Exception("Operador '-' requiere numérico."),
                            _ => throw new Exception($"Unario desconocido '{u.Op}'.")
                        };
                    }

                case BinaryExpr b:
                    {
                        var L = EvalExpr(b.Left, env);
                        var R = EvalExpr(b.Right, env);
                        return b.Op switch
                        {
                            "+" => Add(L, R),
                            "-" => Num2(L, R, (x, y) => x - y),
                            "*" => Num2(L, R, (x, y) => x * y),
                            "/" => Div(L, R),
                            "%" => Mod(L, R),

                            "==" => PcValue.Bool(Eq(L, R)),
                            "!=" => PcValue.Bool(!Eq(L, R)),
                            "beyone" => Cmp(L, R, c => c > 0),
                            "beyoneq" => Cmp(L, R, c => c >= 0),
                            "under" => Cmp(L, R, c => c < 0),
                            "undereq" => Cmp(L, R, c => c <= 0),

                            "and" => PcValue.Bool(AsBool(L) && AsBool(R)),
                            "or" => PcValue.Bool(AsBool(L) || AsBool(R)),

                            _ => throw new Exception($"Operador binario desconocido '{b.Op}'.")
                        };
                    }
                case CallExpr call:
                    return CallFunction(call, env);


                default:
                    throw new Exception("[Runtime] Expresión no soportada en el MVP.");
            }
        }


        private PcValue CallFunction(CallExpr c, Env callerEnv)
        {
            var name = c.FuncName;

            // 1) Built-ins primero
            if (string.Equals(name, "reveal", StringComparison.Ordinal))
            {
                var parts = new List<string>();
                foreach (var a in c.Args) parts.Add(EvalExpr(a, callerEnv).ToString());
                Console.WriteLine(string.Join("", parts));
                return PcValue.Null();
            }
            if (string.Equals(name, "scry", StringComparison.Ordinal))
            {
                if (c.Args.Count > 0)
                {
                    var p = EvalExpr(c.Args[0], callerEnv).ToString();
                    if (!string.IsNullOrEmpty(p)) Console.Write(p);
                }
                var line = Console.ReadLine() ?? "";
                return PcValue.Text(line);
            }

            // 2) Función de usuario
            if (!_funcs.TryGetValue(name, out var f))
                throw new Exception($"[Runtime] Función '{name}' no encontrada.");

            // Verifica aridad (simple)
            if (f.Parameters.Count != c.Args.Count)
                throw new Exception($"[Runtime] '{name}' espera {f.Parameters.Count} arg(s); se pasaron {c.Args.Count}.");

            // Crea el entorno local de la función enlazando parámetros
            var local = new Env(callerEnv);
            for (int i = 0; i < f.Parameters.Count; i++)
            {
                var p = f.Parameters[i];
                var v = EvalExpr(c.Args[i], callerEnv);
                local.Declare(p.Name, v, isConst: false);
            }

            // Ejecuta el cuerpo y captura 'return'
            try
            {
                ExecStmt(f.Body, local);
                // si no hubo 'return', devuelve null
                return PcValue.Null();
            }
            catch (ReturnSignal rs)
            {
                return rs.Value ?? PcValue.Null();
            }
        }


        // ===== Builtins MVP =====
        private PcValue CallBuiltin(CallExpr c, Env env)
        {
            if (string.Equals(c.FuncName, "reveal", StringComparison.Ordinal))
            {
                var parts = new List<string>();
                foreach (var a in c.Args) parts.Add(EvalExpr(a, env).ToString());
                Console.WriteLine(string.Join("", parts)); // imprime sin separador
                return PcValue.Null();
            }
            if (string.Equals(c.FuncName, "scry", StringComparison.Ordinal))
            {
                // scry(prompt) -> text
                if (c.Args.Count > 0)
                {
                    var p = EvalExpr(c.Args[0], env).ToString();
                    if (!string.IsNullOrEmpty(p)) Console.Write(p);
                }
                var line = Console.ReadLine() ?? "";
                return PcValue.Text(line);
            }

            throw new Exception($"[Runtime] Función '{c.FuncName}' no soportada aún en el MVP.");
        }

        // ===== helpers numéricos/comparación/booleanos =====
        private static bool IsNumber(PcValue v) => v.Kind == PcValue.K.Int || v.Kind == PcValue.K.Dec;

        private static PcValue Add(PcValue a, PcValue b)
        {
            if (a.Kind == PcValue.K.Text || b.Kind == PcValue.K.Text)
                return PcValue.Text(a.ToString() + b.ToString());
            if (a.Kind == PcValue.K.Dec || b.Kind == PcValue.K.Dec)
                return PcValue.Dec(ToDouble(a) + ToDouble(b));
            if (a.Kind == PcValue.K.Int && b.Kind == PcValue.K.Int)
                return PcValue.Int(a.AsInt + b.AsInt);
            throw new Exception("Suma: tipos no soportados.");
        }

        private static PcValue Num2(PcValue a, PcValue b, Func<double, double, double> op)
        {
            if (!IsNumber(a) || !IsNumber(b)) throw new Exception("Operación requiere números.");
            if (a.Kind == PcValue.K.Dec || b.Kind == PcValue.K.Dec)
                return PcValue.Dec(op(ToDouble(a), ToDouble(b)));
            var r = op(a.AsInt, b.AsInt);
            return (r % 1 == 0) ? PcValue.Int((int)r) : PcValue.Dec(r);
        }

        private static PcValue Div(PcValue a, PcValue b)
        {
            var denom = ToDouble(b);
            if (Math.Abs(denom) < 1e-12) throw new Exception("División entre cero.");
            if (a.Kind == PcValue.K.Dec || b.Kind == PcValue.K.Dec)
                return PcValue.Dec(ToDouble(a) / denom);
            return PcValue.Int((int)(ToDouble(a) / denom));
        }

        private static PcValue Mod(PcValue a, PcValue b)
        {
            if (a.Kind == PcValue.K.Int && b.Kind == PcValue.K.Int)
                return PcValue.Int(a.AsInt % b.AsInt);
            throw new Exception("Módulo '%' requiere enteros.");
        }

        private static bool Eq(PcValue a, PcValue b)
        {
            if (a.Kind != b.Kind) return false;
            return a.Kind switch
            {
                PcValue.K.Null => true,
                PcValue.K.Int => a.AsInt == b.AsInt,
                PcValue.K.Dec => Math.Abs(a.AsDec - b.AsDec) < 1e-12,
                PcValue.K.Text => string.Equals(a.AsText, b.AsText, StringComparison.Ordinal),
                PcValue.K.Bool => a.AsBool == b.AsBool,
                _ => false
            };
        }

        private static PcValue Cmp(PcValue a, PcValue b, Func<int, bool> pred)
        {
            int cmp;
            if (a.Kind == PcValue.K.Text && b.Kind == PcValue.K.Text)
                cmp = string.Compare(a.AsText, b.AsText, StringComparison.Ordinal);
            else if (IsNumber(a) && IsNumber(b))
                cmp = ToDouble(a).CompareTo(ToDouble(b));
            else
                throw new Exception("Comparación requiere ambos numéricos o ambos text.");
            return PcValue.Bool(pred(cmp));
        }

        private static double ToDouble(PcValue v) => v.Kind == PcValue.K.Dec ? v.AsDec : v.AsInt;

        private static bool AsBool(PcValue v) => v.Kind switch
        {
            PcValue.K.Bool => v.AsBool,
            PcValue.K.Null => false,
            PcValue.K.Int => v.AsInt != 0,
            PcValue.K.Dec => Math.Abs(v.AsDec) > 1e-12,
            PcValue.K.Text => !string.IsNullOrEmpty(v.AsText),
            _ => false
        };
    }
}
