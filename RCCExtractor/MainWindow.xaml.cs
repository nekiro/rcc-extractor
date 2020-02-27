using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RCCExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static char[] header = { '\x89', 'P', 'N', 'G' };
        static char[] footer = { 'I', 'E', 'N', 'D' };

        public MainWindow()
        {
            InitializeComponent();
            ConsoleLog.Clear();

            AddNewLog("\nWelcome, select your file and click extract...\nWaiting for action...");
        }

        int Search(byte[] src, byte[] pattern, int startPosition)
        {
            int c = src.Length - pattern.Length + 1;
            int j;
            for (int i = startPosition; i < c; i++)
            {
                if (src[i] != pattern[0]) continue;
                for (j = pattern.Length - 1; j >= 1 && src[i + j] == pattern[j]; j--);
                if (j == 0) return i;
            }
            return -1;
        }

        private void AddNewLog(string text)
        {
            Application.Current.Dispatcher.Invoke(() => { ConsoleLog.Text += text; ConsoleLog.ScrollToEnd(); });
        }

        private void Extract(string filePath, string path)
        {
            AddNewLog("\nExtracting " + filePath);
            int position = 0;
            try
            {
                byte[] allBytes = File.ReadAllBytes(filePath);
                while (position < allBytes.Length - 1)
                {
                    int start = Search(allBytes, header.Select(c => (byte)c).ToArray(), position);
                    int end = Search(allBytes, footer.Select(c => (byte)c).ToArray(), start);
                    position = end;

                    List<byte> arr = new List<byte>();
                    for (int i = start; i < end; i++)
                    {
                        arr.Add(allBytes[i]);
                    }

                    string name = "image" + start + ".png";
                    File.WriteAllBytes(Path.Combine(path, name), arr.ToArray<byte>());
                    AddNewLog("\nExtracted " + name);
                }
            }
            catch (Exception e)
            {
                //AddNewLog(e.ToString());
            }
        }

        private async void HandleClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            if (btn.Content.ToString() == "Extract")
            {
                if (String.IsNullOrEmpty(PathBox.Text))
                {
                    AddNewLog("\n[ERROR] Missing path");
                    return;
                }

                SaveFileDialog saveFile = new SaveFileDialog() { Filter = "Directory | directory", FileName = "Any Directory" };
                if (saveFile.ShowDialog() == true)
                {
                    string path = PathBox.Text;
                    ExtractButton.IsEnabled = false;
                    BrowseButton.IsEnabled = false;
                    await Task.Run(() => Extract(path, Path.GetDirectoryName(saveFile.FileName)));
                    ExtractButton.IsEnabled = true;
                    BrowseButton.IsEnabled = true;
                }
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Rcc Files (*.rcc) | *.rcc" };
                if (openFileDialog.ShowDialog() == true)
                    PathBox.Text = openFileDialog.FileName;
            }
        }
    }
}
