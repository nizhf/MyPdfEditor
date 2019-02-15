using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.awt.geom;

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
            private int rotation;
            private int flip;
            public event PropertyChangedEventHandler PropertyChanged;

            public FileInfo(string name, int pageStart, int pageEnd, int pageTotal, int rotation, int flip) {
                this.name = name;
                this.pageStart = pageStart;
                this.pageEnd = pageEnd;
                this.pageTotal = pageTotal;
                this.rotation = rotation;
                this.flip = flip;
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

            public int Rotation {
                get => rotation;
                set {
                    rotation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Rotation"));
                }
            }

            public int Flip {
                get => flip;
                set {
                    flip = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Flip"));
                }
            }

        }

        public MainWindow() {
            InitializeComponent();
            FileList = new ObservableCollection<FileInfo>();
            PageList = new ObservableCollection<FileInfo>();
            DefaultPath = "C:\\";
        }

        public ObservableCollection<FileInfo> FileList { get; set; }
        public ObservableCollection<FileInfo> PageList { get; set; }
        public string DefaultPath { get; set; }


        private void MergePdfFiles(string outputFile) {
            Document document = new Document();
            try {
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                foreach (FileInfo item in FileList) {                
                    PdfReader reader = new PdfReader(item.Name);
                    PdfReader.unethicalreading = true;
                    for (int j = 1; j <= reader.NumberOfPages; j++) {
                        document.SetPageSize(reader.GetPageSizeWithRotation(j));
                        document.NewPage();
                        cb.AddTemplate(writer.GetImportedPage(reader, j), 0, 0);
                    }          
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return;
            }
            document.Close();
            MessageBox.Show("Merge completed!");
        }

        private void MergePdfFilesAdvance(string outputFile) {
            Document document = new Document();            
            try {
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));                
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                foreach (FileInfo item in PageList) {
                    PdfReader reader = new PdfReader(item.Name);
                    int rotation = reader.GetPageRotation(item.PageStart) + item.Rotation;
                    rotation = rotation - rotation / 360 * 360;
                    int flip = item.Flip;
                    for (int j = item.PageStart; j <= item.PageEnd; j++) {
                        Rectangle rc = new Rectangle(reader.GetPageSize(j));
                        if (rotation / 90 % 2 == 1) {
                            rc = rc.Rotate();                            
                        }
                        document.SetPageSize(rc);
                        document.NewPage();
                        if (flip == 0) {
                            if (rotation == 90) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 0, -1f, 1f, 0, 0, rc.Height);
                            }
                            else if (rotation == 180) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), -1f, 0, 0, -1f, rc.Width, rc.Height);
                            }
                            else if (rotation == 270) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 0, 1f, -1f, 0, rc.Width, 0);
                            }
                            else {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 1f, 0, 0, 1f, 0, 0);
                            }
                        }
                        else if (flip == 1) {
                            if (rotation == 90) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 0, 1f, 1f, 0, 0, 0);
                            }
                            else if (rotation == 180) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 1f, 0, 0, -1f, 0, rc.Height);
                            }
                            else if (rotation == 270) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 0, -1f, -1f, 0, rc.Width, rc.Height);
                            }
                            else {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), -1, 0, 0, 1, rc.Width, 0);
                            }
                        }
                        else if (flip == 2) {
                            if (rotation == 90) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 0, -1f, -1f, 0, rc.Width, rc.Height);
                            }
                            else if (rotation == 180) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), -1, 0, 0, 1, rc.Width, 0);
                            }
                            else if (rotation == 270) {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 0, 1f, 1f, 0, 0, 0);
                            }
                            else {
                                cb.AddTemplate(writer.GetImportedPage(reader, j), 1f, 0, 0, -1f, 0, rc.Height);
                            }
                        }
                                       
                    }
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return;
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
                FileList.Add(new FileInfo(name, 1, reader.NumberOfPages, reader.NumberOfPages, 0, 0));
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
                PageList.Add(new FileInfo(item.Name, 1, item.PageTotal, item.PageTotal, 0, 0));
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

        private void TextBox_rotation_LostFocus(object sender, RoutedEventArgs e) {
            var currentBox = sender as TextBox;
            var item = currentBox.DataContext as FileInfo;
            if (item.Rotation - item.Rotation / 90 * 90 <= 45) 
                item.Rotation = item.Rotation / 90 * 90;
            else 
                item.Rotation = (item.Rotation / 90 + 1) * 90;            
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

        private void TextBox_rotation_TextChanged(object sender, TextChangedEventArgs e) {
            var currentBox = sender as TextBox;
            var item = currentBox.DataContext as FileInfo;
            if (currentBox.Text.Length == 0) {
                currentBox.Text = "0";
                currentBox.SelectAll();
            }
        }

    }
}

