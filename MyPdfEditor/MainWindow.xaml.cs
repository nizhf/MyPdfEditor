using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MyPdfEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public class FileInfo : INotifyPropertyChanged
        {
            private string name;
            private int pageStart;
            private int pageEnd;
            private int pageTotal;
            public event PropertyChangedEventHandler PropertyChanged;

            public FileInfo(string name, int pageStart, int pageEnd, int pageTotal) {
                this.name = name;
                this.pageStart = pageStart;
                this.pageEnd = pageEnd;
                this.pageTotal = pageTotal;
            }

            public int PageTotal {
                get => pageTotal;
                set {
                    pageTotal = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PageTotal"));
                }
            }
            public string Name {
                get => name;
                set {
                    name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
            public int PageStart {
                get => pageStart;
                set {
                    pageStart = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PageStart"));
                }
            }
            public int PageEnd {
                get => pageEnd;
                set {
                    pageEnd = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PageEnd"));
                }
            }
        }

        public MainWindow() {
            InitializeComponent();
            fileList = new ObservableCollection<FileInfo>();
            pageList = new ObservableCollection<FileInfo>();
            defaultPath = "C:\\";

        }

        private ObservableCollection<FileInfo> fileList;
        public ObservableCollection<FileInfo> FileList { get => fileList; set => fileList = value; }

        private ObservableCollection<FileInfo> pageList;
        public ObservableCollection<FileInfo> PageList { get => pageList; set => pageList = value; }

        private string defaultPath;
        public string DefaultPath { get => defaultPath; set => defaultPath = value; }


        private void MergePdfFiles(string outputFile) {
            Document document = new Document();
            PdfWriter writer = null;
            try {
                writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return;
            }

            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage;
            foreach (FileInfo item in FileList) {                
                try {
                    PdfReader reader = new PdfReader(item.Name);
                    PdfReader.unethicalreading = true;
                    for (int j = 1; j <= reader.NumberOfPages; j++) {
                        document.SetPageSize(reader.GetPageSizeWithRotation(j));
                        document.NewPage();
                        newPage = writer.GetImportedPage(reader, j);
                        int rotation = reader.GetPageRotation(j);
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
                catch (Exception e) {
                    MessageBox.Show(e.Message);
                    return;
                }                
            }
            document.Close();
            MessageBox.Show("Merge completed!");
        }

        private void MergePdfFilesAdvance(string outputFile) {
            Document document = new Document();
            PdfWriter writer = null;
            try {
                writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return;
            }
            
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage;
            foreach (FileInfo item in PageList) {
                PdfReader reader = new PdfReader(item.Name);
                int rotation;
                for (int j = item.PageStart; j <= item.PageEnd; j++) {
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
            foreach (string name in ofd.FileNames) {
                var reader = new PdfReader(name);
                FileList.Add(new FileInfo(name, 1, reader.NumberOfPages, reader.NumberOfPages));
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
            var items = ListView_merge.SelectedItems;
            if (items.Count <= 0)
                return;
            while (items.Count > 0) {
                FileList.Remove(items[0] as FileInfo);
            }
        }

        private void Btn_merge_clear_Click(object sender, RoutedEventArgs e) {
            ClearList();
        }

        private void Btn_merge_up_Click(object sender, RoutedEventArgs e) {
            int index = ListView_merge.SelectedIndex;
            if (index <= 0) {
                return;
            }
            FileList.Insert(index - 1, FileList[index]);
            FileList.RemoveAt(index + 1);
            ListView_merge.SelectedIndex = index - 1;
            if (ListView_merge.SelectedIndex == 0) {
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
            FileList.Insert(index + 2, FileList[index]);
            FileList.RemoveAt(index);
            ListView_merge.SelectedIndex = index + 1;
            if (ListView_merge.SelectedIndex == ListView_merge.Items.Count - 1) {
                Btn_merge_up.IsEnabled = true;
                Btn_merge_down.IsEnabled = false;
            }
            else {
                Btn_merge_up.IsEnabled = true;
                Btn_merge_down.IsEnabled = true;
            }
        }

        private void ListView_merge_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int index = ListView_merge.SelectedIndex;
            if (index == -1 || ListView_merge.Items.Count == 1) {
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
            var items = ListView_left.SelectedItems;
            if (items.Count <= 0)
                return;
            while (items.Count > 0) {
                FileList.Remove(items[0] as FileInfo);
            }
        }

        private void Btn_advanced_clear_Click(object sender, RoutedEventArgs e) {
            ClearList();
        }

        private void Btn_advanced_right_Click(object sender, RoutedEventArgs e) {
            var items = ListView_left.SelectedItems;
            if (items.Count <= 0)
                return;
            foreach (FileInfo item in items) {
                PageList.Add(new FileInfo(item.Name, 1, item.PageTotal, item.PageTotal));
            }
        }

        private void Btn_advanced_left_Click(object sender, RoutedEventArgs e) {
            var items = ListView_right.SelectedItems;
            if (items.Count <= 0)
                return;
            while (items.Count > 0) {
                PageList.Remove(items[0] as FileInfo);
            }
        }

        private void Btn_advanced_up_Click(object sender, RoutedEventArgs e) {
            int index = ListView_right.SelectedIndex;
            if (index <= 0) {
                return;
            }
            PageList.Insert(index - 1, PageList[index]);
            PageList.RemoveAt(index + 1);
            ListView_right.SelectedIndex = index - 1;
            if (ListView_right.SelectedIndex == 0) {
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
            ListView_right.SelectedIndex = index + 1;
            if (ListView_right.SelectedIndex == ListView_right.Items.Count - 1) {
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
            if (index == -1 || ListView_right.Items.Count == 1) {
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

        private void TextBox_start_LostFocus(object sender, RoutedEventArgs e) {
            var currentBox = sender as TextBox;
            var item = currentBox.DataContext as FileInfo;
            if (item.PageStart <= 0) 
                item.PageStart = 1;
            if (item.PageEnd > item.PageTotal) 
                item.PageEnd = item.PageTotal;
            if (item.PageStart > item.PageEnd) 
                item.PageStart = item.PageEnd;
        }

        private void TextBox_end_LostFocus(object sender, RoutedEventArgs e) {
            var currentBox = sender as TextBox;
            var item = currentBox.DataContext as FileInfo;
            if (item.PageStart <= 0) 
                item.PageStart = 1;
            if (item.PageEnd > item.PageTotal)
                item.PageEnd = item.PageTotal;
            else if (item.PageStart > item.PageEnd)
                item.PageEnd = item.PageStart;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) {
                e.Handled = true;
            }
            else if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Delete 
                || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Enter)) {
                e.Handled = true;
            }
        }

        private void TextBox_start_TextChanged(object sender, TextChangedEventArgs e) {
            var currentBox = sender as TextBox;
            if (currentBox.Text.Length == 0) {
                currentBox.Text = "1";
                currentBox.SelectAll();
            }
        }

        private void TextBox_end_TextChanged(object sender, TextChangedEventArgs e) {
            var currentBox = sender as TextBox;
            var item = currentBox.DataContext as FileInfo;
            if (currentBox.Text.Length == 0) {
                currentBox.Text = item.PageTotal.ToString();
                currentBox.SelectAll();
            }
        }
        
    }
}

