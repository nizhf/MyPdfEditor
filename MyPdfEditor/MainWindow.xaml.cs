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
            fileList = new List<string>();
            InitializeComponent();
        }

        private List<string> fileList;
        private string defaultPath;

        private void btn_merge_Click(object sender, RoutedEventArgs e) {
            if (fileList.Count == 0) {
                MessageBox.Show("Please first add PDF files!");
                return;
            }
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Select a location to save the merged file";
            sfd.Filter = "PDF documents |*.pdf";
            sfd.InitialDirectory = defaultPath;
            sfd.FileName = "Merge_Output.pdf";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            string outputFile = sfd.FileName;
            mergePdfFiles(fileList, outputFile);            
        }

        private void btn_remove_Click(object sender, RoutedEventArgs e) {

        }

        private void btn_clear_Click(object sender, RoutedEventArgs e) {
            fileList.Clear();
            textBox_fileList.Text = "All PDF files to merge:";
        }

        private void btn_add_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Title = "Select a PDF file";
            ofd.DefaultExt = ".pdf";
            ofd.Filter = "PDF Documents |*.pdf";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            string[] fileName = ofd.FileNames;
            string nameWithoutPath = ofd.SafeFileName;
            defaultPath = fileName[0].Remove(fileName[0].Length - nameWithoutPath.Length);
            for (int i = 0; i < fileName.Length; i++) {
                fileList.Add(fileName[i]);
                textBox_fileList.AppendText("\n" + fileName[i]);                
            }
        }

        private void mergePdfFiles(List<string> fileList, string outputFile) {
            PdfReader reader;
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage;
            for (int i = 0; i < fileList.Count; i++) {
                string newPath = fileList[i];
                reader = new PdfReader(newPath);
                int pageNum = reader.NumberOfPages;
                int rotation;
                for (int j = 1; j <= pageNum; j++) {
                    document.SetPageSize(reader.GetPageSizeWithRotation(j));
                    document.NewPage();
                    newPage = writer.GetImportedPage(reader, j);
                    rotation = reader.GetPageRotation(j);
                    if (rotation == 90)
                        cb.AddTemplate(newPage, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(j).Height);
                    else if (rotation == 180)
                        cb.AddTemplate(newPage, -1f, 0, 0, -1f, reader.GetPageSizeWithRotation(j).Width, reader.GetPageSizeWithRotation(j).Height);
                    else if (rotation == 270)
                        cb.AddTemplate(newPage, 0, 1f, -1f, 0, reader.GetPageSizeWithRotation(j).Width, 0);
                    else
                        cb.AddTemplate(newPage, 1f, 0, 0, 1f, 0, 0);
                }
            }
            document.Close();
            MessageBox.Show("Merge completed!");
        }
    }
}
