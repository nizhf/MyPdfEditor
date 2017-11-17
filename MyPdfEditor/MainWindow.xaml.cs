using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace MyPdfEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() {
            FileList = new List<string>();
            InitializeComponent();
        }

        private List<string> fileList;
        private string defaultPath;

        public List<string> FileList { get => fileList; set => fileList = value; }
        public string DefaultPath { get => defaultPath; set => defaultPath = value; }

        private void btn_merge_merge_Click(object sender, RoutedEventArgs e) {
            if (FileList.Count == 0) {
                MessageBox.Show("Please first add PDF files!");
                return;
            }
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog() {
                Title = "Select a location to save the merged file",
                Filter = "PDF documents |*.pdf",
                InitialDirectory = DefaultPath,
                FileName = "Merge_Output.pdf"
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }
            MergePdfFiles(FileList, sfd.FileName);            
        }

        private void btn_merge_remove_Click(object sender, RoutedEventArgs e) {
            int index = listBox_merge_fileList.SelectedIndex;
            if (index <= -1) {
                return;
            }
            FileList.RemoveAt(index);
            listBox_merge_fileList.Items.RemoveAt(index);
        }

        private void btn_merge_clear_Click(object sender, RoutedEventArgs e) {
            FileList.Clear();
            listBox_merge_fileList.Items.Clear();
        }

        private void btn_merge_add_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog() {
                Multiselect = true,
                Title = "Select a PDF file",
                DefaultExt = ".pdf",
                Filter = "PDF Documents |*.pdf"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }
            DefaultPath = ofd.FileNames[0].Remove(ofd.FileNames[0].Length - ofd.SafeFileName.Length);
            for (int i = 0; i < ofd.FileNames.Length; i++) {
                FileList.Add(ofd.FileNames[i]);
                listBox_merge_fileList.Items.Add(ofd.FileNames[i]);
            }
        }

        private void MergePdfFiles(List<string> fileList, string outputFile) {
            PdfReader reader;
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage;
            for (int i = 0; i < FileList.Count; i++) {
                reader = new PdfReader(FileList[i]);
                int rotation;
                for (int j = 1; j <= reader.NumberOfPages; j++) {
                    document.SetPageSize(reader.GetPageSizeWithRotation(j));
                    document.NewPage();
                    newPage = writer.GetImportedPage(reader, j);
                    rotation = reader.GetPageRotation(j);
                    if (rotation == 90) {
                        cb.AddTemplate(newPage, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(j).Height);
                    }
                    else if (rotation == 180) {
                        cb.AddTemplate(newPage, -1f, 0, 0, -1f, reader.GetPageSizeWithRotation(j).Width, reader.GetPageSizeWithRotation(j).Height);
                    }
                    else if (rotation == 270) {
                        cb.AddTemplate(newPage, 0, 1f, -1f, 0, reader.GetPageSizeWithRotation(j).Width, 0);
                    }
                    else {
                        cb.AddTemplate(newPage, 1f, 0, 0, 1f, 0, 0);
                    }
                }
            }
            document.Close();
            MessageBox.Show("Merge completed!");
        }

        private void btn_merge_up_Click(object sender, RoutedEventArgs e) {
            int index = listBox_merge_fileList.SelectedIndex;
            if (index <= 0) {
                return;
            }
            fileList.Insert(index - 1, FileList[index]);
            fileList.RemoveAt(index + 1);
            listBox_merge_fileList.Items.Insert(index - 1, listBox_merge_fileList.Items.GetItemAt(index));
            listBox_merge_fileList.Items.RemoveAt(index + 1);
            listBox_merge_fileList.SelectedIndex = index - 1;
        }

        private void btn_merge_down_Click(object sender, RoutedEventArgs e) {
            int index = listBox_merge_fileList.SelectedIndex;
            if (index == -1 || index == listBox_merge_fileList.Items.Count - 1) {
                return;
            }
            fileList.Insert(index + 2, FileList[index]);
            fileList.RemoveAt(index);
            listBox_merge_fileList.Items.Insert(index + 2, listBox_merge_fileList.Items.GetItemAt(index));
            listBox_merge_fileList.Items.RemoveAt(index);
            listBox_merge_fileList.SelectedIndex = index + 1;
        }

        //private void listBox_merge_fileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
        //    int index = listBox_merge_fileList.SelectedIndex;
        //    MessageBox.Show("changed");
        //    if (index == 0) {
        //        btn_merge_up.IsEnabled = false;
        //        btn_merge_down.IsEnabled = true;
        //    }
        //    else if (index == listBox_merge_fileList.Items.Count - 1) {
        //        btn_merge_up.IsEnabled = true;
        //        btn_merge_down.IsEnabled = false;
        //    }
        //    else {
        //        btn_merge_up.IsEnabled = true;
        //        btn_merge_down.IsEnabled = true;
        //    }
        //}
    }
}
