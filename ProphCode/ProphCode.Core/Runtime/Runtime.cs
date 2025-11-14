using System;
using System.Collections.Generic;

namespace ProphCode.Core.Runtime
{
    public sealed class PcValue
    {
        public enum K { Null, Int, Dec, Text, Bool, List, Vec }
        public K Kind { get; }

        public int AsInt;
        public double AsDec;
        public string AsText;
        public bool AsBool;
        public List<PcValue> AsList;   
        public List<PcValue> AsVec;   
        // Constructor privado
        private PcValue(K kind) { Kind = kind; }

        
        public static PcValue Null() => new(K.Null);
        public static PcValue Int(int value) => new(K.Int) { AsInt = value };
        public static PcValue Dec(double value) => new(K.Dec) { AsDec = value };
        public static PcValue Text(string value) => new(K.Text) { AsText = value };
        public static PcValue Bool(bool value) => new(K.Bool) { AsBool = value };

        // Métodos para Listas y Vectores (colecciones de PcValue)
        public static PcValue List(List<PcValue> list) => new(K.List) { AsList = list };
        public static PcValue Vec(List<PcValue> vec) => new(K.Vec) { AsVec = vec };

        // Método de sobrecarga para obtener la representación de la instancia
        public override string ToString()
        {
            return Kind switch
            {
                K.Null => "null",
                K.Int => AsInt.ToString(),
                K.Dec => AsDec.ToString(),
                K.Text => AsText ?? "",
                K.Bool => AsBool ? "lumus" : "nox",
                K.List => $"List({AsList?.Count} items)",   // Muestra el tamaño de la lista
                K.Vec => $"Vec({AsVec?.Count} items)",     // Muestra el tamaño del vector
                _ => "<?>"
            };
        }
    }

    // Señal para interrumpir el flujo de ejecución (break) en ciclos
    public sealed class BreakSignal : Exception { }

    // Señal para retorno de funciones (incluye el valor de retorno)
    public sealed class ReturnSignal : Exception
    {
        public PcValue Value { get; }
        public ReturnSignal(PcValue v) { Value = v; }
    }
    public sealed class VarCell
    {
        public PcValue Value;
        public bool IsConst;
    }
    public class Env
    {
        private readonly Dictionary<string, VarCell> _map = new(StringComparer.Ordinal);
        private readonly Env _parent;

        public Env(Env parent = null)
        {
            _parent = parent;
        }

        // Declarar una nueva variable en el entorno
        public void Declare(string name, PcValue v, bool isConst)
        {
            if (_map.ContainsKey(name))
                throw new Exception($"[Semántica] Variable '{name}' ya declarada en este ámbito.");

            _map[name] = new VarCell { Value = v, IsConst = isConst };
        }

        // Obtener una variable del entorno
        public bool TryGet(string name, out VarCell cell)
        {
            if (_map.TryGetValue(name, out cell))
                return true;

            if (_parent != null)
                return _parent.TryGet(name, out cell);

            cell = null;
            return false;
        }

        // Asignar un valor a una variable en el entorno
        public void Assign(string name, PcValue v)
        {
            if (!TryGet(name, out var cell))
                throw new Exception($"[Semántica] Variable '{name}' no declarada.");

            if (cell.IsConst)
                throw new Exception($"[Semántica] Variable '{name}' es constante.");

            if (!AreTypesCompatible(cell.Value, v))
                throw new Exception($"[Semántica] Tipo incompatible al asignar valor de tipo {v.Kind} a variable '{name}' de tipo {cell.Value.Kind}.");

            cell.Value = v;
        }

        private static bool AreTypesCompatible(PcValue existing, PcValue incoming)
        {
            // Verifica si los tipos son compatibles
            if (existing.Kind == incoming.Kind) return true;
            if (existing.Kind == PcValue.K.Int && incoming.Kind == PcValue.K.Dec) return true;
            if (existing.Kind == PcValue.K.Dec && incoming.Kind == PcValue.K.Int) return true;

            return false;
        }
    }


}
