﻿using System;
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
using System.Collections;

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
            DirectoryInfo importDir = new DirectoryInfo(pathBox.Text);
            DirectoryInfo copyDir = new DirectoryInfo(driveCombo.Text);
            FileInfo[] copyFiles = copyDir.GetFiles("*.*", SearchOption.AllDirectories);

            foreach(FileInfo fi in copyFiles)
            {
                string dateMatch = fi.CreationTime.ToString("yyyy_MM_dd");

                string copyPath = System.IO.Path.Combine(pathBox.Text, dateMatch, fi.Name.ToString());

                DirectoryInfo[] dirs = importDir.GetDirectories();
                foreach(DirectoryInfo di in dirs)
                {
                    if (di.Name.ToString() == dateMatch)
                    {
                        try
                        {
                            fi.CopyTo(copyPath, false);
                        } catch( IOException e)
                        {
                            DialogResult result = System.Windows.Forms.MessageBox.Show(e.Message + " Do you want to overwrite it.", "Missing File", MessageBoxButtons.YesNo);
                            switch(result)
                            {
                                case System.Windows.Forms.DialogResult.Yes:
                                    fi.CopyTo(copyPath, true);
                                    break;
                                case System.Windows.Forms.DialogResult.No:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private Hashtable GetFolderNames()
        {
            // TODO Optimize function
            Hashtable created = new Hashtable();

            DirectoryInfo dirInfo = new DirectoryInfo(driveCombo.Text);
            FileInfo[] files = GetNonHidden(dirInfo).ToArray();

            foreach (FileInfo fi in files)
            {
                string dateStr = fi.CreationTime.ToString("yyyy_MM_dd"); //YYYY_MM_DD
                if(!created.Contains(dateStr))
                {
                    created.Add(dateStr, true);
                }
            }
            return created;
        }

        private List<FileInfo> GetNonHidden(DirectoryInfo baseDirectory)
        {
            var file = new List<FileInfo>();
            file.AddRange(baseDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => (f.Attributes & FileAttributes.Hidden) == 0));
            foreach(var directory in baseDirectory.GetDirectories("*.*", SearchOption.TopDirectoryOnly).Where(f => (f.Attributes & FileAttributes.Hidden) == 0))
            {
                file.AddRange(GetNonHidden(directory));
            }

            return file; 
        }

        private void CreateFolder()
        {
            DirectoryInfo importLocal = new DirectoryInfo(pathBox.Text);
            foreach(DictionaryEntry h in GetFolderNames())
            {
                importLocal.CreateSubdirectory(h.Key.ToString());                
            }
        }
        
        private void ImportClick(object sender, RoutedEventArgs e)
        {
            //TODO check there is a valid location
            //TODO stop UI freezing when import in progress and increment progress bar on copy
            if (pathBox.Text != "")
            {
                CreateFolder();
                GetFiles();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("You have not selected a location to import to!");
            }
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

        private void DriveComboOpen(object sender, EventArgs e)
        {
            GetMediaDrive();
        }
    }
}
