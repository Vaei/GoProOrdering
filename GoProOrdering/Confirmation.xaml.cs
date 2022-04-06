using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoProOrdering
{
    /// <summary>
    /// Interaction logic for Confirmation.xaml
    /// </summary>
    public partial class Confirmation : Window
    {
        public Confirmation()
        {
            InitializeComponent();
        }

        public void PrintResults(Dictionary<string, string> resultFiles)
        {
            Text_Message.Text = "";

            foreach (KeyValuePair<string, string> files in resultFiles)
            {
                string ext = "." + files.Key.Split('.').Last();

                string oldFile = MainWindow.GetFileName(files.Key) + ext;
                string newFile = MainWindow.GetFileName(files.Value) + ext;
                Text_Message.Text += oldFile + " -> " + newFile + "\r\n";
            }
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)?.Rename();
            Close();
        }

        private void Button_Abort_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
