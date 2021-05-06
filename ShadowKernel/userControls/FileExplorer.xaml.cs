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
using System.IO;
using System.Windows.Media.Imaging;
using static ShadowKernel.Classes.Server;
using ShadowKernel.Classes;

namespace ShadowKernel.userControls
{
    /// <summary>
    /// Логика взаимодействия для FileExplorer.xaml
    /// </summary>
    public partial class FileExplorer : Window
    {
        public int ConnectionID { get; set; }
        public bool Update { get; set; }
        public string CurDir;
        public FileExplorer()
        {
            InitializeComponent();
            Update = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Update = false;
        }


        private void currentDir_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            try
            {
                MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("GetDF{" + currentDir.Text + "}"));
            }
            catch
            {
                    string d = currentDir.Text;
                    currentDir.Text = d.Substring(0, d.LastIndexOf("\\"));
                    MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("GetDF{" + currentDir.Text + "}"));
            }
        }

        private void dtgFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                SettingsServer.sFile a = (SettingsServer.sFile)dtgFiles.SelectedItem; 
                if (a.fExt == "Папка с файлами")
                {
                    if (currentDir.Text[currentDir.Text.Length-1] != '\\') currentDir.Text += "\\";
                    CurDir = a.fName;
                    MainServer.Send(ConnectionID,Encoding.UTF8.GetBytes("GetDF{" + currentDir.Text + a.fName + "}"));
                }
                else
                {
                    MainServer.Send(ConnectionID,Encoding.UTF8.GetBytes("TryOpen{" + currentDir.Text + @"\" + a.fName + a.fExt + "}"));
                }
            }
            catch { }
        }
        private void dtgFiles_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            currentDir.Text += CurDir;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("GoUpDir"));
        }




        private void text_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void dtgDrives_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                SettingsServer.driveInfo d = (SettingsServer.driveInfo)dtgDrives.SelectedItem;

                MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("GetDF{" + d.dId + "\\}"));
            }
            catch { }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingsServer.sFile sFile = (SettingsServer.sFile)dtgFiles.SelectedItem;
                TempDataHelper.FileName = sFile.fName;
                TempDataHelper.FileExt = sFile.fExt;
                MainServer.Send(ConnectionID,Encoding.UTF8.GetBytes("GetFile{[" + currentDir.Text + @"\" + sFile.fName +"E<E"+ sFile.fExt + "]}"));
            }
            catch { }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    string FileString = dlg.FileName;
                    byte[] FileBytes;
                    using (FileStream FS = new FileStream(FileString, FileMode.Open))
                    {
                        FileBytes = new byte[FS.Length];
                        FS.Read(FileBytes, 0, FileBytes.Length);
                    }
                    MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("StartFileReceive{" + currentDir.Text + @"\" + Path.GetFileName(dlg.FileName) + "}"));
                    MainServer.Send(ConnectionID, FileBytes);
                }
            }
            catch {}

        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingsServer.sFile sFile = (SettingsServer.sFile)dtgFiles.SelectedItem;
                if (sFile.fExt != "Папка с файлами")
                MainServer.Send(ConnectionID,Encoding.UTF8.GetBytes("DeleteFile{" + currentDir.Text + @"\" + sFile.fName +
                                            sFile.fExt + "}"));
            }
            catch { }
        }
    }
}
