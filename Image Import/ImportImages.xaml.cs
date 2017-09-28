using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Windows.Forms;

namespace Image_Import
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DriveInfo> drives = new List<DriveInfo>();
   
        public MainWindow()
        {
            InitializeComponent();
            GetMediaDrive();
        }

        private void GetMediaDrive()
        {
            DriveInfo[] driveList = DriveInfo.GetDrives();

            foreach (DriveInfo drive in driveList)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    driveCombo.Items.Add(drive.Name);
                    drives.Add(drive);
                }
            }
            driveCombo.SelectedIndex = 0;
        }

        private void GetFiles()
        {

        }

        private void CreateFolder()
        {
            DateTime created; // File.GetCreatedTime

            DirectoryInfo dirInfo = new DirectoryInfo(pathBox.Text);
            FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);

            foreach(FileInfo fi in files)
            {
                Console.WriteLine("{0}", fi.CreationTime);
            }
        }
        
        private void ImportClick(object sender, RoutedEventArgs e)
        {
            CreateFolder();
        }

        private void LocationClick(object sender, RoutedEventArgs e)
        {
            string folderPath = "";
            FolderBrowserDialog dialogFolder = new FolderBrowserDialog();

            if (dialogFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = dialogFolder.SelectedPath;
            }
            pathBox.Text = folderPath;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
