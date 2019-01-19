using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Catpost_VK_Content
{
    /// <summary>
    /// Логика взаимодействия для Window4.xaml
    /// </summary>
    public partial class Window4 : Window
    {
        private MainWindow main;
        public Window4(MainWindow mainw)
        {
            InitializeComponent();
            main = mainw;
            main.CenterWindowOnScreen(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            main.date = datetime.Text;
            Hide();
            main.post_add = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
