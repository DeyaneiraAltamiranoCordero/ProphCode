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

                var lexer = new Lexer();
                var tokens = lexer.Tokenize(source);

                var parser = new Parser(tokens);
                var prog = parser.ParseProgram();

                var runner = new ProphCode.Core.Runtime.Interpreter(prog);

                // 1) Capturar salida en la consola del IDE (Consola TextBox)
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

                // 2) Input para scry: usa un InputBox sencillo
                runner.ReadLine = (prompt) =>
                {
                    // Opción A: Microsoft.VisualBasic.InputBox (agrega referencia Microsoft.VisualBasic)
                    return Microsoft.VisualBasic.Interaction.InputBox(
                        string.IsNullOrEmpty(prompt) ? "Ingrese un valor:" : prompt,
                        "scry()", ""
                    );

                    // Opción B (si no quieres Microsoft.VisualBasic):
                    // Crea tu propia ventana modal InputDialog y devuélvela aquí.
                };

                // limpia la consola y corre
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
