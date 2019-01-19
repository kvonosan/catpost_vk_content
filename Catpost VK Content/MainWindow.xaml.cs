using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using WpfAnimatedGif;

namespace Catpost_VK_Content
{
    public class VOKRUG
    {
        public string title { get; set; } = "";
        public string text { get; set; } = "";
        public string url { get; set; } = "";
        public string origin { get; set; } = "Журнал Вокруг Света";
        public string img { get; set; } = "";
        public string hash;
        public byte[] buffer;
    }

    public class Life_RU
    {
        public string header { get; set; } = "";
        public string text { get; set; } = "";
        public string date_time { get; set; } = "";
        public string url { get; set; } = "";
        public int hash_id { get; set; } = 0;
        public byte[] buffer;
    }

    public class Favorites
    {
        public int offset;
        public int sort_num;
        public string name;
        public string categories;
    }

    /// <summary>
    /// Класс пост вконтакте.
    /// </summary>
    public class Post
    {
        public int id; //идентификатор поста в базе.
        public int group_id; //идентификатор группы вконтакте.
        public int vk_id; //идентификатор поста вконтакте.
        public long date; //дата в которую выложили пост.
        public string text; //текст поста.
        public int likes; //сколько лайков у поста.
        public int reposts; //сколько репостов у поста.
        public int views; //сколько просмотров у поста.
        public string repost_text; //текст, если это репост поста (проверяется если нет основного текста).
        public int owner_id; //идентификатор владельца поста.
        public int who_add; //кто добавил пост в базу(идентификатор пользователя вконтакте).
        public int trash; //если = 1 то пост мусорный и он не показывается.
        //public List<Attachment> attachments = new List<Attachment>(); //прикрепления поста (фото, гифки).
    }

    /// <summary>
    /// Класс прикрепление к посту вконтакте.
    /// </summary>
    public class Attachment
    {
        public int id; //идентификатор прикрепления вконтакте в базе.
        public int group_id; //идентификатор группы вконтакте.
        public int post_id; //идентификатор поста в базе.
        public string type; //тип прикрепления: photo или doc.

        //photo
        public int vk_id; //идентификатор прикрепления вконтакте.
        public int owner_id; //идентификатор владельца прикрепления.
        public string link; //ссылка для загрузки(photo или doc).
        public long date; //дата в которую выложили прикрепление.

        //doc проверяется по заголовку(так как могут быть другие doc) mime type = image/gif.
        //vk_id имеется.
        //owner_id имеется.
        //link имеется.
        //date не имеется.
    }

    public class Loaded_Attachments
    {
        public Attachment attach;
        public byte[] buffer;
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int posts_count = 0;
        public int offset = 0;
        public bool Extended = false;
        public int sort_num = 1;
        public bool loading = false;
        public string categories = "";
        public string date = "";
        public bool post_add = false;
        public string favorite_name;
        public bool authorized;
        private MySqlConnection conn = new MySqlConnection();
        private List<Post> posts = new List<Post>();
        private List<Loaded_Attachments> attachments = new List<Loaded_Attachments>();
        private List<Loaded_Attachments> current_attachments = new List<Loaded_Attachments>();
        private int PostWidth = 600;
        private Window1 window;
        private Window2 window_sort;
        private Window3 window_content;
        private Window4 window_date;
        private Window5 window_fave_name;
        private Window6 window_auth;
        private int post_index = 0;
        private bool next_temp = false;
        private bool prev_temp = false;
        private List<byte[]> buffers_gif = new List<byte[]>();
        private List<Favorites> favorites_list = new List<Favorites>();
        private PipeClient client;
        private bool news_loaded = false;
        private bool goroskopes_loaded = false;
        private bool vokrug_loaded = false;
        private List<Life_RU> life_all = new List<Life_RU>();
        private List<VOKRUG> list_vokrug = new List<VOKRUG>(); // тут собирается готовый результат
        private List<VOKRUG> all_list_vokrug = new List<VOKRUG>();
        private static Object thisLock = new Object();
        private int NewsLoader = 0;
        private int VokrugLoader = 272960;

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void LogError(string error, int tab = 0)
        {
            
            Label label = new Label();
            TextBlock textblock = new TextBlock();
            textblock.Text = error;
            textblock.TextWrapping = TextWrapping.WrapWithOverflow;
            label.Content = textblock;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            if (tab == 0)
            {
                stack.Children.Clear();
                stack.Children.Add(label);
                update_button.IsEnabled = true;
            }
            else if (tab == 2)
            {
                goroskope.Children.Clear();
                goroskope.Children.Add(label);
            }
            else if (tab == 1)
            {
                news.Children.Clear();
                news.Children.Add(label);
            }
            else if (tab == 3)
            {
                pogoda.Children.Clear();
                pogoda.Children.Add(label);
            }
            else if (tab == 4)
            {
                vokrug_sveta.Children.Clear();
                vokrug_sveta.Children.Add(label);
            }
        }

        public void CenterWindowOnScreen(Window window)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = window.Width;
            double windowHeight = window.Height;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
            window.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        public async void Add(int offset, int sort)
        {
            try
            {
                loading = true;
                next_temp = next.IsEnabled;
                prev_temp = prev.IsEnabled;
                next.IsEnabled = false;
                prev.IsEnabled = false;
                sort_button.IsEnabled = false;
                category_button.IsEnabled = false;
                update_button.IsEnabled = false;
                favorites.IsEnabled = false;

                stack.Children.Clear();
                stack.Children.Add(getLoader());
                stack_posts.Children.Add(getTabloader());

                post_index = 0;
                posts_count = 0;

                await Task.Run(() =>
                {
                    AddPost(offset, sort);
                });

                if (posts_count == 0)
                {
                    stack_posts.Children.Clear();
                    LogError("Ошибка: 0 постов в базе!");
                    next.IsEnabled = false;
                    prev.IsEnabled = false;
                    sort_button.IsEnabled = true;
                    category_button.IsEnabled = true;
                    update_button.IsEnabled = true;
                    loading = false;
                    return;
                }
                else
                {
                    if (offset > 0)
                    {
                        prev_temp = true;
                    }
                    if (offset == 0)
                    {
                        prev_temp = false;
                    }
                    if (offset < posts_count)
                    {
                        next_temp = true;
                    }
                    if (offset + 11 > posts_count)
                    {
                        next_temp = false;
                    }
                }

                stack.Children.Clear();
                buffers_gif.Clear();
                foreach (var post in posts)
                {
                    if (post.trash == 1)
                    {
                        continue;
                    }
                    StackPanel fone = new StackPanel();
                    fone.Name = "post" + post.id.ToString();
                    fone.Background = Brushes.Gray;
                    string posttext = post.text != "" ? post.text : post.repost_text;
                    if (posttext != "")
                    {
                        TextBlock textblock = new TextBlock();
                        textblock.Width = PostWidth;
                        textblock.TextWrapping = TextWrapping.WrapWithOverflow;
                        textblock.Text = posttext;
                        fone.Children.Add(textblock);
                    }

                    current_attachments.Clear();
                    foreach (Loaded_Attachments attach1 in attachments)
                    {
                        if (attach1.attach.post_id == post.id)
                        {
                            current_attachments.Add(attach1);
                        }
                    }

                    foreach (Loaded_Attachments attach1 in current_attachments)
                    {
                        if (attach1.attach.type == "photo")
                        {
                            var stream = new MemoryStream(attach1.buffer);
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();

                            Image myImage = new Image();
                            myImage.Width = PostWidth;
                            myImage.Height = 400;
                            myImage.Source = bitmap;

                            fone.Children.Add(myImage);
                        }
                        else if (attach1.attach.type == "doc")
                        {
                            buffers_gif.Add(attach1.buffer);
                            var bitmap1 = new BitmapImage();
                            var mem = new MemoryStream(attach1.buffer);
                            bitmap1.BeginInit();
                            bitmap1.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap1.StreamSource = mem;
                            bitmap1.EndInit();

                            Image myImage1 = new Image();
                            myImage1.Width = PostWidth;
                            myImage1.Height = 400;
                            myImage1.Source = bitmap1;

                            ImageBehavior.SetAnimatedSource(myImage1, bitmap1);
                            ImageBehavior.SetRepeatBehavior(myImage1, new RepeatBehavior(0));
                            ImageBehavior.SetRepeatBehavior(myImage1, RepeatBehavior.Forever);

                            fone.Children.Add(myImage1);
                        }
                    }
                    stack.Children.Add(fone);
                    StackPanel stack_horizontal = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };

                    var bitmaplike = new BitmapImage();
                    bitmaplike.BeginInit();
                    bitmaplike.CacheOption = BitmapCacheOption.OnLoad;
                    bitmaplike.UriSource = new Uri("pack://application:,,,/Images/like.png");
                    bitmaplike.EndInit();

                    Image Image_like = new Image();
                    Image_like.Width = 25;
                    Image_like.Height = 25;
                    Image_like.Source = bitmaplike;
                    stack_horizontal.Children.Add(Image_like);

                    var label_like = new Label();
                    label_like.Content = post.likes.ToString();
                    stack_horizontal.Children.Add(label_like);

                    var bitmaprepost = new BitmapImage();
                    bitmaprepost.BeginInit();
                    bitmaprepost.CacheOption = BitmapCacheOption.OnLoad;
                    bitmaprepost.UriSource = new Uri("pack://application:,,,/Images/repost.png");
                    bitmaprepost.EndInit();

                    Image Image_repost = new Image();
                    Image_repost.Width = 25;
                    Image_repost.Height = 25;
                    Image_repost.Source = bitmaprepost;
                    stack_horizontal.Children.Add(Image_repost);

                    var label_repost = new Label();
                    label_repost.Content = post.reposts.ToString();
                    stack_horizontal.Children.Add(label_repost);

                    var bitmapviews = new BitmapImage();
                    bitmapviews.BeginInit();
                    bitmapviews.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapviews.UriSource = new Uri("pack://application:,,,/Images/views.png");
                    bitmapviews.EndInit();

                    Image Image_views = new Image();
                    Image_views.Width = 25;
                    Image_views.Height = 25;
                    Image_views.Source = bitmapviews;
                    stack_horizontal.Children.Add(Image_views);

                    var label_views = new Label();
                    label_views.Content = post.views.ToString();
                    stack_horizontal.Children.Add(label_views);

                    var label_date = new Label();
                    label_date.Content = " Дата: " + UnixTimeStampToDateTime(post.date).ToString();
                    stack_horizontal.Children.Add(label_date);

                    Label linkLabel = new Label();
                    Run linkText = new Run("https://vk.com/wall" + post.owner_id.ToString() + "_" + post.vk_id.ToString());
                    Hyperlink link = new Hyperlink(linkText);

                    link.NavigateUri = new Uri("https://vk.com/wall" + post.owner_id.ToString() + "_" + post.vk_id.ToString());

                    link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender, RequestNavigateEventArgs e)
                    {
                        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                        e.Handled = true;
                    });

