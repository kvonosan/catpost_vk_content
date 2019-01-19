using System;
using System.Windows;

namespace Catpost_VK_Content
{
    /// <summary>
    /// Логика взаимодействия для Window5.xaml
    /// </summary>
    public partial class Window5 : Window
    {
        private MainWindow main;
        public Window5(MainWindow mainw)
        {
            InitializeComponent();
            main = mainw;
            main.CenterWindowOnScreen(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (textBox.Text == "")
            {
                Random rand = new Random();
                main.favorite_name = "Закладка " + rand.Next(100).ToString();
                e.Cancel = true;
                Hide();
            }
            else
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "")
            {
                MessageBox.Show("Введите имя закладки!");
            }
            else
            {
                main.favorite_name = textBox.Text;
                Hide();
            }
        }
    }
}
