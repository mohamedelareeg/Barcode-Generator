using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

namespace Barcode_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string lastIndexofBox = "";
        private string lastIndexofFile = "";
        private LogEntryContext _context;
        public MainWindow()
        {
            _context = new LogEntryContext();
            InitializeComponent();
            UpdateLastIndexes();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private async void UpdateLastIndexes()
        {
            lastIndexofBox = "0";
            lastIndexofFile = "0";
            var MaxNumof = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
            if (MaxNumof != null)
            {

                lastIndexofBox = MaxNumof.MaxNumOfBox;
                lastIndexofFile = MaxNumof.MaxNumOfFile;
            }

            lastIndexedBoxText.Text = $"* Last Box: {lastIndexofBox}";
            lastIndexedFileText.Text = $"* Last File: {lastIndexofFile}";
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string barcode = barCodeTextBox.Text.Trim();
            string type = (typeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(barcode))
            {
                barcodeErrorText.Text = "Please enter a count.";
                return;
            }

            if (!int.TryParse(barcode, out int count) || count <= 0)
            {
                barcodeErrorText.Text = "Invalid count. Please enter a positive number.";
                return;
            }

            barcodeErrorText.Text = "";
            GenerateBarCodeCount(barcode, type);
        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            string start = startTextBox.Text.Trim();
            string end = endTextBox.Text.Trim();

            if (!int.TryParse(start, out int startIndex) || !int.TryParse(end, out int endIndex) || startIndex > endIndex)
            {
                barcodeErrorText.Text = "Invalid range. Please enter valid start and end indexes.";
                return;
            }

            string type = (typeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            barcodeErrorText.Text = "";

            for (int i = startIndex; i <= endIndex; i++)
            {
                GenerateBarCode(i.ToString().PadLeft(6, '0'), type);
                //GenerateBarCodeCount(i.ToString(), type);
            }
        }

        private async void GenerateBarCodeCount(string Count, string Type)
        {
            if (string.IsNullOrEmpty(Count))
            {
                return;
            }

            var MaxNumof = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
            if (MaxNumof == null)
            {
                MaxnumOfFileAndBox maxnumOfFileAndBox = new MaxnumOfFileAndBox();
                maxnumOfFileAndBox.MaxNumOfFile = "0000000";
                maxnumOfFileAndBox.MaxNumOfBox = "000000";
                _context.maxnumOfFileAndBoxes.Add(maxnumOfFileAndBox);
                _context.SaveChanges();
            }

            if (Type == "Box")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfBox, out int intmaxofbox);
                int.TryParse(Count, out int plus);
                int end = intmaxofbox + plus;
                for (int i = intmaxofbox + 1; i <= end; i++)
                {
                    string current = i.ToString().PadLeft(6, '0');
                    Max.MaxNumOfBox = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateBarCode(current, Type);
                    // Update the last indexed box
                    lastIndexofBox = current;
                    lastIndexedBoxText.Text = $"* Last Box: {lastIndexofBox}";
                }
                await _context.SaveChangesAsync();
            }

            if (Type == "File")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfFile, out int intmaxoffile);
                int.TryParse(Count, out int plus);
                int end = intmaxoffile + plus;
                for (int i = intmaxoffile + 1; i <= end; i++)
                {
                    string current = i.ToString().PadLeft(7, '0');
                    Max.MaxNumOfFile = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateBarCode(current, Type);
                    // Update the last indexed file
                    lastIndexofFile = current;
                    lastIndexedFileText.Text = $"* Last File: {lastIndexofFile}";
                }
                await _context.SaveChangesAsync();
            }

            // Redirect to another page or handle as needed
        }
        private void GenerateTextBarcode(string data , bool showBarcode , bool isShowImg)
        {
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Namaa.jpg");

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            pd.Print();

            void pd_PrintPage(object sender, PrintPageEventArgs ev)
            {

                // Center the image horizontally
                // Center the image horizontally
                int centerX = (ev.MarginBounds.Left + ev.MarginBounds.Right) / 2;
                System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);

                SolidBrush br = new SolidBrush(System.Drawing.Color.Black);

                if (isShowImg)
                {
                    ev.Graphics.DrawImage(img, 140, 160, 120, 30);
                }
                if (showBarcode)
                {
                    // Generate barcode using ZXing.Net
                    BarcodeWriter barcodeWriter = new BarcodeWriter();
                    barcodeWriter.Format = BarcodeFormat.CODE_128;
                    barcodeWriter.Options = new EncodingOptions
                    {
                        Width = 100,
                        Height = 30,
                        PureBarcode = true
                    };
                    System.Drawing.Image barcodeImage = barcodeWriter.Write(data);

                    // Adjust the position to fit the label
                    int barcodeX = centerX - (barcodeImage.Width / 2) - 20;
                    int barcodeY = 195; // Adjust the vertical position as needed

                    ev.Graphics.DrawImage(barcodeImage, barcodeX, barcodeY, barcodeImage.Width, barcodeImage.Height);
                    //ev.Graphics.DrawImage(barcodeImage, 145, 185);


                    // Adjust the position to fit the label
                    int textX = centerX - (barcodeImage.Width / 2); // You may need to adjust the offset
                    int textY = 230; // Adjust the vertical position as needed
                }

                //ev.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //ev.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //ev.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                //ev.Graphics.Clear(System.Drawing.Color.White);
                //ev.Graphics.SmoothingMode = SmoothingMode.None;

                Font printFont1 = new Font("Times New Roman", 12, System.Drawing.FontStyle.Bold);
                ev.Graphics.DrawString(data, printFont1, br, 150, 230);
                
                //ev.Graphics.DrawString("تم الفهرسة", printFont1, br, textX + 70, textY);
            }
        }
     

        private void GenerateBarCode(string data, string type)
        {
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Namaa.jpg");

            if (type == "File")
            {
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                pd.Print();

                void pd_PrintPage(object sender, PrintPageEventArgs ev)
                {

                    // Center the image horizontally
                    int centerX = (ev.MarginBounds.Left + ev.MarginBounds.Right) / 2;
                    System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);

                    SolidBrush br = new SolidBrush(System.Drawing.Color.Black);

                    ev.Graphics.DrawImage(img, 150, 160, 120, 30);

                    // Generate barcode using ZXing.Net
                    BarcodeWriter barcodeWriter = new BarcodeWriter();
                    barcodeWriter.Format = BarcodeFormat.CODE_39;
                    barcodeWriter.Options = new EncodingOptions
                    {
                        Width = 100,
                        Height = 30,
                        PureBarcode = true
                    };
                    System.Drawing.Image barcodeImage = barcodeWriter.Write(data);

                    // Adjust the position to fit the label
                    int barcodeX = centerX - (barcodeImage.Width / 2) - 10;
                    int barcodeY = 195; // Adjust the vertical position as needed

                    ev.Graphics.DrawImage(barcodeImage, barcodeX, barcodeY, barcodeImage.Width, barcodeImage.Height);
                    //ev.Graphics.DrawImage(barcodeImage, 145, 185);

                    Font printFont1 = new Font("Times New Roman", 12, System.Drawing.FontStyle.Bold);

                    // Adjust the position to fit the label
                    int textX = centerX - (barcodeImage.Width / 2); // You may need to adjust the offset
                    int textY = 230; // Adjust the vertical position as needed

                    ev.Graphics.DrawString(data, printFont1, br, textX, textY);
                    ev.Graphics.DrawString("تم الفهرسة", printFont1, br, textX + 70, textY);
                }
            }


            if (type == "Box")
            {
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                pd.Print();

                void pd_PrintPage(object sender, PrintPageEventArgs ev)
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);

                    SolidBrush br = new SolidBrush(System.Drawing.Color.Black);
                    SolidBrush darkerBrush = new SolidBrush(System.Drawing.Color.Black); 
                    ev.Graphics.DrawImage(img, 160, 125, 130, 45);


                    // Generate barcode using ZXing.Net
                    BarcodeWriter barcodeWriter = new BarcodeWriter();
                    barcodeWriter.Format = BarcodeFormat.CODE_39;
                    barcodeWriter.Options = new EncodingOptions
                    {
                        Width = 380,
                        Height = 70,
                        PureBarcode = true
                    };     
                    Font printFont1 = new Font("Times New Roman", 16, System.Drawing.FontStyle.Bold);
                    System.Drawing.Image barcodeImage = barcodeWriter.Write(data);
                    ev.Graphics.DrawImage(barcodeImage, 20, 180);

                    ev.Graphics.DrawString(data, printFont1, br, 165, 260);
                    //ev.Graphics.DrawString("E-Bank", printFont1, br, 165, 270);
                }
            }
        }

        private void GenerateTextButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateTextBarcode(InputbarCodeTextBox.Text , (bool)InputBarcodeCheckBox.IsChecked , (bool)InputBarcodeImgCheckBox.IsChecked);
        }
        private void UploadExcelButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                excelFilePathTextBox.Text = filePath;

                // Process the Excel file
                ProcessExcelFile(filePath);
            }
        }

        private void ProcessExcelFile(string filePath)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) 
                {
                    string barcodeValue = worksheet.Cells[row, 1].Value?.ToString();

                    if (!string.IsNullOrEmpty(barcodeValue))
                    {
                        GenerateTextBarcode(barcodeValue, (bool)InputBarcodeCheckBox.IsChecked, (bool)InputBarcodeImgCheckBox.IsChecked); 
                    }
                }
            }
        }

        private void DownloadTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a worksheet to the Excel package
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Template");

                // Add header
                worksheet.Cells["A1"].Value = "Barcode";

                // Set the column width
                worksheet.Column(1).Width = 15;

                // Set up file stream for writing Excel package
                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);

                    // Provide the Excel package as a downloadable file
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = "BarcodeTemplate.xlsx";
                    saveFileDialog.Filter = "Excel Files|*.xlsx";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, stream.ToArray());
                    }
                }
            }
        }

        /*
private void GenerateBarCode(string data, string type)
{
   string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Namaa.jpg");

   if (type == "File")
   {
       PrintDocument pd = new PrintDocument();
       pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
       pd.Print();

       void pd_PrintPage(object sender, PrintPageEventArgs ev)
       {
           Font Threeofnine = new Font("Free 3 of 9 Extended", 31, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
           Font printFont1 = new Font("Times New Roman", 12, System.Drawing.FontStyle.Bold);
           Font Item = new Font("Times New Roman", 16, System.Drawing.FontStyle.Bold);
           System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);

           SolidBrush br = new SolidBrush(System.Drawing.Color.Black);

           ev.Graphics.DrawImage(img, 170, 12, 70, 27);
           ev.Graphics.DrawString("*" + data + "*", Threeofnine, br, 130, 45);
           ev.Graphics.DrawString(data, printFont1, br, 175, 80);
       }
   }

   if (type == "Box")
   {
       PrintDocument pd = new PrintDocument();
       pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
       pd.Print();

       void pd_PrintPage(object sender, PrintPageEventArgs ev)
       {
           Font Threeofnine = new Font("Free 3 of 9 Extended", 60, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
           Font printFont1 = new Font("Times New Roman", 16, System.Drawing.FontStyle.Bold);
           System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);

           SolidBrush br = new SolidBrush(System.Drawing.Color.Black);


           ev.Graphics.DrawImage(img, 160, 125, 130, 45);
           //ev.Graphics.DrawString("Box", Item, br, 220, 135);
           ev.Graphics.DrawString("*" + data + "*", Threeofnine, br, 70, 175);
           ev.Graphics.DrawString(data, printFont1, br, 165, 240);
           ev.Graphics.DrawString("E-Bank", printFont1, br, 165, 270);
       }
   }
}
*/
    }
}


