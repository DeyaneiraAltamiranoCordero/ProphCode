using ProphCode.Core.Compiler;
using ProphCode.Core.Lexing;
using System.Text;
using System.Windows;

namespace ProphCode.IDE
{
    public partial class MainWindow : Window
    {
        private readonly Compiler compiler = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnCompilar_Click(object sender, RoutedEventArgs e)
        {
            
                string textEditor = Editor.Text;             // <- lo que escribiste
                var result = compiler.Compile(textEditor);         // <- fachada Core

                var sb = new StringBuilder();
                foreach (var d in result.Diagnostics)
                    sb.AppendLine($"[{d.Severity}] ({d.Line},{d.Col}) {d.Message}");

                if (result.Output.Any())
                {
                    sb.AppendLine("----- Salida -----");
                    foreach (var line in result.Output) sb.AppendLine(line);
                }

                Output.Text = sb.ToString();          // lo ves en la IU
            
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
            Output.AppendText("\n[INFO] Ejecutando...\nHello, World!");
            Output.ScrollToEnd();
        }

        // === Opcional: handlers del menú (si les pusiste Click en XAML) ===
        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            Editor.Clear();
            Output.Clear();
        }

        private void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
