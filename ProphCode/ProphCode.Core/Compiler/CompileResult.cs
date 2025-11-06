using ProphCode.Core.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace ProphCode.Core
{
    public sealed class CompileResult
    {
        public List<Diagnostic> Diagnostics { get; } = new();
        public List<string> Output { get; } = new(); //Lo usamos para dar el resultado en la salida
    }
}
