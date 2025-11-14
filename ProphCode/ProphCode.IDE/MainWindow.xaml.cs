using ProphCode.Core.AST;
using ProphCode.Core.Lexing;
using ProphCode.Core.Parsing;
using ProphCode.Core.Runtime;
using System.Text;
using System.Windows;


namespace ProphCode.IDE
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void BtnCompilar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string source = Editor.Text;

                // 1) LEXER
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(source);

                var sb = new StringBuilder();
                sb.AppendLine("[LEX] Tokens:");
                foreach (var t in tokens)
                    sb.AppendLine($"{t.Kind,-20} \"{Escape(t.Lexeme)}\"  @ {t.Line}:{t.Col}");

                // 2) PARSER
                sb.AppendLine();
                sb.AppendLine("[PARSER] Árbol sintáctico:");

                ProgramNode prog;
                try
                {
                    var parser = new Parser(tokens);
                    prog = parser.ParseProgram();

                    // Imprimir AST
                    sb.AppendLine(AstPrinter.Print(prog));
                }
                catch (Exception exParse)
                {
                    sb.AppendLine();
                    sb.AppendLine("[Parse ERROR] " + exParse.Message);

                    Consola.Text = sb.ToString();
                    Consola.ScrollToEnd();
                    return; // si hay error de sintaxis, no seguimos a semántica
                }

                // 3) ANÁLISIS SEMÁNTICO
                sb.AppendLine();
                sb.AppendLine("[SEMANTIC] Análisis:");

                var sema = new SemanticAnalyzer(prog);
                sema.Analyze();

                if (sema.Errors.Count == 0)
                {
                    sb.AppendLine("Sin errores semánticos. ✔");
                }
                else
                {
                    sb.AppendLine("Se encontraron errores semánticos:");
                    foreach (var err in sema.Errors)
                        sb.AppendLine(" - " + err);
                }

                // 4) Mostrar en la consola del IDE
                Consola.Text = sb.ToString();
                Consola.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Consola.Text = "[LEX/GENERAL ERROR] " + ex.Message;
                Consola.ScrollToEnd();
            }
        }

        private string Escape(string s)
        {
            if (s == null) return "";
            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }
        private void BtnEjecutar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string source = Editor.Text;

                // 1) LEXER
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(source);

                // 2) PARSER
                var parser = new Parser(tokens);
                var prog = parser.ParseProgram();

                // 3) ANÁLISIS SEMÁNTICO ANTES DE EJECUTAR
                var sema = new SemanticAnalyzer(prog);
                sema.Analyze();

                if (sema.Errors.Count > 0)
                {
                    Consola.AppendText("[RUN] No se ejecuta por errores semánticos:\n");
                    foreach (var err in sema.Errors)
                        Consola.AppendText(" - " + err + "\n");
                    Consola.ScrollToEnd();
                    return; // No permitimos la ejecución si hay errores semánticos
                }

                // 4) RUNTIME / INTERPRETER
                var runner = new ProphCode.Core.Runtime.Interpreter(prog);

                runner.WriteLine = (text) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Consola.AppendText(text + Environment.NewLine);
                        Consola.ScrollToEnd();
                    });
                };

                runner.ReadLine = (prompt) =>
                {
                    return Microsoft.VisualBasic.Interaction.InputBox(
                        string.IsNullOrEmpty(prompt) ? "Ingrese un valor:" : prompt,
                        "scry()", ""
                    );
                };

                Consola.AppendText("[RUN] Iniciando...\n");
                runner.Run();
                Consola.AppendText("[RUN] OK\n");
                Consola.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Consola.AppendText("[RUN ERROR] " + ex.Message + "\n");
                Consola.ScrollToEnd();
            }
        }


        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            Editor.Clear();
            Consola.Clear();
        }

        private void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
         
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        //Tipos de datos
        private void InsertProphecy(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("prophecy int nombreConstante -> 0;");
        }

        private void InsertText(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("text nombreString -> \"Hola mundo\";");
        }

        private void InsertInt(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("int nombreEntero -> 42;");
        }

        private void InsertDec(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("dec nombreDecimal -> 3.14;");
        }

        private void InsertBool(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("bool nombreBolean -> lumus;");
        }

        private void InsertChar(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("char nombreLetra -> 'a';");
        }

        private void InsertList(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("list<int> nombreLista -> [];");
        }

        private void InsertVec(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("vec<int> nombreVector -> [0, 1, 2];");
        }

        //Operadores
        private void InsertGreaterThan(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("variableDeclada -> a beyone b;");
        }

        private void InsertGreaterThanOrEqual(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultado -> a beyoneq b;");
        }

        private void InsertLessThan(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultado -> a under b;");
        }

        private void InsertLessThanOrEqual(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultado -> a undereq b;");
        }

        private void InsertAddition(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultadoSuma -> a + b;");
        }

        private void InsertSubtraction(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultadoResta -> a - b;");
        }

        private void InsertMultiplication(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultadoMulti -> a * b;");
        }

        private void InsertDivision(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultadoDiv -> a / b;");
        }

        private void InsertModulo(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultadoMod -> a % b;");
        }
        //
        //Palabras reservadas literales
        private void InsertLiteralTrue(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("lumus;  ~Representa true~");
        }

        private void InsertLiteralFalse(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("nox;  ~Representa false~");
        }

        private void InsertLiteralNull(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("null;  ~Representa un valor nulo~");
        }

        //Operadores Lógicas
        private void InsertReveal(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("reveal(\"Hola mundo\");  ~Muestra un mensaje en consola~");
        }

        private void InsertAnd(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultado -> a and b;  ~Operador lógico AND~");
        }

        private void InsertOr(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultado -> a or b;  ~Operador lógico OR~");
        }

        private void InsertAnti(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("resultado -> anti a;  ~Operador lógico NOT~");
        }
        //Funciones
        private void InsertFunctionStructure(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "spell <tipo> nombreFuncion(<tipoParametro> <nombreVariable>) ->{\n" +

                "    <tipo> variable -> <asignacion>;\n" +
                "    return variable;\n" +

                "}endSpell"
            );
        }
        private void InsertMainFunctionStructure(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "abracadabra {\n" +
                "    ~Bloque principal del programa~\n" +
                "} disappear"
            );
        }
        private void InsertFunctionWithoutReturn(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "spell silence nombreFuncion(<tipoParametro> <nombreVariable>) -> {\n" +
                "    ~Cuerpo de la función sin retorno~\n" +
                "} endSpell"
            );
        }
        private void InsertInvokeFunction(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor("invoke nombreFuncion();  ~Invoca una función~");
        }
        //Control
        private void InsertIfElseStructure(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "if_spell_say (condición) {\n" +
                "    ~Bloque si la condición es verdadera~\n" +
                "} if_fail_say (otraCondición) {\n" +
                "    ~Bloque si la otra condición es verdadera~\n" +
                "} if_fail {\n" +
                "    ~Bloque si ninguna condición es verdadera~\n" +
                "}"
            );
        }

        private void InsertWhileStructure(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "int i-> 0;\n\n" +
                "eternal_loop(i under 5) {\n" +
                "    reveal(\"Iteración \", i);\n" +
                "    i->i + 1;\n" +
                "}\n"
            );
        }


        private void InsertDoWhileStructure(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "int j-> 0;\n\n" +
                "eternal_loop_once {\n" +
                "    reveal(\"j=\", j);\n" +
                "    j->j + 1;\n" +
                "} (j under 3);\n"
            );
        }


        private void InsertForStructure(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "ancestral_loop(str int i-> 0; end i under 3; igm i -> i + 1) {\n" +
                "    reveal(\"Iteración \", i);\n" +
                "}\n"
            );
        }


        private void InsertSwitchStructure(object sender, RoutedEventArgs e)
        {
            InsertTextInEditor(
                "int i -> 1;{\n" + 
                "destiny_choose (i) {\n" +
                "    path 0: {\n" +
                "        reveal(\"Cero\");\n" +
                "        break;\n" +
                "    }\n" +
                "    path 1: {\n" +
                "        reveal(\"Uno\");\n" +
                "        break;\n" +
                "    }\n" +
                "    hidden_path: {\n" +
                "        reveal(\"Otro valor\");\n" +
                "    }\n" +
                "}"
            );
        }

        //Insertar en el editor de texto en la posición del cursor
        private void InsertTextInEditor(string text)
        {
            int caretIndex = Editor.CaretIndex;

            Editor.Text = Editor.Text.Insert(caretIndex, text);
            Editor.CaretIndex = caretIndex + text.Length;

            Editor.Focus(); //recibe el texto focus
        }
    }
}