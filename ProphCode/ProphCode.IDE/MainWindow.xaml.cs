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
            // TODO: aquí llamarás a tu compilador en ProphCode.Core con Editor.Text
            Consola.Text = "[SUCCESS] Compilación finalizada.\n[INFO] 0 errores o warnings.";
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
