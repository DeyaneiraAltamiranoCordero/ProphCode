using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProphCode.Core.Compiler
{
    public enum Severity { Info, Warning, Error }
    public sealed record Diagnostic(Severity Severity, int Line, int Col, string Message);
}