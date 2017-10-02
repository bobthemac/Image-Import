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
using System.Collections;
using System.ComponentModel;

namespace Image_Import
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bWorker; // Background worker for copying files 
        string kDrivePath; // Path of removable drive to copy from
        string kCopyToPath; // Path to copy files to on local machine

        public MainWindow()
        {
            /*
             * Called on start calls the standard Initialize function then calls 
             * the get media drive function to populate the list straight away.
             */ 
            InitializeComponent();
            GetMediaDrive();
            kDrivePath = driveCombo.Text;
        }

        private void GetMediaDrive()
        {
            /*
             * Gets the avalible drives that are of type removeable and adds them
             * to an array, thn sets the display comboBox to index 0 to remove the empty 
             * gap and move the top drive into the selected view.
             */ 
            DriveInfo[] driveList = DriveInfo.GetDrives();

            foreach (DriveInfo drive in driveList)
            {
                if (drive.DriveType == DriveType.Removable && !driveCombo.Items.Contains(drive.Name))
                {
                    driveCombo.Items.Add(drive.Name);
                }
            }
            driveCombo.SelectedIndex = 0;
        }

        private void bWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            /*
             * Sets the value of the progress bar when the progress has changed to the
             * value repoted by the background worker.
             */ 
            importProgress.Value = e.ProgressPercentage;
        }

        private void bWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            /*
             * Sets the directories then gets the files and sets initial progress
             * values. Files are lopped through one by one and a matching directory 
             * is found then the file is copied, if the file is a duplicate the user
             * is notified and asked if they want to overwrite the file. When a file has
             * been copied the progressCount is incremented when it is equal to the 
             * percentIncrementVal the percentage is increased and reported.
             */ 
            DirectoryInfo importDir = new DirectoryInfo(kCopyToPath);
            DirectoryInfo copyDir = new DirectoryInfo(kDrivePath);
            FileInfo[] copyFiles = copyDir.GetFiles("*.*", SearchOption.AllDirectories);

            int percentIncrementVal = copyFiles.Count() / 100;

            int progressCount = 0;
            int percentCount = 0;

            bWorker.ReportProgress(percentCount);

            foreach (FileInfo fi in copyFiles)
            {
                string dateMatch = fi.CreationTime.ToString("yyyy_MM_dd");
                string copyPath = System.IO.Path.Combine(kCopyToPath, dateMatch, fi.Name.ToString());

                DirectoryInfo[] dirs = importDir.GetDirectories();
                foreach (DirectoryInfo di in dirs)
                {
                    if (di.Name.ToString() == dateMatch)
                    {
                        progressCount++;
                        try
                        {
                            fi.CopyTo(copyPath, false);
                        }
                        catch (IOException ex)
                        {
                            DialogResult result = System.Windows.Forms.MessageBox.Show(ex.Message + " Do you want to overwrite it.", "Missing File", MessageBoxButtons.YesNo);
                            switch (result)
                            {
                                case System.Windows.Forms.DialogResult.Yes:
                                    fi.CopyTo(copyPath, true);
                                    break;
                                case System.Windows.Forms.DialogResult.No:
                                    break;
                            }
                        }
                    }
                    if(progressCount > percentIncrementVal)
                    {
                        percentCount++;
                        bWorker.ReportProgress(percentCount);
                        progressCount = 0;
                    }

                }
            }

            bWorker.ReportProgress(100);
        }

        private void GetFiles()
        {
            /*
             * Creates a new background worker for the copy task as not to
             * lock the UI and make the program unresponsive. Sets the refences 
             * to the background worker handlers and enables progress reporting, 
             * then runs the worker.
             */ 
            bWorker = new BackgroundWorker();

            bWorker.DoWork += new DoWorkEventHandler(bWorker_DoWork);
            bWorker.ProgressChanged += new ProgressChangedEventHandler(bWorker_ProgressChanged);
            bWorker.WorkerReportsProgress = true;

            bWorker.RunWorkerAsync();
        }

        private Hashtable GetFolderNames()
        {
            /*
             * Checks the dates of all the files on the removable drive and 
             * creates a hashtable of unique dates that can then be used as
             * a list to create folders in the import location.
             */ 
            // TODO Optimize function
            Hashtable created = new Hashtable();

            DirectoryInfo dirInfo = new DirectoryInfo(kDrivePath);
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
            /*
             * Gets all files without geting the files in hidden folders it returns a list
             * of FileInfo objects.
             */ 
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
            /*
             * Takes the path that files will be copied to and creates subdirectorys
             * for each diffrent required folder name.
             */
            DirectoryInfo importLocal = new DirectoryInfo(kCopyToPath);
            foreach(DictionaryEntry h in GetFolderNames())
            {
                importLocal.CreateSubdirectory(h.Key.ToString());                
            }
        }

        private bool IsValidPath(string path)
        {
            /*
             * Checks that a given path is valid and there are no other issues with the
             * selected path. If an exception occours the user will be shown a message 
             * explaing the issue and then finaly if the path is invalid another message 
             * will be displayed to show that the selected path is invalid.
             */ 
            FileInfo fi = null;
            try
            {
                fi = new FileInfo(path);
            }
            catch(ArgumentException e) { System.Windows.Forms.MessageBox.Show(e.Message); }
            catch(PathTooLongException e) { System.Windows.Forms.MessageBox.Show(e.Message); }
            catch(NotSupportedException e) { System.Windows.Forms.MessageBox.Show(e.Message); }
            if(ReferenceEquals(fi, null))
            {
                System.Windows.Forms.MessageBox.Show("The file path entered is invalid!");
                return true;
            }

            return false;
        }
        
        private void ImportClick(object sender, RoutedEventArgs e)
        {
            /*
             * Starts the process for importing by first checking if the path is valid
             * and if it is then calling the functions to create the folders and start 
             * copying.
             */
            if (!IsValidPath(pathBox.Text))
            {
                CreateFolder();
                GetFiles();
            }
        }

        private void LocationClick(object sender, RoutedEventArgs e)
        {
            /*
             * Opens a folder browsing window to all the user to select where they want to
             * import the files to. It then changes the text on the text box used to display 
             * the selected path.
             */
            string folderPath = "";
            FolderBrowserDialog dialogFolder = new FolderBrowserDialog();

            if (dialogFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = dialogFolder.SelectedPath;
            }
            kCopyToPath = folderPath;
            pathBox.Text = kCopyToPath;
        }

        private void DriveComboOpen(object sender, EventArgs e)
        {
            /*
             * Function called when the ComboBox selecting the source is cliked causing it to open 
             */
            GetMediaDrive();
        }
    }
}
