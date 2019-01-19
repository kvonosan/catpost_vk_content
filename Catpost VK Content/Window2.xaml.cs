using System.Windows;

namespace Catpost_VK_Content
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        private MainWindow main;
        public Window2(MainWindow mainwindow)
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

        private void date_new_sort_Checked(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                main.sort_label.Content = date_new_sort.Content;
                main.sort_num = 1;
                main.offset = 0;
                main.prev.IsEnabled = false;
                main.next.IsEnabled = true;
                if (main.offset + 11 > main.posts_count)
                {
                    main.next.IsEnabled = false;
                }
                main.Add(main.offset, main.sort_num);
                Hide();
            }
        }

        private void data_old_sort_Checked(object sender, RoutedEventArgs e)
        {
            main.sort_label.Content = data_old_sort.Content;
            main.sort_num = 2;
            main.offset = 0;
            main.prev.IsEnabled = false;
            main.next.IsEnabled = true;
            if (main.offset + 11 > main.posts_count)
            {
                main.next.IsEnabled = false;
            }
            main.Add(main.offset, main.sort_num);
            Hide();
        }

        private void likes_sort_Checked(object sender, RoutedEventArgs e)
        {
            main.sort_label.Content = likes_sort.Content;
            main.sort_num = 3;
            main.offset = 0;
            main.prev.IsEnabled = false;
            main.next.IsEnabled = true;
            if (main.offset + 11 > main.posts_count)
            {
                main.next.IsEnabled = false;
            }
            main.Add(main.offset, main.sort_num);
            Hide();
        }

        private void reposts_sort_Checked(object sender, RoutedEventArgs e)
        {
            main.sort_label.Content = reposts_sort.Content;
            main.sort_num = 4;
            main.offset = 0;
            main.prev.IsEnabled = false;
            main.next.IsEnabled = true;
            if (main.offset + 11 > main.posts_count)
            {
                main.next.IsEnabled = false;
            }
            main.Add(main.offset, main.sort_num);
            Hide();
        }

        private void views_sort_Checked(object sender, RoutedEventArgs e)
        {
            main.sort_label.Content = views_sort.Content;
            main.sort_num = 5;
            main.offset = 0;
            main.prev.IsEnabled = false;
            main.next.IsEnabled = true;
            if (main.offset + 11 > main.posts_count)
            {
                main.next.IsEnabled = false;
            }
            main.Add(main.offset, main.sort_num);
            Hide();
        }

        private void likes_reposts_views_sort_Checked(object sender, RoutedEventArgs e)
        {
            main.sort_label.Content = likes_reposts_views_sort.Content;
            main.sort_num = 6;
            main.offset = 0;
            main.prev.IsEnabled = false;
            main.next.IsEnabled = true;
            if (main.offset + 11 > main.posts_count)
            {
                main.next.IsEnabled = false;
            }
            main.Add(main.offset, main.sort_num);
            Hide();
        }

        private void add_sort_Checked(object sender, RoutedEventArgs e)
        {
            main.sort_label.Content = add_sort.Content;
            main.sort_num = 7;
            main.offset = 0;
            main.prev.IsEnabled = false;
            main.next.IsEnabled = true;
            if (main.offset + 11 > main.posts_count)
            {
                main.next.IsEnabled = false;
            }
            main.Add(main.offset, main.sort_num);
            Hide();
        }
    }
}
