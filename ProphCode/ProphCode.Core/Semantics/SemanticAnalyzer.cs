using ProphCode.Core.AST;
using ProphCode.Core.Runtime;
using System;
using System.Collections.Generic;

public class SemanticAnalyzer
{
    private readonly ProgramNode _program;
    private readonly Env _globalEnv;
    private readonly List<string> _errors = new();

    public SemanticAnalyzer(ProgramNode program)
    {
        _program = program ?? throw new ArgumentNullException(nameof(program));
        _globalEnv = new Env();  // El entorno global para las variables
    }

    public IReadOnlyList<string> Errors => _errors;

    public void Analyze()
    {
        // 1. Analizar funciones y su cuerpo
        foreach (var func in _program.Functions)
            AnalyzeFunction(func);

        // 2. Analizar sentencias (cuerpo principal)
        foreach (var stmt in _program.Body)
            AnalyzeStmt(stmt, _globalEnv);
    }

    private void AnalyzeFunction(FunctionDecl func)
    {
        // Verificar si la función ya fue declarada
        if (_globalEnv.TryGet(func.Name, out _))
            _errors.Add($"[Semántica] Función '{func.Name}' ya declarada.");

        // Declaramos la función en el entorno global
        _globalEnv.Declare(func.Name, PcValue.Null(), isConst: false);

        var localEnv = new Env(_globalEnv);

        // 3. Declarar parámetros de la función y verificar su tipo
        foreach (var param in func.Parameters)
        {
            PcValue paramType = param.TypeName switch
            {
                "int" => PcValue.Int(0),
                "dec" => PcValue.Dec(0.0),
                "text" => PcValue.Text(""),
                "bool" => PcValue.Bool(true),
                _ => throw new Exception($"[Semántica] Tipo de parámetro no soportado: {param.TypeName}")
            };
            localEnv.Declare(param.Name, paramType, isConst: false);
        }

        // 4. Analizar las sentencias del cuerpo de la función
        foreach (var stmt in func.Body.Statements)
            AnalyzeStmt(stmt, localEnv);

        // Verificar que la función retorne el tipo correcto
        if (func.ReturnTypeName != "void")
        {
            var returnStmt = func.Body.Statements.OfType<ReturnStmt>().FirstOrDefault();
            if (returnStmt == null)
                _errors.Add($"[Semántica] La función '{func.Name}' debería retornar un valor de tipo {func.ReturnTypeName}.");
        }
    }

    private void AnalyzeStmt(Stmt stmt, Env env)
    {
        switch (stmt)
        {
            case VarDeclStmt varDecl:
                AnalyzeVarDecl(varDecl, env);
                break;
            case AssignStmt assign:
                AnalyzeAssign(assign, env);
                break;
                // Otros casos de sentencias (puedes agregarlos aquí según necesidad)
        }
    }
    private void AnalyzeVarDecl(VarDeclStmt varDecl, Env env)
    {
        // Verificar si la variable ya está declarada
        if (env.TryGet(varDecl.Name, out _))
            _errors.Add($"[Semántica] La variable '{varDecl.Name}' ya ha sido declarada.");

        // Obtener el tipo de la variable
        PcValue value = PcValue.Null();
        if (varDecl.Init != null)
        {
            // Verificar el tipo de la inicialización
            value = GetValueType(varDecl.Init);

            // Verificar que el tipo de la inicialización coincida con el tipo de la variable
            if (!AreTypesCompatible(GetVarType(varDecl.TypeName), value))
                _errors.Add($"[Semántica] Incompatibilidad de tipos: '{varDecl.Name}' es de tipo {varDecl.TypeName} pero se le asigna un valor de tipo {value.Kind}.");
        }

        // Declarar la variable en el entorno
        env.Declare(varDecl.Name, value, varDecl.IsConst);
    }

    private PcValue GetVarType(string typeName)
    {
        return typeName switch
        {
            "int" => PcValue.Int(0),
            "dec" => PcValue.Dec(0.0),
            "text" => PcValue.Text(""),
            "bool" => PcValue.Bool(true),
            _ => throw new Exception($"[Semántica] Tipo desconocido: {typeName}")
        };
    }

    private PcValue GetValueType(Expr expr)
    {
        switch (expr)
        {
            case IntLitExpr intLit:
                return PcValue.Int(intLit.Value);
            case DecLitExpr decLit:
                return PcValue.Dec(decLit.Value);
            case TextLitExpr textLit:
                return PcValue.Text(textLit.Value);
            default:
                throw new Exception($"[Semántica] Expresión desconocida: {expr.GetType().Name}");
        }
    }


    private void AnalyzeAssign(AssignStmt assign, Env env)
    {
        // Verifica que la variable esté declarada
        if (assign.Target is VarExpr varExpr)
        {
            if (!env.TryGet(varExpr.Name, out var cell))
                _errors.Add($"[Semántica] La variable '{varExpr.Name}' no está declarada.");

            // Verificar la compatibilidad de tipos de la expresión de la asignación
            PcValue value = GetValueType(assign.Value);

            if (!AreTypesCompatible(cell.Value, value))
                _errors.Add($"[Semántica] Asignación incompatible de tipo {value.Kind} a '{varExpr.Name}' de tipo {cell.Value.Kind}");

            // Realiza la asignación
            env.Assign(varExpr.Name, value);
        }
        else
        {
            _errors.Add("[Semántica] La asignación solo permite variables simples.");
        }
    }

    // Verifica la compatibilidad entre tipos
    private static bool AreTypesCompatible(PcValue existing, PcValue incoming)
    {
        // Los tipos son compatibles si son iguales o uno puede ser convertido al otro
        if (existing.Kind == incoming.Kind) return true;

        // Permitir int <-> dec
        if (existing.Kind == PcValue.K.Int && incoming.Kind == PcValue.K.Dec) return true;
        if (existing.Kind == PcValue.K.Dec && incoming.Kind == PcValue.K.Int) return true;

        // Para otros tipos no compatibles
        return false;
    }
}