                    linkLabel.Content = link;

                    stack_horizontal.Children.Add(linkLabel);

                    var trashButton = new Button();
                    trashButton.Content = "Удалить";
                    trashButton.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                    {
                        if (trashButton.Content.ToString() == "Удалить")
                        {
                            string sql = "UPDATE posts_vk SET trash=1 WHERE vk_id=@post_vk_id";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@post_vk_id", post.vk_id);
                            cmd.ExecuteNonQuery();
                            trashButton.Content = "Восстановить";
                        }
                        else
                        {
                            string sql = "UPDATE posts_vk SET trash=0 WHERE vk_id=@post_vk_id";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@post_vk_id", post.vk_id);
                            cmd.ExecuteNonQuery();
                            trashButton.Content = "Удалить";
                        }
                    });

                    if (Extended)
                    {
                        stack_horizontal.Children.Add(trashButton);
                    }

                    var addbutton = new Button();
                    addbutton.Content = "Запостить";
                    addbutton.Margin = new Thickness(10, 0, 0, 0);
                    addbutton.Name = "post" + post.id.ToString();
                    addbutton.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                    {
                        window_date.datetime.Value = DateTime.Now;
                        post_add = false;
                        window_date.ShowDialog();
                        if (post_add)
                        {
                            string text = "";
                            int i = 0;
                            List<string> images = new List<string>();
                            int gif_index = 0;
                            foreach (var child in stack.Children)
                            {
                                string childname = "no name";
                                if (child is FrameworkElement)
                                {
                                    childname = (child as FrameworkElement).Name;
                                }
                                if (childname == addbutton.Name && child is StackPanel)
                                {
                                    foreach (var child1 in (child as StackPanel).Children)
                                    {
                                        if (child1 is TextBlock)
                                        {
                                            text = (child1 as TextBlock).Text;
                                        }
                                        if (child1 is Image)
                                        {
                                            i++;
                                            var image = child1 as Image;
                                            if (!Directory.Exists("Files/"))
                                            {
                                                Directory.CreateDirectory("Files");
                                            }
                                            FileStream fileStream;
                                            BitmapEncoder encoder = new PngBitmapEncoder();
                                            if (image.Source is RenderTargetBitmap)
                                            {
                                                fileStream = new FileStream("Files/image_" + post.id + "_" + i + ".gif", FileMode.Create);
                                                Stream stream = new MemoryStream(buffers_gif[gif_index]);
                                                stream.CopyTo(fileStream);
                                                images.Add("image_" + post.id + "_" + i + ".gif");
                                                gif_index++;
                                            }
                                            else
                                            {
                                                fileStream = new FileStream("Files/image_" + post.id + "_" + i + ".png", FileMode.Create);
                                                encoder.Frames.Add(BitmapFrame.Create(image.Source as BitmapSource));
                                                images.Add("image_" + post.id + "_" + i + ".png");
                                                encoder.Save(fileStream);
                                            }
                                            fileStream.Close();
                                        }
                                    }
                                }
                                else
                                {
                                    if (child is StackPanel)
                                    {
                                        foreach (var child1 in (child as StackPanel).Children)
                                        {
                                            if (child1 is Image)
                                            {
                                                var image = child1 as Image;
                                                if (image.Source is RenderTargetBitmap)
                                                {
                                                    gif_index++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            JObject global = new JObject(new JProperty("text", text), new JProperty("date", date), new JProperty("link", "https://vk.com/wall" + post.owner_id.ToString() + "_" + post.vk_id.ToString()));
                            int j = 0;
                            foreach (var image in images)
                            {
                                j++;
                                global.Add(new JProperty("image" + j.ToString(), image));
                            }
                            File.WriteAllText(@"Files/post_" + post.id + ".json", global.ToString());
                        }
                    });

                    stack_horizontal.Children.Add(addbutton);
                    stack.Children.Add(stack_horizontal);

                    Separator separator = new Separator();
                    separator.Height = 50;
                    stack.Children.Add(separator);
                    post_index++;
                }
                next.IsEnabled = next_temp;
                prev.IsEnabled = prev_temp;
                sort_button.IsEnabled = true;
                category_button.IsEnabled = true;
                update_button.IsEnabled = true;
                favorites.IsEnabled = true;
                scroll.ScrollToTop();
                loading = false;
                if (posts_count == 0)
                {
                    posts_count_label.Content = "";
                }
                else
                {
                    posts_count_label.Content = "Всего постов в базе " + posts_count.ToString() + ".";
                }
                stack_posts.Children.Clear();
            }
            catch (Exception ex)
            {
                conn.Close();
                stack_posts.Children.Clear();
                LogError("Ошибка: " + ex.Message);
            }
        }

        private void AddPost(int offset, int sort)
        {
            posts.Clear();
            attachments.Clear();

            if (conn.State.ToString() != "Open")
            {
                string connStr = "server=peshkova-natalia.ru;user=root;database=catpost_content_vk;port=3306;password=test1234;Character Set=utf8mb4;";
                conn = new MySqlConnection(connStr);
                conn.Open();
            }

            string sql = "";
            string sql1 = "";
            if (sort == 1)
            {
                sql = "SELECT * FROM posts_vk WHERE trash=0 " + categories + " ORDER BY date DESC LIMIT 10 OFFSET @offset";
                sql1 = "SELECT COUNT(id) FROM posts_vk WHERE trash=0 " + categories + " ORDER BY date DESC LIMIT 10";
            }
            if (sort == 2)
            {
                sql = "SELECT * FROM posts_vk WHERE trash=0 " + categories + " ORDER BY date LIMIT 10 OFFSET @offset";
                sql1 = "SELECT COUNT(id) FROM posts_vk WHERE trash=0 " + categories + " ORDER BY date LIMIT 10";
            }
            if (sort == 3)
            {
                sql = "SELECT * FROM posts_vk WHERE trash=0 " + categories + " ORDER BY likes DESC LIMIT 10 OFFSET @offset";
                sql1 = "SELECT COUNT(id) FROM posts_vk WHERE trash=0 " + categories + " ORDER BY likes DESC LIMIT 10";
            }
            if (sort == 4)
            {
                sql = "SELECT * FROM posts_vk WHERE trash=0 " + categories + " ORDER BY reposts DESC LIMIT 10 OFFSET @offset";
                sql1 = "SELECT COUNT(id) FROM posts_vk WHERE trash=0 " + categories + " ORDER BY reposts DESC LIMIT 10";
            }
            if (sort == 5)
            {
                sql = "SELECT * FROM posts_vk WHERE trash=0 " + categories + " ORDER BY views DESC LIMIT 10 OFFSET @offset";
                sql1 = "SELECT COUNT(id) FROM posts_vk WHERE trash=0 " + categories + " ORDER BY views DESC LIMIT 10";
            }
            if (sort == 6)
            {
                sql = "SELECT * FROM posts_vk WHERE trash=0 " + categories + " ORDER BY likes DESC, reposts DESC, views DESC LIMIT 10 OFFSET @offset";
                sql1 = "SELECT COUNT(id) FROM posts_vk WHERE trash=0 " + categories + " ORDER BY likes DESC, reposts DESC, views DESC LIMIT 10";
            }
            if (sort == 7)
            {
                sql = "SELECT * FROM posts_vk WHERE trash=0 " + categories + " LIMIT 10 OFFSET @offset";
                sql1 = "SELECT COUNT(id) FROM posts_vk WHERE trash=0 " + categories + " LIMIT 10";
            }

            MySqlCommand cmd1 = new MySqlCommand(sql1, conn);
            cmd1.CommandTimeout = 2147483;
            cmd1.Prepare();
            MySqlDataReader reader1 = cmd1.ExecuteReader();

            posts_count = 0;
            while (reader1.Read())
            {
                posts_count = int.Parse(reader1[0].ToString());
            }
            reader1.Close();

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@offset", offset);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Post post = new Post();
                post.id = int.Parse(reader["id"].ToString());
                post.vk_id = int.Parse(reader["vk_id"].ToString());
                post.date = int.Parse(reader["date"].ToString());
                post.text = reader["text"].ToString();
                post.likes = int.Parse(reader["likes"].ToString());
                post.reposts = int.Parse(reader["reposts"].ToString());
                post.views = int.Parse(reader["views"].ToString());
                post.repost_text = reader["repost_text"].ToString();
                post.owner_id = int.Parse(reader["owner_id"].ToString());
                post.who_add = int.Parse(reader["who_add"].ToString());
                post.trash = int.Parse(reader["trash"].ToString());
                posts.Add(post);
            }
            reader.Close();

            foreach (Post post in posts)
            {
                string sql2 = "SELECT * FROM attachments_vk WHERE post_id = @id";
                MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                cmd2.Prepare();
                cmd2.Parameters.AddWithValue("@id", post.id);
                MySqlDataReader reader2 = cmd2.ExecuteReader();
                int count = 0;
                List<Loaded_Attachments> attach_to_add = new List<Loaded_Attachments>();
                while (reader2.Read())
                {
                    Attachment attach = new Attachment();
                    attach.post_id = int.Parse(reader2["post_id"].ToString());
                    attach.type = reader2["type"].ToString();
                    attach.vk_id = int.Parse(reader2["vk_id"].ToString());
                    attach.owner_id = int.Parse(reader2["owner_id"].ToString());
                    attach.link = reader2["link"].ToString();
                    attach.date = int.Parse(reader2["date"].ToString());

                    if (attach.type == "doc")
                    {
                        count++;
                    }

                    if (count > 2)
                    {
                        string connStr = "server=peshkova-natalia.ru;user=root;database=catpost_content_vk;port=3306;password=test1234;Character Set=utf8mb4;";
                        MySqlConnection conn1 = new MySqlConnection(connStr);
                        conn1.Open();
                        string sql3 = "UPDATE posts_vk SET trash=1 WHERE id = @id";
                        MySqlCommand cmd3 = new MySqlCommand(sql3, conn1);
                        cmd3.Prepare();
                        cmd3.Parameters.AddWithValue("@id", post.id);
                        cmd3.ExecuteNonQuery();
                        conn1.Close();
                        post.trash = 1;
                        continue;
                    }

                    var loaded = new Loaded_Attachments();
                    loaded.attach = attach;

                    WebClient wc = new WebClient();
                    var stream = wc.OpenRead(attach.link);
                    Int64 bytes_total = Convert.ToInt64(wc.ResponseHeaders["Content-Length"]);
                    var type = wc.ResponseHeaders["Content-Type"];
                    stream.Close();

                    if (attach.type == "doc" && type != "image/gif")
                    {
                        string connStr = "server=peshkova-natalia.ru;user=root;database=catpost_content_vk;port=3306;password=test1234;Character Set=utf8mb4;";
                        MySqlConnection conn1 = new MySqlConnection(connStr);
                        conn1.Open();
                        string sql3 = "UPDATE posts_vk SET trash=1 WHERE id = @id";
                        MySqlCommand cmd3 = new MySqlCommand(sql3, conn1);
                        cmd3.Prepare();
                        cmd3.Parameters.AddWithValue("@id", post.id);
                        cmd3.ExecuteNonQuery();
                        conn1.Close();
                        post.trash = 1;
                    }

                    if (bytes_total < 5242880 && count <= 2)
                    {
                        loaded.buffer = new WebClient().DownloadData(attach.link);
                        attach_to_add.Add(loaded);
                    }
                    else
                    {
                        string connStr = "server=peshkova-natalia.ru;user=root;database=catpost_content_vk;port=3306;password=test1234;Character Set=utf8mb4;";
                        MySqlConnection conn1 = new MySqlConnection(connStr);
                        conn1.Open();
                        string sql3 = "UPDATE posts_vk SET trash=1 WHERE id = @id";
                        MySqlCommand cmd3 = new MySqlCommand(sql3, conn1);
                        cmd3.Prepare();
                        cmd3.Parameters.AddWithValue("@id", post.id);
                        cmd3.ExecuteNonQuery();
                        conn1.Close();
                        post.trash = 1;
                    }
                }
                if (count <= 2)
                {
                    foreach (var att in attach_to_add)
                    {
                        attachments.Add(att);
                    }
                }
                reader2.Close();
            }
        }

        private bool Update()
        {
            if (File.Exists("update.date"))
            {
                string date = File.ReadAllText("update.date");
                var src = DateTime.Now;
                var hm = DateTime.Now.AddHours(-1);
                if (hm > DateTime.Parse(date))
                {
                    File.WriteAllText("update.date", src.ToString());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                File.WriteAllText("update.date", DateTime.Now.ToString());
                return true;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            CenterWindowOnScreen(this);
            window = new Window1(this);
            window_sort = new Window2(this);
            window_content = new Window3(this);
            window_date = new Window4(this);
            window_fave_name = new Window5(this);
            window_auth = new Window6(this);

            if (Update())
            {
                client = new PipeClient();
                client.Update();
            }

            authorized = false;
            window_auth.ShowDialog();
            if (!authorized)
            {
                Application.Current.Shutdown();
            }

            if (!File.Exists("favorites.ini"))
            {
                File.WriteAllText("favorites.ini", "");
            }

            favorites.Items.Add("Сохранить закладку");
            favorites.Items.Add("Очистить закладки");

            List<Favorites> fav = JsonConvert.DeserializeObject<List<Favorites>>(File.ReadAllText("favorites.ini"));
            if (fav != null)
            {
                foreach (var f in fav)
                {
                    favorites_list.Add(f);
                    favorites.Items.Add(f.name);
                }
            }

            Add(offset, sort_num);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            offset += 10;
            if (offset > 0)
            {
                prev.IsEnabled = true;
            }
            if (offset + 11 > posts_count)
            {
                next.IsEnabled = false;
            }
            favorites.SelectedIndex = -1;
            Add(offset, sort_num);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (offset == 20)
            {
                offset -= 20;
            }
            else
            {
                offset -= 10;
            }
            if (offset == 0)
            {
                prev.IsEnabled = false;
            }
            if (offset < posts_count)
            {
                next.IsEnabled = true;
            }
            favorites.SelectedIndex = -1;
            Add(offset, sort_num);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.R) ||
                Keyboard.IsKeyDown(Key.RightCtrl) && Keyboard.IsKeyDown(Key.R))
            {
                window.ShowDialog();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            File.WriteAllText("favorites.ini", JsonConvert.SerializeObject(favorites_list));
            Application.Current.Shutdown();
        }

        private void sort_button_Click(object sender, RoutedEventArgs e)
        {
            if (sort_num == 1)
            {
                window_sort.date_new_sort.IsChecked = true;
            }
            else if (sort_num == 2)
            {
                window_sort.data_old_sort.IsChecked = true;
            }
            else if (sort_num == 3)
            {
                window_sort.likes_sort.IsChecked = true;
            }
            else if (sort_num == 4)
            {
                window_sort.reposts_sort.IsChecked = true;
            }
            else if (sort_num == 5)
            {
                window_sort.views_sort.IsChecked = true;
            }
            else if (sort_num == 6)
            {
                window_sort.likes_reposts_views_sort.IsChecked = true;
            }
            else if (sort_num == 7)
            {
                window_sort.add_sort.IsChecked = true;
            }
            favorites.SelectedIndex = -1;
            window_sort.Show();
        }

        private void category_button_Click(object sender, RoutedEventArgs e)
        {
            favorites.SelectedIndex = -1;
            window_content.Rendered();
            window_content.Show();
        }

        private void Update_button_Click(object sender, RoutedEventArgs e)
        {
            if (conn.State.ToString() != "Open")
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    LogError("Ошибка: " + ex.Message);
                }
            }
            Add(offset, sort_num);
        }

