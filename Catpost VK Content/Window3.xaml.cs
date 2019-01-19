using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WpfAnimatedGif;

namespace Catpost_VK_Content
{
    class content
    {
        public string id;
        public string vk_id;
        public string name;
        public string tag;
        public TextBox edittag;
        public CheckBox check;
    }

    /// <summary>
    /// Логика взаимодействия для Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        private MainWindow main;
        private List<content> content_list = new List<content>();
        public Window3(MainWindow mainwindow)
        {
            InitializeComponent();
            main = mainwindow;
            main.CenterWindowOnScreen(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void LogError(string error)
        {
            stack.Children.Clear();
            Label label = new Label();
            TextBlock textblock = new TextBlock();
            textblock.Text = error;
            textblock.TextWrapping = TextWrapping.WrapWithOverflow;
            label.Content = textblock;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Children.Add(label);
        }

        public void Rendered()
        {

            stack.Children.Clear();

            var bitmapload = new BitmapImage();
            bitmapload.BeginInit();
            bitmapload.CacheOption = BitmapCacheOption.OnLoad;
            bitmapload.UriSource = new Uri("pack://application:,,,/Images/giphy.gif");
            bitmapload.EndInit();

            Image Image1 = new Image();
            Image1.Width = 400;
            Image1.Height = 400;
            Image1.Source = bitmapload;

            ImageBehavior.SetAnimatedSource(Image1, bitmapload);
            ImageBehavior.SetRepeatBehavior(Image1, new RepeatBehavior(0));
            ImageBehavior.SetRepeatBehavior(Image1, RepeatBehavior.Forever);

            stack.Children.Add(Image1);

            if (main.Extended)
            {
                try
                {
                    string connStr = "server=peshkova-natalia.ru;user=root;database=catpost_content_vk;port=3306;password=test1234;Character Set=utf8mb4;";
                    MySqlConnection conn = new MySqlConnection(connStr);
                    conn.Open();

                    string sql1 = "SELECT * FROM groups_and_users";
                    MySqlCommand cmd1 = new MySqlCommand(sql1, conn);
                    cmd1.Prepare();
                    MySqlDataReader reader1 = cmd1.ExecuteReader();

                    stack.Children.Clear();
                    content_list.Clear();
                    var button1 = new Button();
                    button1.Content = "Без категории";
                    button1.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                    {
                        main.categories = "";
                        main.Add(main.offset, main.sort_num);
                        Hide();
                    });
                    stack.Children.Add(button1);
                    var stack1 = new StackPanel() { Orientation = Orientation.Horizontal };
                    var checkbox = new Label();
                    checkbox.Content = "Показать";
                    checkbox.Width = 60;
                    var vk_idlabel = new Label();
                    vk_idlabel.Content = "vk ID";
                    vk_idlabel.Width = 80;
                    var namelabel = new Label();
                    namelabel.Content = "Ссылка на группу";
                    namelabel.Width = 200;
                    var category = new Label();
                    category.Content = "Категории";
                    stack1.Children.Add(checkbox);
                    stack1.Children.Add(vk_idlabel);
                    stack1.Children.Add(namelabel);
                    stack1.Children.Add(category);
                    stack.Children.Add(stack1);

                    string[] str = main.categories.Split(' ');
                    List<int> groups = new List<int>();
                    foreach (var s in str)
                    {
                        int result = 0;
                        int.TryParse(s, out result);
                        if (result != 0)
                        {
                            groups.Add(int.Parse(s));
                        }
                    }

                    while (reader1.Read())
                    {
                        var content_item = new content();
                        var stack2 = new StackPanel() { Orientation = Orientation.Horizontal };
                        var checkbox1 = new CheckBox();
                        checkbox1.Width = 60;
                        var selected = false;
                        foreach (var i in groups)
                        {
                            if (reader1["id"].ToString() == i.ToString())
                            {
                                selected = true;
                            }
                        }
                        checkbox1.IsChecked = selected;
                        content_item.check = checkbox1;

                        content_item.id = reader1["id"].ToString();

                        var vk_id = new TextBox();
                        vk_id.Text = reader1["vk_id"].ToString();
                        vk_id.Width = 80;
                        vk_id.IsReadOnly = true;
                        content_item.vk_id = reader1["vk_id"].ToString();

                        Label linkLabel = new Label();
                        Run linkText = new Run(reader1["name"].ToString());
                        Hyperlink link = new Hyperlink(linkText);

                        link.NavigateUri = new Uri(reader1["name"].ToString());

                        link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender1, RequestNavigateEventArgs e1)
                        {
                            Process.Start(new ProcessStartInfo(e1.Uri.AbsoluteUri));
                            e1.Handled = true;
                        });

                        linkLabel.Content = link;
                        linkLabel.Width = 200;
                        content_item.name = reader1["name"].ToString();

                        var tag = new TextBox();
                        tag.Text = reader1["tag"].ToString();
                        tag.Width = 140;
                        content_item.tag = reader1["tag"].ToString();
                        content_item.edittag = tag;

                        stack2.Children.Add(checkbox1);
                        stack2.Children.Add(vk_id);
                        stack2.Children.Add(linkLabel);
                        stack2.Children.Add(tag);
                        stack.Children.Add(stack2);
                        content_list.Add(content_item);
                    }
                    reader1.Close();

                    var button = new Button();
                    button.Content = "Сохранить категории";
                    button.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                    {
                        string sql = "UPDATE groups_and_users SET tag=@tag WHERE vk_id=@vk_id";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Prepare();
                        foreach (var content1 in content_list)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@tag", content1.edittag.Text);
                            cmd.Parameters.AddWithValue("@vk_id", content1.vk_id);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Сохранено!");
                    });
                    stack.Children.Add(button);

                    var button2 = new Button();
                    button2.Content = "Показать категории";
                    button2.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                    {
                        main.categories = "";
                        int checked_content = 0;
                        var first = true;
                        foreach (var content2 in content_list)
                        {
                            if (content2.check.IsChecked.Value)
                            {
                                if (first)
                                {
                                    main.categories += "AND group_id = " + content2.id + " ";
                                    first = false;
                                }
                                else
                                {
                                    main.categories += "OR group_id = " + content2.id + " ";
                                }
                                
                                checked_content++;
                            }
                        }
                        if (checked_content == 0)
                        {
                            MessageBox.Show("Выберите категорию!");
                            return;
                        }
                        main.Add(main.offset, main.sort_num);
                        Hide();
                    });
                    stack.Children.Add(button2);
                }
                catch (Exception ex)
                {
                    LogError("Ошибка: " + ex.Message);
                }
            }
            else
            {
                stack.Children.Clear();
                var button = new Button();
                button.Content = "Без категории";
                button.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                {
                    main.categories = "";
                    main.Add(main.offset, main.sort_num);
                    Hide();
                });
                stack.Children.Add(button);

                try
                {
                    string connStr = "server=peshkova-natalia.ru;user=root;database=catpost_content_vk;port=3306;password=test1234;Character Set=utf8mb4;";
                    MySqlConnection conn = new MySqlConnection(connStr);
                    conn.Open();

                    string sql1 = "SELECT * FROM groups_and_users";
                    MySqlCommand cmd1 = new MySqlCommand(sql1, conn);
                    cmd1.Prepare();
                    MySqlDataReader reader1 = cmd1.ExecuteReader();
                    content_list.Clear();
                    string[] str = main.categories.Split(' ');
                    List<int> groups = new List<int>();
                    foreach (var s in str)
                    {
                        int result = 0;
                        int.TryParse(s, out result);
                        if (result != 0)
                        {
                            groups.Add(int.Parse(s));
                        }
                    }

                    while (reader1.Read())
                    {
                        var content_item = new content();
                        content_item.id = reader1["id"].ToString();
                        content_item.vk_id = reader1["vk_id"].ToString();
                        content_item.name = reader1["name"].ToString();
                        content_item.tag = reader1["tag"].ToString();

                        if (content_item.tag == "")
                        {
                            content_item.tag = content_item.name;
                        }

                        CheckBox cb = new CheckBox();
                        var selected = false;
                        foreach (var i in groups)
                        {
                            if (reader1["id"].ToString() == i.ToString())
                            {
                                selected = true;
                            }
                        }
                        cb.IsChecked = selected;

                        cb.Content = content_item.tag;
                        content_item.check = cb;
                        stack.Children.Add(cb);
                        content_list.Add(content_item);
                    }

                    var button1 = new Button();
                    button1.Content = "Показать категории";
                    button1.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                    {
                        main.categories = "";
                        int checked_content = 0;
                        var first = true;
                        foreach (var content2 in content_list)
                        {
                            if (content2.check.IsChecked.Value)
                            {
                                if (first)
                                {
                                    main.categories += "AND group_id = " + content2.id + " ";
                                    first = false;
                                }
                                else
                                {
                                    main.categories += "OR group_id = " + content2.id + " ";
                                }

                                checked_content++;
                            }
                        }
                        if (checked_content == 0)
                        {
                            MessageBox.Show("Выберите категорию!");
                            return;
                        }
                        main.Add(main.offset, main.sort_num);
                        Hide();
                    });
                    stack.Children.Add(button1);
                }
                catch (Exception ex)
                {
                    LogError("Ошибка: " + ex.Message);
                }
            }
        }
    }
}
