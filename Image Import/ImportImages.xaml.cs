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

        private void ImportClick(object sender, RoutedEventArgs e)
        {

        }

        private void LocationClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