        private void favorites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (favorites.SelectedIndex == 0)
            {
                favorite_name = "";
                window_fave_name.ShowDialog();
                Favorites fave = new Favorites();
                fave.name = favorite_name;
                fave.offset = offset;
                fave.sort_num = sort_num;
                fave.categories = categories;
                favorites_list.Add(fave);
                favorites.Items.Add(fave.name);
                favorites.SelectedIndex = -1;
            }
            else if (favorites.SelectedIndex == 1)
            {
                favorites_list.Clear();
                favorites.Items.Clear();
                favorites.Items.Add("Сохранить закладку");
                favorites.Items.Add("Очистить закладки");
            }
            else if (favorites.SelectedIndex != -1)
            {
                offset = favorites_list[favorites.SelectedIndex - 2].offset;
                sort_num = favorites_list[favorites.SelectedIndex - 2].sort_num;
                categories = favorites_list[favorites.SelectedIndex - 2].categories;
                if (sort_num == 1)
                {
                    sort_label.Content = "По дате - новое вначале";
                }
                else if (sort_num == 2)
                {
                    sort_label.Content = "По дате - старое вначале";
                }
                else if (sort_num == 3)
                {
                    sort_label.Content = "По лайкам";
                }
                else if (sort_num == 4)
                {
                    sort_label.Content = "По репостам";
                }
                else if (sort_num == 5)
                {
                    sort_label.Content = "По просмотрам";
                }
                else if (sort_num == 6)
                {
                    sort_label.Content = "По лайкам, репостам, просмотрам";
                }
                else if (sort_num == 7)
                {
                    sort_label.Content = "Без сортировки, как добавлено в базу данных";
                }

                Add(offset, sort_num);
            }
        }

        private string getgoroskope(string link)
        {
            var web = new WebClient();
            var result = web.DownloadData(link);
            string data = Encoding.UTF8.GetString(result);
            var index = data.IndexOf("<div class=\"horoBlock\">");
            if (index > 0)
            {
                var index2 = data.IndexOf("<p class=\"\">", index);
                if (index2 > 0)
                {
                    var index3 = data.IndexOf("</p>", index2);
                    if (index3 > 0)
                    {
                        string goro = data.Substring(index2 + 22, index3 - index2 - 22);
                        return goro;
                    }
                }
            }
            return "";
        }

        public Label getLink(string url)
        {
            Label linkLabel = new Label();
            linkLabel.Width = PostWidth;
            Run linkText = new Run("Источник: " + url);
            Hyperlink link = new Hyperlink(linkText);

            link.NavigateUri = new Uri(url);

            link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender1, RequestNavigateEventArgs e1)
            {
                Process.Start(new ProcessStartInfo(e1.Uri.AbsoluteUri));
                e1.Handled = true;
            });

            linkLabel.Content = link;
            return linkLabel;
        }

        public TextBlock getGoroTextBlock(string goro)
        {
            var text = new TextBlock();
            text.Text = goro;
            text.Width = PostWidth;
            text.TextWrapping = TextWrapping.WrapWithOverflow;
            return text;
        }

        public Button getGoroButton(string goro, string name, string link)
        {
            var button = new Button();
            button.Content = "Запостить";
            button.Width = PostWidth;
            button.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
            {
                window_date.datetime.Value = DateTime.Now;
                post_add = false;
                window_date.ShowDialog();
                if (post_add)
                {
                    if (!Directory.Exists("Files/"))
                    {
                        Directory.CreateDirectory("Files");
                    }
                    JObject global = new JObject(new JProperty("text", goro), new JProperty("date", date), new JProperty("link", link));
                    File.WriteAllText(@"Files/post_" + name + ".json", global.ToString());
                }
            });
            return button;
        }

        public Image getLoader()
        {
            var bitmapload = new BitmapImage();
            bitmapload.BeginInit();
            bitmapload.CacheOption = BitmapCacheOption.OnLoad;
            bitmapload.UriSource = new Uri("pack://application:,,,/Images/giphy.gif");
            bitmapload.EndInit();

            Image image = new Image();
            image.Width = PostWidth;
            image.Height = 400;
            image.Source = bitmapload;

            ImageBehavior.SetAnimatedSource(image, bitmapload);
            ImageBehavior.SetRepeatBehavior(image, new RepeatBehavior(0));
            ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);

            return image;
        }

        public Image getTabloader()
        {
            var bitmapload = new BitmapImage();
            bitmapload.BeginInit();
            bitmapload.CacheOption = BitmapCacheOption.OnLoad;
            bitmapload.UriSource = new Uri("pack://application:,,,/Images/small_loader.gif");
            bitmapload.EndInit();

            Image image = new Image();
            image.Height = 10;
            image.Width = 10;
            image.Source = bitmapload;

            ImageBehavior.SetAnimatedSource(image, bitmapload);
            ImageBehavior.SetRepeatBehavior(image, new RepeatBehavior(0));
            ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);

            return image;
        }

        public async void LoadNews()
        {
            news.Children.Add(getLoader());
            stack_news.Children.Add(getTabloader());

            List<Life_RU> lf = new List<Life_RU>();

            await Task.Run(() =>
            {
                string url = "https://life.ru/xml/feed.xml";

                var web = new WebClient();
                var data = web.DownloadData(url);
                string result = Encoding.UTF8.GetString(data);
                var doc = new XmlDocument();

                int index1 = 0;
                int index2 = 0;
                doc.LoadXml(result);
                foreach (XmlNode node in doc.SelectNodes("rss/channel"))
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (NewsLoader > index2)
                        {
                            index2++;
                            continue;
                        }
                        if (index1 >= 50)
                        {
                            break;
                        }

                        if (child.LocalName == "item")
                        {
                            Life_RU _lf = new Life_RU(); // создаю новый экземпляр класса Life_RU
                            foreach (XmlNode child2 in child.ChildNodes)
                            {
                                if (child2.LocalName == "title")
                                {
                                    _lf.header = child2.InnerText;
                                    _lf.hash_id = child2.InnerText.GetHashCode(); // хеш заголовка - это ID каждой новости 
                                }
                                else if (child2.LocalName == "description")
                                {
                                    _lf.text = child2.InnerText;
                                }
                                else if (child2.LocalName == "pubDate")
                                {
                                    _lf.date_time = child2.InnerText;
                                }
                                else if (child2.LocalName == "link")
                                {
                                    _lf.url = child2.InnerText;
                                }
                                else if (child2.LocalName == "enclosure")
                                {
                                    _lf.buffer = new WebClient().DownloadData(child2.Attributes["url"]?.InnerText);
                                }
                            }
                            bool exist = false;
                            foreach (var life in life_all)
                            {
                                if (life.hash_id == _lf.hash_id)
                                {
                                    exist = true;
                                }
                            }
                            if (!exist)
                            {
                                lf.Add(_lf); // заполненный экземпляр _lf класса Life_RU  по одной новости добовляю в List lf
                                life_all.Add(_lf);
                                index1++;
                            }
                        }
                        if (index2 >= NewsLoader)
                        {
                            NewsLoader++;
                        }
                        index2++;
                    }
                }
            });
            NewsLoader++;
            news.Children.RemoveAt(news.Children.Count - 1);

            foreach (var l in lf)
            {
                StackPanel fone = new StackPanel();
                fone.Width = PostWidth;
                fone.Background = Brushes.Gray;

                var header = new TextBlock();
                header.Text = l.header;
                header.Width = PostWidth;
                header.TextWrapping = TextWrapping.WrapWithOverflow;
                fone.Children.Add(header);

                Image myImage = new Image();
                if (l.buffer != null)
                {
                    var stream = new MemoryStream(l.buffer);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    myImage.Width = PostWidth;
                    myImage.Height = 400;
                    myImage.Source = bitmap;

                    fone.Children.Add(myImage);
                }

                var text = new TextBlock();
                text.Text = l.text;
                text.Width = PostWidth;
                text.TextWrapping = TextWrapping.WrapWithOverflow;
                fone.Children.Add(text);

                Label linkLabel = new Label();
                Run linkText = new Run("Источник: " + l.url);
                Hyperlink link = new Hyperlink(linkText);

                link.NavigateUri = new Uri(l.url);

                link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender1, RequestNavigateEventArgs e1)
                {
                    Process.Start(new ProcessStartInfo(e1.Uri.AbsoluteUri));
                    e1.Handled = true;
                });

                linkLabel.Content = link;
                fone.Children.Add(linkLabel);

                var button = new Button();
                button.Content = "Запостить";
                button.Width = PostWidth;
                button.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                {
                    window_date.datetime.Value = DateTime.Now;
                    post_add = false;
                    window_date.ShowDialog();
                    if (post_add)
                    {
                        if (!Directory.Exists("Files/"))
                        {
                            Directory.CreateDirectory("Files");
                        }
                        if (l.buffer != null)
                        {
                            FileStream fileStream;
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            fileStream = new FileStream("Files/image_news_" + l.hash_id.ToString() + ".png", FileMode.Create);
                            encoder.Frames.Add(BitmapFrame.Create(myImage.Source as BitmapSource));
                            encoder.Save(fileStream);
                            fileStream.Close();
                            JObject global = new JObject(new JProperty("text", l.text), new JProperty("date", date), new JProperty("link", l.url), new JProperty("image", "image_news_" + l.hash_id.ToString().ToString() + ".png"));
                            File.WriteAllText(@"Files/post_news_" + l.hash_id.ToString() + ".json", global.ToString());
                        }
                        else
                        {
                            JObject global = new JObject(new JProperty("text", l.text), new JProperty("date", date), new JProperty("link", l.url));
                            File.WriteAllText(@"Files/post_news_" + l.hash_id.ToString() + ".json", global.ToString());
                        }
                    }
                });
                fone.Children.Add(button);
                news.Children.Add(fone);

                Separator separator = new Separator();
                separator.Height = 50;
                news.Children.Add(separator);
            }

            var button1 = new Button();
            button1.Content = "Загрузить еще";
            button1.Width = PostWidth;
            button1.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
            {
                news.Children.RemoveAt(news.Children.Count - 1);

                LoadNews();
            });
            news.Children.Add(button1);
            stack_news.Children.Clear();
        }

        public async void VokrugLoad()
        {
            vokrug_sveta.Children.Add(getLoader());
            stack_vokrug.Children.Add(getTabloader());
            list_vokrug.Clear();

            if (VokrugLoader <= 0)
            {
                vokrug_sveta.Children.RemoveAt(vokrug_sveta.Children.Count - 1);
                return;
            }

            int toProcess = 10;
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                int index1 = VokrugLoader;
                var list = new List<int>();
                for (int i = 0; i < 10; i++) list.Add(i);
                for (int i = 0; i < 10; i++)
                {
                    ThreadPool.QueueUserWorkItem(
                       new WaitCallback(x =>
                       {
                           index1 -= 50;
                           START(index1);
                                   // Safely decrement the counter
                                   if (Interlocked.Decrement(ref toProcess) == 0)
                           {
                               resetEvent.Set();
                           }
                       }), list[i]);
                }

                await Task.Run(() => {
                    resetEvent.WaitOne();
                    foreach (var vokrug in list_vokrug)
                    {
                        var _buffer = new WebClient().DownloadData(vokrug.img);
                        vokrug.buffer = _buffer;
                    }
                });
            }
            VokrugLoader -= 500;
            vokrug_sveta.Children.RemoveAt(vokrug_sveta.Children.Count - 1);

            foreach (var vokrug in list_vokrug)
            {
                StackPanel fone = new StackPanel();
                fone.Width = PostWidth;
                fone.Background = Brushes.Gray;

                var header = new TextBlock();
                header.Text = vokrug.title;
                header.Width = PostWidth;
                header.TextWrapping = TextWrapping.WrapWithOverflow;
                fone.Children.Add(header);

                Image myImage = new Image();
                if (vokrug.buffer != null)
                {
                    var stream = new MemoryStream(vokrug.buffer);
                    var bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnDemand;
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    myImage.Width = PostWidth;
                    myImage.Height = 400;
                    myImage.Source = bitmap;

                    fone.Children.Add(myImage);
                }

                var text = new TextBlock();
                text.Text = vokrug.text;
                text.Width = PostWidth;
                text.TextWrapping = TextWrapping.WrapWithOverflow;
                fone.Children.Add(text);

                Label linkLabel = new Label();
                Run linkText = new Run("Источник: " + vokrug.url);
                Hyperlink link = new Hyperlink(linkText);

                link.NavigateUri = new Uri(vokrug.url);

                link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender1, RequestNavigateEventArgs e1)
                {
                    Process.Start(new ProcessStartInfo(e1.Uri.AbsoluteUri));
                    e1.Handled = true;
                });

                linkLabel.Content = link;
                fone.Children.Add(linkLabel);

                var button = new Button();
                button.Content = "Запостить";
                button.Width = PostWidth;
                button.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                {
                    window_date.datetime.Value = DateTime.Now;
                    post_add = false;
                    window_date.ShowDialog();
                    if (post_add)
                    {
                        if (!Directory.Exists("Files/"))
                        {
                            Directory.CreateDirectory("Files");
                        }
                        if (vokrug.buffer != null)
                        {
                            FileStream fileStream;
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            fileStream = new FileStream("Files/image_vokrug_" + vokrug.hash.ToString() + ".png", FileMode.Create);
                            encoder.Frames.Add(BitmapFrame.Create(myImage.Source as BitmapSource));
                            encoder.Save(fileStream);
                            fileStream.Close();
                            JObject global = new JObject(new JProperty("text", vokrug.text), new JProperty("date", date), new JProperty("link", vokrug.url), new JProperty("image", "image_vokrug_" + vokrug.hash.ToString() + ".png"));
                            File.WriteAllText(@"Files/post_vokrug_" + vokrug.hash.ToString() + ".json", global.ToString());
                        }
                        else
                        {
                            JObject global = new JObject(new JProperty("text", vokrug.text), new JProperty("date", date), new JProperty("link", vokrug.url));
                            File.WriteAllText(@"Files/post_vokrug_" + vokrug.hash.ToString() + ".json", global.ToString());
                        }
                    }
                });
                fone.Children.Add(button);
                vokrug_sveta.Children.Add(fone);

                Separator separator = new Separator();
                separator.Height = 50;
                vokrug_sveta.Children.Add(separator);
            }
            stack_vokrug.Children.Clear();

            var button1 = new Button();
            button1.Content = "Загрузить еще";
            button1.Width = PostWidth;
            button1.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
            {
                vokrug_sveta.Children.RemoveAt(vokrug_sveta.Children.Count - 1);

                VokrugLoad();
            });
            vokrug_sveta.Children.Add(button1);
        }

        private async void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (tab.SelectedIndex == 2)
            {
                try
                {
                    if (goroskopes_loaded)
                    {
                        return;
                    }
                    goroskopes_loaded = true;
                    goroskope.Children.Clear();

                    goroskope.Children.Add(getLoader());
                    stack_goroskope.Children.Add(getTabloader());

                    string oven = "Гороскоп Овен на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string telec = "Гороскоп Телец на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string blizneci = "Гороскоп Близнецы на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string rak = "Гороскоп Рак на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string lev = "Гороскоп Лев на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string deva = "Гороскоп Дева на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string vesi = "Гороскоп Весы на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string scorpion = "Гороскоп Скорпион на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string strelec = "Гороскоп Стрелец на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string kozerog = "Гороскоп Козерог на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string vodolei = "Гороскоп Водолей на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;
                    string ribi = "Гороскоп Рыбы на сегодня " + DateTime.Now.ToShortDateString() + Environment.NewLine;

                    string oven_tomorrow = "Гороскоп Овен на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string telec_tomorrow = "Гороскоп Телец на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string blizneci_tomorrow = "Гороскоп Близнецы на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string rak_tomorrow = "Гороскоп Рак на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string lev_tomorrow = "Гороскоп Лев на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string deva_tomorrow = "Гороскоп Дева на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string vesi_tomorrow = "Гороскоп Весы на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string scorpion_tomorrow = "Гороскоп Скорпион на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string strelec_tomorrow = "Гороскоп Стрелец на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string kozerog_tomorrow = "Гороскоп Козерог на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string vodolei_tomorrow = "Гороскоп Водолей на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;
                    string ribi_tomorrow = "Гороскоп Рыбы на завтра " + DateTime.Now.AddDays(1).ToShortDateString() + Environment.NewLine;


                    await Task.Run(() =>
                    {
                        oven += getgoroskope("http://orakul.com/horoscope/astrologic/more/aries/today.html");
                        telec += getgoroskope("http://orakul.com/horoscope/astrologic/more/taurus/today.html");
                        blizneci += getgoroskope("http://orakul.com/horoscope/astrologic/more/gemini/today.html");
                        rak += getgoroskope("http://orakul.com/horoscope/astrologic/more/cancer/today.html");
                        lev += getgoroskope("http://orakul.com/horoscope/astrologic/more/lion/today.html");
                        deva += getgoroskope("http://orakul.com/horoscope/astrologic/more/virgo/today.html");
                        vesi += getgoroskope("http://orakul.com/horoscope/astrologic/more/libra/today.html");
                        scorpion += getgoroskope("http://orakul.com/horoscope/astrologic/more/scorpio/today.html");
                        strelec += getgoroskope("http://orakul.com/horoscope/astrologic/more/sagittarius/today.html");
                        kozerog += getgoroskope("http://orakul.com/horoscope/astrologic/more/capricorn/today.html");
                        vodolei += getgoroskope("http://orakul.com/horoscope/astrologic/more/aquarius/today.html");
                        ribi += getgoroskope("http://orakul.com/horoscope/astrologic/more/pisces/today.html");

                        oven_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/aries/tomorrow.html");
                        telec_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/taurus/tomorrow.html");
                        blizneci_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/gemini/tomorrow.html");
                        rak_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/cancer/tomorrow.html");
                        lev_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/lion/tomorrow.html");
                        deva_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/virgo/tomorrow.html");
                        vesi_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/libra/tomorrow.html");
                        scorpion_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/scorpio/tomorrow.html");
                        strelec_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/sagittarius/tomorrow.html");
                        kozerog_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/capricorn/tomorrow.html");
                        vodolei_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/aquarius/tomorrow.html");
                        ribi_tomorrow += getgoroskope("http://orakul.com/horoscope/astrologic/more/pisces/tomorrow.html");

                    });

                    goroskope.Children.Clear();
                    goroskope.Children.Add(getGoroTextBlock(oven));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/aries/today.html"));
                    goroskope.Children.Add(getGoroButton(oven, "oven", "http://orakul.com/horoscope/astrologic/more/aries/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(telec));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/taurus/today.html"));
                    goroskope.Children.Add(getGoroButton(telec, "telec", "http://orakul.com/horoscope/astrologic/more/taurus/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(blizneci));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/gemini/today.html"));
                    goroskope.Children.Add(getGoroButton(blizneci, "blizneci", "http://orakul.com/horoscope/astrologic/more/gemini/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(rak));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/cancer/today.html"));
                    goroskope.Children.Add(getGoroButton(rak, "rak", "http://orakul.com/horoscope/astrologic/more/cancer/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(lev));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/lion/today.html"));
                    goroskope.Children.Add(getGoroButton(lev, "lev", "http://orakul.com/horoscope/astrologic/more/lion/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(deva));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/virgo/today.html"));
                    goroskope.Children.Add(getGoroButton(deva, "deva", "http://orakul.com/horoscope/astrologic/more/virgo/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(vesi));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/libra/today.html"));
                    goroskope.Children.Add(getGoroButton(vesi, "vesi", "http://orakul.com/horoscope/astrologic/more/libra/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(scorpion));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/scorpio/today.html"));
                    goroskope.Children.Add(getGoroButton(scorpion, "scorpion", "http://orakul.com/horoscope/astrologic/more/scorpio/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(strelec));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/sagittarius/today.html"));
                    goroskope.Children.Add(getGoroButton(strelec, "strelec", "http://orakul.com/horoscope/astrologic/more/sagittarius/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(kozerog));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/capricorn/today.html"));
                    goroskope.Children.Add(getGoroButton(kozerog, "kozerog", "http://orakul.com/horoscope/astrologic/more/capricorn/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(vodolei));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/aquarius/today.html"));
                    goroskope.Children.Add(getGoroButton(vodolei, "vodolei", "http://orakul.com/horoscope/astrologic/more/aquarius/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(ribi));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/pisces/today.html"));
                    goroskope.Children.Add(getGoroButton(ribi, "ribi", "http://orakul.com/horoscope/astrologic/more/pisces/today.html"));

                    goroskope.Children.Add(getGoroTextBlock(oven_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/aries/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(oven_tomorrow, "oven_tomorrow", "http://orakul.com/horoscope/astrologic/more/aries/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(telec_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/taurus/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(telec_tomorrow, "telec_tomorrow", "http://orakul.com/horoscope/astrologic/more/taurus/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(blizneci_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/gemini/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(blizneci_tomorrow, "blizneci_tomorrow", "http://orakul.com/horoscope/astrologic/more/gemini/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(rak_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/cancer/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(rak_tomorrow, "rak_tomorrow", "http://orakul.com/horoscope/astrologic/more/cancer/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(lev_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/lion/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(lev_tomorrow, "lev_tomorrow", "http://orakul.com/horoscope/astrologic/more/lion/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(deva_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/virgo/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(deva_tomorrow, "deva_tomorrow", "http://orakul.com/horoscope/astrologic/more/virgo/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(vesi_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/libra/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(vesi_tomorrow, "vesi_tomorrow", "http://orakul.com/horoscope/astrologic/more/libra/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(scorpion_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/scorpio/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(scorpion_tomorrow, "scorpion_tomorrow", "http://orakul.com/horoscope/astrologic/more/scorpio/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(strelec_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/sagittarius/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(strelec_tomorrow, "strelec_tomorrow", "http://orakul.com/horoscope/astrologic/more/sagittarius/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(kozerog_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/capricorn/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(kozerog_tomorrow, "kozerog_tomorrow", "http://orakul.com/horoscope/astrologic/more/capricorn/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(vodolei_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/aquarius/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(vodolei_tomorrow, "vodolei_tomorrow", "http://orakul.com/horoscope/astrologic/more/aquarius/tomorrow.html"));

                    goroskope.Children.Add(getGoroTextBlock(ribi_tomorrow));
                    goroskope.Children.Add(getLink("http://orakul.com/horoscope/astrologic/more/pisces/tomorrow.html"));
                    goroskope.Children.Add(getGoroButton(ribi_tomorrow, "ribi_tomorrow", "http://orakul.com/horoscope/astrologic/more/pisces/tomorrow.html"));

                    stack_goroskope.Children.Clear();
                }
                catch (Exception ex)
                {
                    stack_goroskope.Children.Clear();
                    LogError(ex.Message, 2);
                }
            }
            else if (tab.SelectedIndex == 1)
            {
                try
                {
                    if (news_loaded)
                    {
                        return;
                    }
                    news_loaded = true;
                    news.Children.Clear();

                    LoadNews();
                }
                catch (Exception ex)
                {
                    stack_news.Children.Clear();
                    LogError(ex.Message, 1);
                }
            }
            else if (tab.SelectedIndex == 4)
            {
                try
                {
                    if (vokrug_loaded)
                    {
                        return;
                    }
                    vokrug_loaded = true;
                    vokrug_sveta.Children.Clear();

                    VokrugLoad();
                }
                catch (Exception ex)
                {
                    stack_vokrug.Children.Clear();
                    LogError(ex.Message, 4);
                }
            }
        }

        private string GetPogoda(string gorod, bool oneday = true)
        {
            try
            {
                var document = new WebClient().DownloadData("https://yandex.ru/pogoda/" + gorod);
                HtmlDocument html = new HtmlDocument();
                string doc = Encoding.UTF8.GetString(document);
                html.LoadHtml(doc);
                HtmlNodeCollection today_date_html = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[1]/span[2]");
                if (today_date_html != null)
                {
                    if (oneday)
                    {
                        HtmlNodeCollection today_date = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[1]/span[2]");
                        HtmlNodeCollection today_pogoda = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[2]/div[1]");
                        HtmlNodeCollection today_gradus_dnem = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[2]/div[2]");
                        HtmlNodeCollection today_gradus_nochiy = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[3]");
                        string today = "";
                        string mesyac = "";
                        if (today_date != null && today_pogoda != null && today_gradus_dnem != null && today_gradus_nochiy != null)
                        {
                            mesyac = today_date[0].InnerHtml.Split(' ')[1];
                            today = today_date[0].InnerHtml + " " + today_pogoda[0].InnerHtml + " " +
                                today_gradus_dnem[0].InnerHtml + " " + today_gradus_nochiy[0].InnerHtml + ";";
                        }
                        return today;
                    }
                    else
                    {
                        HtmlNodeCollection today_date = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[1]/span[2]");
                        HtmlNodeCollection today_pogoda = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[2]/div[1]");
                        HtmlNodeCollection today_gradus_dnem = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[2]/div[2]");
                        HtmlNodeCollection today_gradus_nochiy = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item forecast-brief__item_today day-anchor i-bem']/div[3]");
                        string today = "";
                        string mesyac = "";
                        if (today_date != null && today_pogoda != null && today_gradus_dnem != null && today_gradus_nochiy != null)
                        {
                            mesyac = today_date[0].InnerHtml.Split(' ')[1];
                            today = today_date[0].InnerHtml + " " + today_pogoda[0].InnerHtml + " " +
                                today_gradus_dnem[0].InnerHtml + " " + today_gradus_nochiy[0].InnerHtml + ";";
                        }
                        HtmlNodeCollection dates_day_name = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item day-anchor i-bem']/div[1]/span[1]");
                        HtmlNodeCollection dates_day = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item day-anchor i-bem']/div[1]/span[2]");
                        HtmlNodeCollection dates_pogoda = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item day-anchor i-bem']/div[2]/div[1]");
                        HtmlNodeCollection dates_gradys_dnem = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item day-anchor i-bem']/div[2]/div[2]");
                        HtmlNodeCollection dates_gradys_nochiy = html.DocumentNode.SelectNodes("//li[@class='forecast-brief__item day-anchor i-bem']/div[3]");
                        if (dates_day_name != null && dates_day != null && dates_pogoda != null &&
                            dates_gradys_dnem != null && dates_gradys_nochiy != null)
                        {
                            if (dates_day_name.Count == dates_day.Count &&
                                dates_day_name.Count == dates_pogoda.Count &&
                                dates_day_name.Count == dates_gradys_dnem.Count &&
                                dates_day_name.Count == dates_gradys_nochiy.Count)
                            {
                                List<string> dates_pogodes = new List<string>();
                                for (int i = 0; i < dates_day_name.Count; i++)
                                {
                                    if (dates_day[i].InnerHtml.Contains(mesyac))
                                    {
                                        dates_pogodes.Add(dates_day_name[i].InnerHtml + " " + dates_day[i].InnerHtml + " " +
                                        " " + dates_pogoda[i].InnerHtml + " " +
                                        dates_gradys_dnem[i].InnerHtml + " днем " + dates_gradys_nochiy[i].InnerHtml +
                                        " ночью; ");
                                    }
                                    else
                                    {
                                        dates_pogodes.Add(dates_day_name[i].InnerHtml + " " + dates_day[i].InnerHtml + " " +
                                        mesyac + " " + dates_pogoda[i].InnerHtml + " " +
                                        dates_gradys_dnem[i].InnerHtml + " днем " + dates_gradys_nochiy[i].InnerHtml +
                                        " ночью; ");
                                    }
                                }
                                string result = today + " " + String.Join(String.Empty, dates_pogodes.ToArray());
                                return result;
                            }
                        }
                    }
                }
                return "Нет данных. Проверьте правильно ли вы ввели город!";
            }
            catch
            {
                return "Нет данных. Проверьте правильно ли вы ввели город!";
            }
        }

        private async void pogoda_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pogoda1 = "";
                string pogoda2 = "";
                string gorod_translate = "";
                string gorod1 = pogoda_text.Text;

                if (gorod1 == "")
                {
                    MessageBox.Show("Введите название города!");
                    return;
                }

                pogoda.Children.Clear();

                pogoda.Children.Add(getLoader());
                stack_pogoda.Children.Add(getTabloader());

                await Task.Run(() =>
                {

                    WebClient client = new WebClient();
                    Stream data = client.OpenRead("https://translate.yandex.net/api/v1.5/tr.json/translate?key=trnsl.1.1.20170703T111416Z.630db1772cafdea0.f3ad020382a5312c158cbbbf8fe3e92f3ab5015b&text=" +
                        gorod1 + "&lang=ru-en");
                    StreamReader reader = new StreamReader(data);
                    JObject gorod = JObject.Parse(reader.ReadToEnd());
                    reader.Close();
                    data.Close();

                    gorod_translate = gorod["text"][0].ToString();

                    pogoda1 = GetPogoda(gorod_translate);
                    pogoda2 = GetPogoda(gorod_translate, false);
                });

                int number;
                bool result = Int32.TryParse(gorod1, out number);
                if (result)
                {
                    pogoda1 = "Нет данных. Проверьте правильно ли вы ввели город!";
                    pogoda2 = "Нет данных. Проверьте правильно ли вы ввели город!";
                }

                pogoda.Children.Clear();

                var text = new TextBlock();
                text.Text = gorod1 + ". Погода на сегодня:" + Environment.NewLine + pogoda1;
                text.Width = PostWidth;
                text.TextWrapping = TextWrapping.WrapWithOverflow;
                pogoda.Children.Add(text);

                Label linkLabel = new Label();
                linkLabel.Width = PostWidth;
                Run linkText = new Run("Источник: " + "https://yandex.ru/pogoda/" + gorod_translate);
                Hyperlink link = new Hyperlink(linkText);

                link.NavigateUri = new Uri("https://yandex.ru/pogoda/" + gorod_translate);

                link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender1, RequestNavigateEventArgs e1)
                {
                    Process.Start(new ProcessStartInfo(e1.Uri.AbsoluteUri));
                    e1.Handled = true;
                });

                linkLabel.Content = link;

                var button = new Button();
                button.Content = "Запостить";
                button.Width = PostWidth;
                button.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                {
                    window_date.datetime.Value = DateTime.Now;
                    post_add = false;
                    window_date.ShowDialog();
                    if (post_add)
                    {
                        if (!Directory.Exists("Files/"))
                        {
                            Directory.CreateDirectory("Files");
                        }
                        JObject global = new JObject(new JProperty("text", text.Text), new JProperty("date", date), new JProperty("link", "https://yandex.ru/pogoda/" + gorod_translate));
                        File.WriteAllText(@"Files/post_pogoda_" + gorod_translate + "_today.json", global.ToString());
                    }
                });

                if (pogoda1 != "Нет данных. Проверьте правильно ли вы ввели город!")
                {
                    pogoda.Children.Add(linkLabel);
                    pogoda.Children.Add(button);
                }

                var text1 = new TextBlock();
                text1.Text = gorod1 + ". Погода на 10 дней:" + Environment.NewLine + pogoda2;
                text1.Width = PostWidth;
                text1.TextWrapping = TextWrapping.WrapWithOverflow;
                pogoda.Children.Add(text1);

                Label linkLabel1 = new Label();
                linkLabel1.Width = PostWidth;
                Run linkText1 = new Run("Источник: " + "https://yandex.ru/pogoda/" + gorod_translate);
                Hyperlink link1 = new Hyperlink(linkText1);

                link1.NavigateUri = new Uri("https://yandex.ru/pogoda/" + gorod_translate);

                link1.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender1, RequestNavigateEventArgs e1)
                {
                    Process.Start(new ProcessStartInfo(e1.Uri.AbsoluteUri));
                    e1.Handled = true;
                });

                linkLabel1.Content = link1;

                var button1 = new Button();
                button1.Content = "Запостить";
                button1.Width = PostWidth;
                button1.Click += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
                {
                    window_date.datetime.Value = DateTime.Now;
                    post_add = false;
                    window_date.ShowDialog();
                    if (post_add)
                    {
                        if (!Directory.Exists("Files/"))
                        {
                            Directory.CreateDirectory("Files");
                        }
                        JObject global = new JObject(new JProperty("text", text1.Text), new JProperty("date", date), new JProperty("link", "https://yandex.ru/pogoda/" + gorod_translate));
                        File.WriteAllText(@"Files/post_pogoda_" + gorod_translate + "_10.json", global.ToString());
                    }
                });

                if (pogoda2 != "Нет данных. Проверьте правильно ли вы ввели город!")
                {
                    pogoda.Children.Add(linkLabel1);
                    pogoda.Children.Add(button1);
                }
                stack_pogoda.Children.Clear();
            }
            catch (Exception ex)
            {
                stack_pogoda.Children.Clear();
                LogError(ex.Message, 3);
            }
        }

        private void START(int start_index)
        {
            string url = "http://www.vokrugsveta.ru/article/";
            for (int i = start_index; i > start_index-50; i--)
            {
                var t = GET(url + i + "/");
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(t);
                HtmlNodeCollection h1_html = html.DocumentNode.SelectNodes("//div[@class='article-info']/../h1");

                if (h1_html != null)
                {
                    string _title = "";
                    string _text = "";
                    string _url = url + i + "/";
                    string _img = Parsing(t).Replace("\" /", "");
                    string _hash = "";

                    _title = h1_html[0].InnerText.ToString();
                    _hash = _title.ToLower().Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").GetHashCode().ToString();

                    HtmlNodeCollection text_html = html.DocumentNode.SelectNodes("//div[@class='detail-text']/p");
                    if (text_html != null)
                    {
                        foreach (var text in text_html)
                        {
                            _text += text.InnerText;
                        }
                    }
                    _text = WebUtility.HtmlDecode(_text);

                    lock (thisLock)
                    {
                        bool add = true;
                        foreach (var vokrug in all_list_vokrug)
                        {
                            if (vokrug.hash == _hash)
                            {
                                add = false;
                            }
                        }

                        foreach (var vokrug in list_vokrug)
                        {
                            if (vokrug.hash == _hash)
                            {
                                add = false;
                            }
                        }

                        if (add)
                        {
                            var vokrug = new VOKRUG { img = _img, text = _text, title = _title, url = _url, hash = _hash };
                            list_vokrug.Add(vokrug);
                            all_list_vokrug.Add(vokrug);
                        }
                    }
                }
                //Thread.Sleep(300);
            }
        }

        private static string Parsing(string inputString)
        {
            string pattern = @"(?<=<meta).*?(?=>)";
            MatchCollection matches = Regex.Matches(inputString, pattern);
            string value = "";
            foreach (Match match in matches)
            {
                if (match.Value.Contains("og:image"))
                {
                    value = match.Value.Replace(" name=\"og:image\" content=\"", "");
                    break;
                }
            }
            return value;
        }

        public static string GET(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Get;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:50.0) Gecko/20100101 Firefox/50.0";
                request.AllowAutoRedirect = false;
                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Accept = "text/html,application/json,application/xml;q=0.9,*/*;q=0.8";
                string resp = RESPONSE(request);
                return resp;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string RESPONSE(HttpWebRequest request)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string answer = "";
                var headers = response.Headers.ToString();

                if (Convert.ToInt32(response.StatusCode) == 302 || Convert.ToInt32(response.StatusCode) == 200)
                {
                    using (Stream rspStm = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(rspStm, Encoding.GetEncoding(1251), true))
                        {
                            answer = string.Empty; answer = reader.ReadToEnd();
                        }
                    }
                    return answer;
                }
                else
                {
                    response.Close(); return WebUtility.HtmlDecode(response.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                return WebUtility.HtmlDecode(ex.Message);
            }
        }

        private void scroll_Unloaded(object sender, RoutedEventArgs e)
        {
            scroll.Tag = scroll.VerticalOffset;
        }

        private void scroll_Loaded(object sender, RoutedEventArgs e)
        {
            double offset;
            if (scroll.Tag != null
                 && double.TryParse(scroll.Tag.ToString(), out offset))
            {
                scroll.ScrollToVerticalOffset(offset);
            }
        }

        private void scroll_news_Loaded(object sender, RoutedEventArgs e)
        {
            double offset;
            if (scroll_news.Tag != null
                 && double.TryParse(scroll_news.Tag.ToString(), out offset))
            {
                scroll_news.ScrollToVerticalOffset(offset);
            }
        }

        private void scroll_news_Unloaded(object sender, RoutedEventArgs e)
        {
            scroll_news.Tag = scroll_news.VerticalOffset;
        }

        private void scroll_goroskope_Unloaded(object sender, RoutedEventArgs e)
        {
            scroll_goroskope.Tag = scroll_goroskope.VerticalOffset;
        }

        private void scroll_goroskope_Loaded(object sender, RoutedEventArgs e)
        {
            double offset;
            if (scroll_goroskope.Tag != null
                 && double.TryParse(scroll_goroskope.Tag.ToString(), out offset))
            {
                scroll_goroskope.ScrollToVerticalOffset(offset);
            }
        }

        private void vokrug_sveta_Loaded(object sender, RoutedEventArgs e)
        {
            double offset;
            if (scroll_vokrug.Tag != null
                 && double.TryParse(scroll_vokrug.Tag.ToString(), out offset))
            {
                scroll_vokrug.ScrollToVerticalOffset(offset);
            }
        }

        private void vokrug_sveta_Unloaded(object sender, RoutedEventArgs e)
        {
            scroll_vokrug.Tag = scroll_vokrug.VerticalOffset;
        }
    }
}
