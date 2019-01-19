using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Catpost_VK_Content
{
    /// <summary>
    /// Логика взаимодействия для Window6.xaml
    /// </summary>
    public partial class Window6 : Window
    {
        private MainWindow main;
        public Window6(MainWindow mainw)
        {
            main = mainw;
            InitializeComponent();
            main.CenterWindowOnScreen(this);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // кнопку "авторизоваться" делаем неактивную
            button.IsEnabled = false;


            label.Content = "Идёт авторизация... подождите...";
            try
            {
                //если поле для логина было пустое
                if (string.IsNullOrWhiteSpace(textBox.Text.ToString()))
                {
                    MessageBox.Show("Введите логин!!!"); button.IsEnabled = true; return;
                }
                //если текст в поле для логина менее 4 символов
                else if (textBox.Text.ToString().Length < 4)
                {
                    MessageBox.Show("Введите корректный логин!!!"); button.IsEnabled = true; return;
                }
                //если текст в поле для пароля пустое
                else if (string.IsNullOrWhiteSpace(passwordBox.Password.ToString()))
                {
                    MessageBox.Show("Пароль!!!"); button.IsEnabled = true; return;
                }
                //если текст в поле для пароля менее 4 символов
                else if (passwordBox.Password.ToString().Length < 4)
                {
                    MessageBox.Show("Введите корректный пароль!!!"); button.IsEnabled = true; return;
                }

                // если всё ок, то создаем аналог HttpWenRequest в awesomium
                using (var client = new CookieAwareWebClient())
                {
                    // используем кодировку сайта UTF8
                    client.Encoding = Encoding.UTF8;

                    //передаем в post запросе логин и пароль от сайта catpost
                    var values = new NameValueCollection
                    {
                        {"submitted2","1" },
                        { "username", textBox.Text.ToString() },
                        { "password", passwordBox.Password.ToString() }
                    };
                    // отправляем post Запрос
                    client.UploadValues("https://tomnolane.ru/index.html", values);

                    string html = client.DownloadString("https://tomnolane.ru/login-home.php");

                    // правильная html страница должна содержать строку: Меню управления CatPost (если авторизация прошла успешно)
                    if (html.Contains("Меню управления CatPost"))
                    {
                        if (html.Contains("Пополните счёт"))
                        {
                            MessageBox.Show("У вас закончились оплаченные дни.");
                            label.Content = "У вас закончись оплаченные дни.\nЗайдите в свой личный кабинет на сайте https://tomnolane.ru/ \nи пополните баланс.";
                            label.Foreground = Brushes.Red;
                            button.IsEnabled = true;
                            return;
                        }

                        label.Content = "Успешно авторизовались.";
                        label.Foreground = Brushes.Black;
                        textBox.Visibility = Visibility.Hidden;
                        passwordBox.Visibility = Visibility.Hidden;
                        button.Visibility = Visibility.Hidden;
                        label_login.Visibility = Visibility.Hidden;
                        label_pass.Visibility = Visibility.Hidden;
                        main.authorized = true;
                        Hide();
                    }
                    // если при авторизации был введен неверный логин или пароль
                    else
                    {
                        MessageBox.Show("Авторизация не удалась.\nПроверьте правильность ввода логина и пароля.");
                        label.Content = "Авторизация не удалась.\nПроверьте правильность ввода логина и пароля.";
                        label.Foreground = Brushes.Red;
                        button.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
                label.Content = "Ошибка: " + ex.Message;
                label.Foreground = Brushes.Red;
                button.IsEnabled = true;
            }
        }

        public class CookieAwareWebClient : WebClient
        {
            public CookieAwareWebClient()
            {
                CookieContainer = new CookieContainer();
            }
            public CookieContainer CookieContainer { get; private set; }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.CookieContainer = CookieContainer;
                return request;
            }
        }
    }
}
