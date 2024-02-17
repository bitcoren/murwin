using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Controls;
using System.Security.Policy;

namespace Murzilla
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string programPath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = System.IO.Path.Combine(programPath, "data");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            LoadData();
            this.Loaded += MainWindow_Loaded;
            webView.NavigationCompleted += NavigationCompleted;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }
        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                if (addressBar.Text.StartsWith("https://") | addressBar.Text.StartsWith("http://"))
                {
                    webView.CoreWebView2.Navigate(addressBar.Text);
                } else { addressBar.Text = "http://" + addressBar.Text;
                    webView.CoreWebView2.Navigate(addressBar.Text);
                }
            }
        }
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.GoBack();
            }
        }
        private void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Reload();
            }
        }
        private void ButtonForward_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.GoForward();
            }
        }
        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {
            addressBar.Text = "http://127.0.0.1:8080/ipfs/QmS4ustL54uo8FzR9455qaxZwuMiUhyvMcX9Ba8nUH4uVv/readme";
            webView.CoreWebView2.Navigate(addressBar.Text);
        }
        private void ButtonSource_Click(object sender, RoutedEventArgs e)
        {
            addressBar.Text = "https://github.com/bitcoren/murzilla";
            webView.CoreWebView2.Navigate(addressBar.Text);
        }
        private void ButtonOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = addressBar.Text,
                UseShellExecute = true
            });
        }
        private void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                titleBar.Text = webView.CoreWebView2.DocumentTitle;
                addressBar.Text = webView.Source.ToString();
            }
            else
            {
                titleBar.Text = "Error";
            }
        }
        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (webView != null && webView.CoreWebView2 != null)
                {
                    if (addressBar.Text.StartsWith("https://") | addressBar.Text.StartsWith("http://"))
                    {
                        webView.CoreWebView2.Navigate(addressBar.Text);
                    }
                    else
                    {
                        addressBar.Text = "http://" + addressBar.Text;
                        webView.CoreWebView2.Navigate(addressBar.Text);
                    }
                }
            }
        }
        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.Activate();
            string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "nircmd.exe");
            string arguments = "win activate title \"Murzilla\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = true
            };
            Process process = new Process { StartInfo = startInfo };
            process.Start();
        }
        private void Hide_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            Thread.Sleep(1000);
            Application.Current.MainWindow.Visibility = Visibility.Hidden;
        }
        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            webView.CoreWebView2.Navigate(e.Uri);
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async(null);

            string html = await webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");

            html = html.Substring(1, html.Length - 2);
            html = Regex.Replace(html, @"\\u(?<Value>[a-zA-Z0-9]{4})", match => ((char)int.Parse(match.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());            

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(html);
            html = Encoding.UTF8.GetString(utf8Bytes);
            html = html.Replace("\\n", "");
            html = html.Replace("\\\"", "\"");
            string programPath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = System.IO.Path.Combine(programPath, "data");
            string fileName = System.IO.Path.Combine(folderPath, "webpage.html");
            File.WriteAllText(fileName, html);
            var user = new Product { URL = addressBar.Text, Page = Encoding.UTF8.GetBytes(html), Size = Encoding.UTF8.GetByteCount(html) };
            using (var db = new AppDbContext())
            {
                db.Products.Add(user);
                db.SaveChanges();
                webView.CoreWebView2.Navigate(fileName);
                LoadData();
            }
        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //string url = "https://yandex.ru/search/?lr=225&text=" + title;
            addressBar.Text = "https://yandex.ru/search/?lr=225&text=" + titleBar.Text;
            webView.CoreWebView2.Navigate(addressBar.Text);
            await webView.EnsureCoreWebView2Async(null);
            webView.NavigationCompleted += OnSearchNavigationCompleted;
            

        }
        private async void OnSearchNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {

            string html = await webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");


            html = html.Substring(1, html.Length - 2);
            html = Regex.Replace(html, @"\\u(?<Value>[a-zA-Z0-9]{4})", match => ((char)int.Parse(match.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(html);
            html = Encoding.UTF8.GetString(utf8Bytes);
            html = html.Replace("\\n", "");
            html = html.Replace("\\\"", "\"");
            string programPath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = System.IO.Path.Combine(programPath, "data");
            string fileName = System.IO.Path.Combine(folderPath, "webpage.html");
            File.WriteAllText(fileName, html);
            var user = new Product { URL = addressBar.Text, Page = Encoding.UTF8.GetBytes(html), Size = Encoding.UTF8.GetByteCount(html) };
            using (var db = new AppDbContext())
            {
                db.Products.Add(user);
                db.SaveChanges();
                webView.CoreWebView2.Navigate(fileName);
                LoadData();
            }

            webView.NavigationCompleted -= OnSearchNavigationCompleted;
        }
        public ObservableCollection<Product> Products { get; set; }
        private void LoadData()
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }
            using (var db = new AppDbContext())
            {
                Products = new ObservableCollection<Product>(db.Products.ToList());
            }
            this.DataContext = this;
            dbList.ItemsSource = null;
            dbList.ItemsSource = Products; dbList.SelectedIndex = 0;
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null)
            {
                var product = row.Item as Product;
                OpenProductDetails(product);
            }
        }

        void OpenProductDetails(Product product)
        {
            using (var ctx = new AppDbContext())
            {
                var savedPage = ctx.Products.FirstOrDefault(p => p.Id == product.Id);
                byte[] pageContent = savedPage.Page;
                string html = Encoding.UTF8.GetString(pageContent);
                string programPath = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = System.IO.Path.Combine(programPath, "data");
                string fileName = System.IO.Path.Combine(folderPath, "webpage.html");
                File.WriteAllText(fileName, html);
                webView.CoreWebView2.Navigate(fileName);
            }
        }
    }
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Timestamp { get; set; }
        public string URL { get; set; }
        public byte[] Page { get; set; }
        public decimal Size { get; set; }
    }
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string programPath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = System.IO.Path.Combine(programPath, "data\\murzilla.fdb");
            string fbConf = "User=SYSDBA;Password=murzilla;DataSource=localhost;Port=3050;Dialect=3;Charset=UTF8;Database=" + folderPath + ";";
            optionsBuilder.UseFirebird(fbConf);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(u => u.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Product>()
              .Property(u => u.Timestamp)
              .IsRowVersion()
              .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
