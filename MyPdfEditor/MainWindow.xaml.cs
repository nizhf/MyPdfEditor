using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyPdfEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    

    public partial class MainWindow : Window
    {
        public class FilePages : INotifyPropertyChanged
        {
            private string name;
            private int pageStart;
            private int pageEnd;
            public event PropertyChangedEventHandler PropertyChanged;

            public FilePages(string name, int pageStart, int pageEnd) {
                this.name = name;
                this.pageStart = pageStart;
                this.pageEnd = pageEnd;
            }

            public string Name { get => name; set => name = value; }
            public int PageStart { get => pageStart; set => pageStart = value; }
            public int PageEnd { get => pageEnd; set => pageEnd = value; }
        }

        public class FileInfo : INotifyPropertyChanged
        {
            private string name;
            private int pageTotal;
            public event PropertyChangedEventHandler PropertyChanged;

            public FileInfo(string name, int pageTotal) {
                this.name = name;
                this.pageTotal = pageTotal;
            }

            public int PageTotal { get => pageTotal; set => pageTotal = value; }
            public string Name { get => name; set => name = value; }
        }

        public MainWindow() {                       
            InitializeComponent();
            FileList = new ObservableCollection<FileInfo>();
            pageList = new ObservableCollection<FilePages>();
            defaultPath = "C:\\";

        }

        private ObservableCollection<FileInfo> fileList;
        public ObservableCollection<FileInfo> FileList { get => fileList; set => fileList = value; }
        private ObservableCollection<FilePages> pageList;
        public ObservableCollection<FilePages> PageList { get => pageList; set => pageList = value; }

        private string defaultPath;
        public string DefaultPath { get => defaultPath; set => defaultPath = value; }
       

        private void MergePdfFiles(string outputFile) {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage;
            for (int i = 0; i < FileList.Count; i++) {
                PdfReader reader = new PdfReader(FileList[i].Name);
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

        private void MergePdfFilesAdvance(string outputFile) {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage;
            for (int i = 0; i < PageList.Count; i++) {
                PdfReader reader = new PdfReader(PageList[i].Name);
                int rotation;
                for (int j = PageList[i].PageStart; j <= PageList[i].PageEnd; j++) {
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

        private void AddItem() {
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
                var reader = new PdfReader(ofd.FileNames[i]);
                FileList.Add(new FileInfo(ofd.FileNames[i], reader.NumberOfPages));
            }
        }

        private void ClearList() {
            FileList.Clear();
            PageList.Clear();
            Btn_merge_up.IsEnabled = false;
            Btn_merge_down.IsEnabled = false;
            Btn_advanced_down.IsEnabled = false;
            Btn_advanced_up.IsEnabled = false;
        }

        private void Btn_merge_merge_Click(object sender, RoutedEventArgs e) {
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
            MergePdfFiles(sfd.FileName);            
        }

        private void Btn_merge_add_Click(object sender, RoutedEventArgs e) {
            AddItem();
        }

        private void Btn_merge_remove_Click(object sender, RoutedEventArgs e) {
            int index = ListView_merge.SelectedIndex;
            if (index < 0)
                return;
            FileList.RemoveAt(index);
        }

        private void Btn_merge_clear_Click(object sender, RoutedEventArgs e) {
            ClearList();
        }                

        private void Btn_merge_up_Click(object sender, RoutedEventArgs e) {
            int index = ListView_merge.SelectedIndex;
            if (index <= 0) {
                return;
            }
            fileList.Insert(index - 1, FileList[index]);
            fileList.RemoveAt(index + 1);
            if (index - 1 == 0) {
                Btn_merge_up.IsEnabled = false;
                Btn_merge_down.IsEnabled = true;
            }
            else {
                Btn_merge_up.IsEnabled = true;
                Btn_merge_down.IsEnabled = true;
            }
        }

        private void Btn_merge_down_Click(object sender, RoutedEventArgs e) {
            int index = ListView_merge.SelectedIndex;
            if (index == -1 || index == ListView_merge.Items.Count - 1) {
                return;
            }
            fileList.Insert(index + 2, FileList[index]);
            fileList.RemoveAt(index);
            if (index + 1 == ListView_merge.Items.Count - 1) {
                Btn_merge_up.IsEnabled = true;
                Btn_merge_down.IsEnabled = false;
            }
            else {
                Btn_merge_up.IsEnabled = true;
                Btn_merge_down.IsEnabled = true;
            }
        }

        private void ListView_merge_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            int index = ListView_merge.SelectedIndex;
            if (index == -1) {
                Btn_merge_up.IsEnabled = false;
                Btn_merge_down.IsEnabled = false;
            }
            else if (index == 0) {
                Btn_merge_up.IsEnabled = false;
                Btn_merge_down.IsEnabled = true;
            }
            else if (index == ListView_merge.Items.Count - 1) {
                Btn_merge_up.IsEnabled = true;
                Btn_merge_down.IsEnabled = false;
            }
            else {
                Btn_merge_up.IsEnabled = true;
                Btn_merge_down.IsEnabled = true;
            }
        }

        private void Btn_advanced_add_Click(object sender, RoutedEventArgs e) {
            AddItem();
        }

        private void Btn_advanced_remove_Click(object sender, RoutedEventArgs e) {
            int index = ListView_left.SelectedIndex;
            if (index < 0)
                return;
            FileList.RemoveAt(index);
        }

        private void Btn_advanced_clear_Click(object sender, RoutedEventArgs e) {
            ClearList();
        }

        private void Btn_advanced_right_Click(object sender, RoutedEventArgs e) {
            int index = ListView_left.SelectedIndex;
            if (index < 0)
                return;
            PageList.Add(new FilePages(FileList[index].Name, 1, FileList[index].PageTotal));
        }

        private void Btn_advanced_left_Click(object sender, RoutedEventArgs e) {
            int index = ListView_right.SelectedIndex;
            if (index < 0)
                return;
            PageList.RemoveAt(index);
        }

        private void Btn_advanced_up_Click(object sender, RoutedEventArgs e) {
            int index = ListView_right.SelectedIndex;
            if (index <= 0) {
                return;
            }
            PageList.Insert(index - 1, PageList[index]);
            PageList.RemoveAt(index + 1);
            if (index - 1 == 0) {
                Btn_advanced_up.IsEnabled = false;
                Btn_advanced_down.IsEnabled = true;
            }
            else {
                Btn_advanced_up.IsEnabled = true;
                Btn_advanced_down.IsEnabled = true;
            }
        }

        private void Btn_advanced_down_Click(object sender, RoutedEventArgs e) {
            int index = ListView_right.SelectedIndex;
            if (index == -1 || index == ListView_right.Items.Count - 1) {
                return;
            }
            PageList.Insert(index + 2, PageList[index]);
            PageList.RemoveAt(index);
            if (index + 1 == ListView_right.Items.Count - 1) {
                Btn_advanced_up.IsEnabled = true;
                Btn_advanced_down.IsEnabled = false;
            }
            else {
                Btn_advanced_up.IsEnabled = true;
                Btn_advanced_down.IsEnabled = true;
            }
        }

        private void Btn_advanced_merge_Click(object sender, RoutedEventArgs e) {
            if (PageList.Count == 0) {
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
            MergePdfFilesAdvance(sfd.FileName);
        }

        private void ListView_right_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int index = ListView_right.SelectedIndex;
            if (index == -1) {
                Btn_advanced_up.IsEnabled = false;
                Btn_advanced_down.IsEnabled = false;
            }
            else if (index == 0) {
                Btn_advanced_up.IsEnabled = false;
                Btn_advanced_down.IsEnabled = true;
            }
            else if (index == ListView_right.Items.Count - 1) {
                Btn_advanced_up.IsEnabled = true;
                Btn_advanced_down.IsEnabled = false;
            }
            else {
                Btn_advanced_up.IsEnabled = true;
                Btn_advanced_down.IsEnabled = true;
            }
        }
    }
}
