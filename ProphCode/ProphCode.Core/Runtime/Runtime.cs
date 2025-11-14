using System;
using System.Collections.Generic;
using ProphCode.Core.AST;

namespace ProphCode.Core.Runtime
{
    // Valor en tiempo de ejecución (MVP: int, dec, text, bool, null)
    public sealed class PcValue
    {
        public enum K { Null, Int, Dec, Text, Bool }
        public K Kind { get; }
        public int AsInt; public double AsDec; public string AsText; public bool AsBool;

        private PcValue(K k) { Kind = k; }
        public static PcValue Null() => new(K.Null);
        public static PcValue Int(int v) => new(K.Int) { AsInt = v };
        public static PcValue Dec(double v) => new(K.Dec) { AsDec = v };
        public static PcValue Text(string v) => new(K.Text) { AsText = v };
        public static PcValue Bool(bool v) => new(K.Bool) { AsBool = v };

        public sealed class BreakSignal : Exception { }

        public override string ToString() => Kind switch
        {
            K.Null => "null",
            K.Int => AsInt.ToString(),
            K.Dec => AsDec.ToString(),
            K.Text => AsText ?? "",
            K.Bool => AsBool ? "lumus" : "nox",
            _ => "<?>"
        };
    }


    public sealed class ReturnSignal : Exception
    {
        public PcValue Value { get; }
        public ReturnSignal(PcValue v) { Value = v; }
    }
    // Celda de variable (con flag para prophecy)
    public sealed class VarCell
    {
        public PcValue Value;
        public bool IsConst;
    }

    // Entorno (ámbitos anidados)
    public sealed class Env
    {
        private readonly Dictionary<string, VarCell> _map = new(StringComparer.Ordinal);
        private readonly Env _parent;
        public Env(Env parent = null) { _parent = parent; }

        public void Declare(string name, PcValue v, bool isConst)
        {
            if (_map.ContainsKey(name))
                throw new Exception($"[Runtime] Variable '{name}' ya declarada en este ámbito.");
            _map[name] = new VarCell { Value = v, IsConst = isConst };
        }

        public bool TryGet(string name, out VarCell cell)
            => _map.TryGetValue(name, out cell) || (_parent != null && _parent.TryGet(name, out cell));

        public void Assign(string name, PcValue v)
        {
            if (!TryGet(name, out var cell))
                throw new Exception($"[Runtime] Variable '{name}' no declarada.");
            if (cell.IsConst)
                throw new Exception($"[Runtime] Variable '{name}' es 'prophecy' (const).");
            cell.Value = v;
        }
    }
}