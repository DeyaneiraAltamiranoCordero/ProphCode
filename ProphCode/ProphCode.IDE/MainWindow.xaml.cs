using ProphCode.Core.AST;
using ProphCode.Core.Lexing;
using ProphCode.Core.Parsing;
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

                //Aquí empieza el analisis lexer
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(source);

                var sb = new StringBuilder();
                sb.AppendLine("[LEX] Tokens:");
                foreach (var t in tokens)
                    sb.AppendLine($"{t.Kind,-20} \"{Escape(t.Lexeme)}\"  @ {t.Line}:{t.Col}");

                //Después pasa al parser para hacer el arbol y el analsis sintáctico
                sb.AppendLine();
                sb.AppendLine("[PARSER] Árbol sintáctico:");

                try
                {
                    var parser = new Parser(tokens);
                    var prog = parser.ParseProgram();

                   //hace el arbol(Lo imprime en realidad)
                    sb.AppendLine(AstPrinter.Print(prog));
                }
                catch (Exception exParse)
                {
                    sb.AppendLine();
                    sb.AppendLine("[Parse ERROR] " + exParse.Message);
                }

                //Muestra en el IDE el resultado
                Consola.Text = sb.ToString();
                Consola.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Consola.Text = "[LEX ERROR] " + ex.Message;
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

                var lexer = new Lexer();
                var tokens = lexer.Tokenize(source);

                var parser = new Parser(tokens);
                var prog = parser.ParseProgram();

                var runner = new ProphCode.Core.Runtime.Interpreter(prog);

                // Captuptura la salida en la consola del IDE
                var outBuffer = new StringBuilder();
                runner.WriteLine = (text) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Consola.AppendText(text + Environment.NewLine);
                        Consola.ScrollToEnd();
                    });
                    outBuffer.AppendLine(text);
                };

                //Maneja las entradas de scry()
                runner.ReadLine = (prompt) =>
                {
                    return Microsoft.VisualBasic.Interaction.InputBox(
                        string.IsNullOrEmpty(prompt) ? "Ingrese un valor:" : prompt,
                        "scry()", ""
                    );
                };

                // limpia la consola
                Consola.Text = "[RUN] Iniciando...\n";
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

        // Método auxiliar para insertar texto en el editor
        private void InsertTextInEditor(string text)
        {
            int caretIndex = Editor.CaretIndex;

            Editor.Text = Editor.Text.Insert(caretIndex, text);
            Editor.CaretIndex = caretIndex + text.Length;

            Editor.Focus(); //recibe el texto focus
        }
    }
}