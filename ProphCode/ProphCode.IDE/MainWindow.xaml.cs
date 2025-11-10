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

                // 1) LEXER
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(source);

                var sb = new StringBuilder();
                sb.AppendLine("[LEX] Tokens:");
                foreach (var t in tokens)
                    sb.AppendLine($"{t.Kind,-20} \"{Escape(t.Lexeme)}\"  @ {t.Line}:{t.Col}");

                // 2) PARSER: intentar parsear el programa completo
                sb.AppendLine();
                sb.AppendLine("[PARSER] Árbol sintáctico:");

                try
                {
                    var parser = new Parser(tokens);
                    var prog = parser.ParseProgram();

                    // 3) Imprimir árbol AST completo
                    sb.AppendLine(AstPrinter.Print(prog));
                }
                catch (Exception exParse)
                {
                    sb.AppendLine();
                    sb.AppendLine("[Parse ERROR] " + exParse.Message);
                }

                // 4) Mostrar todo en la consola del IDE
                Consola.Text = sb.ToString();
                Consola.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Consola.Text = "[LEX ERROR] " + ex.Message;
                Consola.ScrollToEnd();
            }
        }

        // Utilidad auxiliar para escapar comillas, etc.
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

                // 2) PARSER (¡no uses ExprParser aquí!)
                var parser = new Parser(tokens);
                var prog = parser.ParseProgram();

                // 3) CAPTURAR Console.Write/WriteLine en la consola del IDE (opcional pero útil)
                var sw = new System.IO.StringWriter();
                var oldOut = Console.Out;
                Console.SetOut(sw);

                try
                {
                    // 4) INTÉRPRETE
                    var runner = new ProphCode.Core.Runtime.Interpreter(prog);
                    runner.Run();

                    // 5) Mostrar salida capturada
                    Console.Out.Flush();
                    Consola.Text = "[RUN] OK\n" + sw.ToString();
                }
                finally
                {
                    Console.SetOut(oldOut);
                }

                Consola.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Consola.Text = "[RUN ERROR] " + ex.Message + "\n" + Consola.Text;
                Consola.ScrollToEnd();
            }
        }

        // === Opcional: handlers del menú (si les pusiste Click en XAML) ===
        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            Editor.Clear();
            Consola.Clear();
        }

        private void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
