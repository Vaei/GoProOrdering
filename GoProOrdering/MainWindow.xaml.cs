using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ookii.Dialogs.Wpf;
using MessageBox = System.Windows.MessageBox;


namespace GoProOrdering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Dictionary<string, string> _resultFiles = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();

            string config = GetConfigFilePath();
            if (File.Exists(config))
            {
                string[] lines = File.ReadAllLines(config);
                if (lines.Length > 0)
                {
                    Text_Directory.Text = lines.First();
                }
            }
        }

        private string GetConfigFilePath()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string baseDir = AppDomain.CurrentDomain.FriendlyName;
            string file = dir + "config.ini";

            // Create it if it doesn't exist
            if (!File.Exists(file))
            {
                File.Create(file);
            }

            return file;
        }

        public static string GetFileName(string filePath)
        {
            string file = filePath.Split('\\').Last();
            return file.Split('.')[0];
        }

        // Does not factor the codec (replaces with '+')
        private string GetWorkingFileName(string filePath)
        {
            string file = GetFileName(filePath);
            file = file.Remove(1, 1);
            file = file.Insert(1, "+");
            return file;
        }

        private int GetChapter(string file)
        {
            string chapter = file.Substring(2, 2);
            if (int.TryParse(chapter, out int i))
            {
                return i;
            }
            return -1;
        }

        private string GetFirstChapterFileName(string file)
        {
            string key = file.Remove(2, 2);
            key = key.Insert(2, "01");
            return key;
        }

        private int GetFileOrder(string file)
        {
            string fileOrder = file.Substring(4, 4);
            if (int.TryParse(fileOrder, out int i))
            {
                return i;
            }
            return -1;
        }

        private void Browse_Directory_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Select directory folder
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            bool? result = folderBrowserDialog.ShowDialog(this);
            if (result.HasValue && result.Value == true)
            {
                Text_Directory.Text = folderBrowserDialog.SelectedPath;
                
                // Update config with last path
                string config = GetConfigFilePath();

                // GetConfigFilePath() will create it if it doesn't exist, but check in case of permission issues
                if (File.Exists(config))
                {
                    string[] lines = { Text_Directory.Text };
                    File.WriteAllLines(config, lines);
                }
            }
        }

        private void Text_FirstNumber_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_FirstNumber_Up_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (int.TryParse(Text_FirstNumber.Text, out int i))
            {
                Text_FirstNumber.Text = (i + 1).ToString();
            }
        }
        private void Button_FirstNumber_Dn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (int.TryParse(Text_FirstNumber.Text, out int i))
            {
                Text_FirstNumber.Text = (i - 1).ToString();
            }
        }
        private void Text_ExtraPadding_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_ExtraPadding_Up_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (int.TryParse(Text_ExtraPadding.Text, out int i))
            {
                Text_ExtraPadding.Text = (i + 1).ToString();
            }
        }

        private void Button_ExtraPadding_Dn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (int.TryParse(Text_ExtraPadding.Text, out int i))
            {
                Text_ExtraPadding.Text = (i - 1).ToString();
            }
        }
        private void Button_Start_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _resultFiles.Clear();

            // Is directory valid?
            string dir = Text_Directory.Text;
            if (!Directory.Exists(dir))
            {
                MessageBox.Show(this, "Invalid directory");
                return;
            }

            // Read all files
            string[] baseFiles = Directory.GetFiles(dir, "*");

            // Ensure directory not empty
            if (baseFiles.Length == 0)
            {
                MessageBox.Show(this, "Directory is empty");
                return;
            }

            // Ensure all files are GoPro files
            foreach (string filePath in baseFiles)
            {
                string file = GetFileName(filePath);

                // GoPro file names always have 8 characters
                if (file.Length != 8)
                {
                    MessageBox.Show(this, file + " is not a valid GoPro file (invalid file length)");
                    return;
                }

                // 1st character is G
                if (file[0] != 'G')
                {
                    MessageBox.Show(this, file + " is not a valid GoPro file (1st character not 'G')");
                    return;
                }

                // 2nd character is encoding
                if (int.TryParse(file[1].ToString(), out _))
                {
                    MessageBox.Show(this, file + " is not a valid GoPro file (2nd character not encoding)");
                    return;
                }

                // 3rd-4th characters are chapter (01 for single files, looped not supported)
                int chapter = GetChapter(file);
                if (chapter <= 0)
                {
                    MessageBox.Show(this, file + " is not a valid GoPro file (3rd-4th characters not valid chapters)");
                    return;
                }

                // 5th-8th are file
                int fileOrder = GetFileOrder(file);
                if (fileOrder < 0)
                {
                    MessageBox.Show(this, file + " is not a valid GoPro file (last 4 characters are not a valid number)");
                    return;
                }
            }

            int extraPadding;
            if (!int.TryParse(Text_ExtraPadding.Text, out extraPadding))
            {
                extraPadding = 0;
            }

            int padding = baseFiles.Length.ToString().Length + extraPadding;

            // Get all files without extensions (and without duplicates)
            List<string> files = new List<string>();
            foreach (string file in baseFiles)
            {
                // Remove extension and path
                string s = GetWorkingFileName(file);

                // Don't add duplicates
                if (!files.Contains(s))
                {
                    files.Add(s);
                }
            }

            // Take all files from only chapter one
            List<string> core = new List<string>();
            foreach (string file in files)
            {
                // Chapter one only
                int chapter = GetChapter(file);
                if (chapter == 1)
                {
                    core.Add(file);
                }
            }

            // Group all other chapters under first chapter
            Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();
            foreach (string file in core)
            {
                // Fill keys
                mapping.Add(file, new List<string>());
            }

            foreach (string file in files)
            {
                // All additional chapters
                int chapter = GetChapter(file);
                if (chapter == 10)
                {
                    Debug.WriteLine("10");
                }
                if (chapter > 1)
                {
                    // Get matching chapter and add to mapping
                    string key = GetFirstChapterFileName(file);
                    mapping[key].Add(file);
                }
            }

            // Sort each file by ordered file number
            List<string> sorted = new List<string>();
            foreach (KeyValuePair<string, List<string>> map in mapping)
            {
                // do something with entry.Value or entry.Key
                // Add the key (first chapter)
                sorted.Add(map.Key);

                // Add the values (subsequent chapters)
                foreach (string file in map.Value)
                {
                    sorted.Add(file);
                }
            }

            // Iterate and prefix all files based on their sorted order
            // This is a dirty N^2 loop, maybe there is a more efficient way
            foreach (string file in baseFiles)
            {
                // This is the file we want to lookup in the sorted List
                string lookup = GetWorkingFileName(file);

                // Prefix is based on lookup's index in sorted List
                // (i.e where this file is in the sorted List)
                int prefix = -1;
                for (int i = 0; i < sorted.Count; i++)
                {
                    if (sorted[i] == lookup)
                    {
                        prefix = i;
                        break;
                    }
                }

                if (prefix == -1)
                {
                    // We have an error that should never happen
                    MessageBox.Show(this, "Incalculable error #8961");
                    return;
                }

                // Add first number to padding
                if (int.TryParse(Text_FirstNumber.Text, out int p))
                {
                    prefix += p;
                }

                // The prefix can then be acknowledged
                string prefixString = prefix.ToString();

                // Add any required padding
                int prefixLength = prefixString.Length;
                if (prefixLength != padding)
                {
                    for (int i = prefixLength; i < padding; i++)
                    {
                        prefixString = prefixString.Insert(0, "0");
                    }
                }

                string ext = "." + file.Split('.').Last();

                string fileName = GetFileName(file) + ext;
                string newName = prefixString + "_" + fileName;

                string newFile = file.Replace(fileName, newName);
                _resultFiles.Add(file, newFile);
            }

            // Display all pending changes
            Confirmation confirmation = new Confirmation();

            confirmation.PrintResults(_resultFiles);
            confirmation.ShowDialog();
        }

        public void Rename()
        {
            // Finally, actually rename the files
            foreach (KeyValuePair<string, string> files in _resultFiles)
            {
                File.Move(files.Key, files.Value);
            }
        }
    }
}