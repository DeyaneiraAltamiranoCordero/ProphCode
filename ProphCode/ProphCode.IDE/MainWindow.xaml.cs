using System.Windows;

using ProphCode.Core.Lexing;
using System.Text;

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
                // 1) Obtener el código fuente del editor
                // Si tu Editor es TextBox:
                string source = Editor.Text;

                // Si fuera RichTextBox, usa esto en su lugar:
                // string source = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;

                // 2) Ejecutar el lexer
                var lexer = new Lexer();
                var tokens = lexer.Tokenize(source);

                // 3) Mostrar los tokens en la consola
                var sb = new StringBuilder();
                sb.AppendLine("[LEX] Tokens:");
                foreach (var t in tokens)
                {
                    sb.AppendLine($"{t.Kind,-18} \"{Escape(t.Lexeme)}\"  @ {t.Line}:{t.Col}");
                }

                Consola.Text = sb.ToString();
                Consola.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Consola.Text = "[LEX ERROR] " + ex.Message;
                Consola.ScrollToEnd();
            }
        }

        private static string Escape(string s)
        {
            return s?.Replace("\\", "\\\\")
                     ?.Replace("\r", "\\r")
                     ?.Replace("\n", "\\n")
                     ?.Replace("\t", "\\t")
                     ?? "";
        }

        private void BtnEjecutar_Click(object sender, RoutedEventArgs e)
        {
            // TODO: aquí invocarás tu runtime/intérprete con el resultado de compilar
            Consola.AppendText("\n[INFO] Ejecutando...\nHello, World!");
            Consola.ScrollToEnd();
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
