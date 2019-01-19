using System.Windows;

namespace Catpost_VK_Content
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private MainWindow main;
        public Window1(MainWindow mainwindow)
        {
            InitializeComponent();
            main = mainwindow;
            main.CenterWindowOnScreen(this);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (!main.Extended)
            {
                if (textBox.Text == "555")
                {
                    main.Extended = true;
                    label.Visibility = Visibility.Hidden;
                    textBox.Visibility = Visibility.Hidden;
                    button.Visibility = Visibility.Hidden;
                    button1.Content = "Отключить";
                    Hide();
                    if (!main.loading)
                    {
                        main.Add(main.offset, main.sort_num);
                    }
                }
                else
                {
                    MessageBox.Show("Пароль неверный.");
                }
            }
            else
            {
                main.Extended = false;
                label.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Visible;
                button.Visibility = Visibility.Visible;
                button1.Content = "Включить";
                Hide();
                if (!main.loading)
                {
                    main.Add(main.offset, main.sort_num);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
