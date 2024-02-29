using Barcode_Generator.Context;
using Barcode_Generator.Helper;
using Barcode_Generator.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
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
using System.Windows.Shapes;

namespace Barcode_Generator.Windows
{

    public partial class BarcodeGenerator : Window
    {
        private string lastIndexofAsset = "";
        private string lastIndexofBox = "";
        private string lastIndexofFile = "";
        private string lastIndexofDocument = "";
        private LogEntryContext _context;
        private AppSettings appSettings;
        public BarcodeGenerator()
        {
            _context = new LogEntryContext();
            InitializeComponent();
            appSettings = SettingsHelper.LoadAppSettings();
          
            PopulatePrinterComboBox();
            UpdateLastIndexes();
        }
        private void PopulatePrinterComboBox()
        {
            var printerComboBox = FindName("printerComboBox") as ComboBox;

            if (printerComboBox != null)
            {
                var installedPrinters = GetInstalledBarcodePrinters();
                foreach (var printerName in installedPrinters)
                {
                    printerComboBox.Items.Add(printerName);
                }
            }
        }
        public List<string> GetInstalledBarcodePrinters()
        {
            List<string> printerNames = new List<string>();

            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;

            foreach (string printerName in printers)
            {
                if (IsBarcodePrinter(printerName))
                {
                    printerNames.Add(printerName);
                }
            }

            return printerNames;
        }
        private bool IsBarcodePrinter(string printerName)
        {
            if (printerName.Contains("Zebra", StringComparison.OrdinalIgnoreCase) ||
                printerName.Contains("Honeywell", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
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
            lastIndexofAsset = "0";
            lastIndexofDocument = "0";
            var MaxNumof = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
            if (MaxNumof != null)
            {

                lastIndexofBox = MaxNumof.MaxNumOfBox;
                lastIndexofFile = MaxNumof.MaxNumOfFile;
                lastIndexofDocument = MaxNumof.MaxNumOfDocument;
                lastIndexofAsset = MaxNumof.MaxNumOfAsset;
            }
            lastIndexedAssetText.Text = $"* Last Asset: {lastIndexofAsset}";
            lastIndexedBoxText.Text = $"* Last Box: {lastIndexofBox}";
            lastIndexedFileText.Text = $"* Last File: {lastIndexofFile}";
            lastIndexedDocumentText.Text = $"* Last Document: {lastIndexofDocument}";
            RemainCount.Text = $"* Remain: {appSettings.MaxBarcodePrints - appSettings.UsedBarcodePrints}";
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {

            if (appSettings.UsedBarcodePrints < appSettings.MaxBarcodePrints)
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
                string selectedPrinter = printerComboBox.SelectedValue as string;
                barcodeErrorText.Text = "";
                GenerateBarCodeCount(barcode, type, selectedPrinter);
                appSettings.UsedBarcodePrints = appSettings.UsedBarcodePrints + Int32.Parse(barcode);//Count
                SettingsHelper.SaveAppSettings(appSettings);
                appSettings = SettingsHelper.LoadAppSettings();
                RemainCount.Text = $"* Remain: {appSettings.MaxBarcodePrints - appSettings.UsedBarcodePrints}";
            }
            else
            {
                MessageBox.Show("You have reached the maximum allowed barcode prints.");
            }
        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (appSettings.UsedBarcodePrints < appSettings.MaxBarcodePrints)
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

                string selectedPrinter = printerComboBox.SelectedValue as string;
                //GenerateHelper.GenerateBarCode(i.ToString().PadLeft(6, '0'), type);
                switch (selectedPrinter)
                {
                    case "Honeywell PC42t plus (203 dpi)":
                        HandleHoneywellPrinterFromTo(start, end, type);
                        break;
                    case "Zebra ZD220 (203 dpi) - ZPL":
                        HandleZebraPrinterFromTo(start, end, type);
                        break;
                    default:

                        break;
                }

                int count = endIndex - startIndex;
                appSettings.UsedBarcodePrints = appSettings.UsedBarcodePrints + count;//Count
                SettingsHelper.SaveAppSettings(appSettings);
                appSettings = SettingsHelper.LoadAppSettings();
                RemainCount.Text = $"* Remain: {appSettings.MaxBarcodePrints - appSettings.UsedBarcodePrints}";
            }
            else
            {
                MessageBox.Show("You have reached the maximum allowed barcode prints.");
            }
        }

        private async void GenerateBarCodeCount(string Count, string Type, string Printer)
        {

            //Honeywell PC42t plus (203 dpi)
            //Zebra ZD220 (203 dpi) - ZPL

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
                maxnumOfFileAndBox.MaxNumOfAsset = "000000";
                maxnumOfFileAndBox.MaxNumOfDocument = "00000000";
                _context.maxnumOfFileAndBoxes.Add(maxnumOfFileAndBox);
                _context.SaveChanges();
            }
            switch (Printer)
            {
                case "Honeywell PC42t plus (203 dpi)":
                    HandleHoneywellPrinter(Count, Type);
                    break;
                case "Zebra ZD220 (203 dpi) - ZPL":
                    HandleZebraPrinter(Count, Type);
                    break;
                default:

                    break;
            }

        }
        private async void HandleHoneywellPrinter(string Count, string Type)
        {
            if (Type == "Asset")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfAsset, out int intmaxofbox);
                int.TryParse(Count, out int plus);
                int end = intmaxofbox + plus;
                for (int i = intmaxofbox + 1; i <= end; i++)
                {
                    string current = i.ToString().PadLeft(6, '0');
                    Max.MaxNumOfAsset = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.Zebra_5_5_Landscape(current, Type);//TODO
                    // Update the last indexed box
                    lastIndexofAsset = current;
                    lastIndexedAssetText.Text = $"* Last Asset: {lastIndexofAsset}";
                }
                await _context.SaveChangesAsync();
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
                    GenerateHelper.HoneyWell_5_10_landscape(current, Type);
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
                    GenerateHelper.HoneyWell_5_2_5_portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofFile = current;
                    lastIndexedFileText.Text = $"* Last File: {lastIndexofFile}";
                }
                await _context.SaveChangesAsync();
            }
            if (Type == "Document")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfDocument, out int intmaxoffile);
                int.TryParse(Count, out int plus);
                int end = intmaxoffile + plus;
                for (int i = intmaxoffile + 1; i <= end; i++)
                {
                    string current = i.ToString().PadLeft(9, '0');
                    Max.MaxNumOfDocument = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.HoneyWell_5_2_5_portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofDocument = current;
                    lastIndexedDocumentText.Text = $"* Last Document: {lastIndexofDocument}";
                }
                await _context.SaveChangesAsync();
            }
        }

        private async void HandleZebraPrinter(string Count, string Type)
        {
            if (Type == "Asset")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfAsset, out int intmaxofbox);
                int.TryParse(Count, out int plus);
                int end = intmaxofbox + plus;
                for (int i = intmaxofbox + 1; i <= end; i++)
                {
                    string current = i.ToString().PadLeft(6, '0');
                    Max.MaxNumOfAsset = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.Zebra_5_5_Landscape(current, Type);
                    // Update the last indexed box
                    lastIndexofAsset = current;
                    lastIndexedAssetText.Text = $"* Last Asset: {lastIndexofAsset}";
                }
                await _context.SaveChangesAsync();
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
                    GenerateHelper.HoneyWell_5_10_landscape(current, Type);//TODO
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
                    GenerateHelper.Zebra_5_2_5_Portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofFile = current;
                    lastIndexedFileText.Text = $"* Last File: {lastIndexofFile}";
                }
                await _context.SaveChangesAsync();
            }
            if (Type == "Document")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfDocument, out int intmaxoffile);
                int.TryParse(Count, out int plus);
                int end = intmaxoffile + plus;
                for (int i = intmaxoffile + 1; i <= end; i++)
                {
                    string current = i.ToString().PadLeft(9, '0');
                    Max.MaxNumOfDocument = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.Zebra_5_2_5_Portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofDocument = current;
                    lastIndexedDocumentText.Text = $"* Last Document: {lastIndexofDocument}";
                }
                await _context.SaveChangesAsync();
            }
        }
        private async void HandleHoneywellPrinterFromTo(string start, string to, string Type)
        {
            if (!int.TryParse(start, out int startIndex) || !int.TryParse(to, out int endIndex) || startIndex > endIndex)
            {
                barcodeErrorText.Text = "Invalid range. Please enter valid start and end indexes.";
                return;
            }
            if (Type == "Asset")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfAsset, out int intmaxofbox);

                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(6, '0');
                    Max.MaxNumOfAsset = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.Zebra_5_5_Landscape(current, Type);//TODO
                    // Update the last indexed box
                    lastIndexofAsset = current;
                    lastIndexedAssetText.Text = $"* Last Asset: {lastIndexofAsset}";
                }
                await _context.SaveChangesAsync();
            }
            if (Type == "Box")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfBox, out int intmaxofbox);

                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(6, '0');
                    Max.MaxNumOfBox = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.HoneyWell_5_10_landscape(current, Type);
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

                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(7, '0');
                    Max.MaxNumOfFile = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.HoneyWell_5_2_5_portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofFile = current;
                    lastIndexedFileText.Text = $"* Last File: {lastIndexofFile}";
                }
                await _context.SaveChangesAsync();
            }
            if (Type == "Document")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfDocument, out int intmaxoffile);
                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(9, '0');
                    Max.MaxNumOfDocument = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.HoneyWell_5_2_5_portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofDocument = current;
                    lastIndexedDocumentText.Text = $"* Last Document: {lastIndexofDocument}";
                }
                await _context.SaveChangesAsync();
            }
        }

        private async void HandleZebraPrinterFromTo(string start, string to, string Type)
        {
            if (!int.TryParse(start, out int startIndex) || !int.TryParse(to, out int endIndex) || startIndex > endIndex)
            {
                barcodeErrorText.Text = "Invalid range. Please enter valid start and end indexes.";
                return;
            }
            if (Type == "Asset")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfAsset, out int intmaxofbox);

                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(6, '0');
                    Max.MaxNumOfAsset = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.Zebra_5_5_Landscape(current, Type);
                    // Update the last indexed box
                    lastIndexofAsset = current;
                    lastIndexedAssetText.Text = $"* Last Asset: {lastIndexofAsset}";
                }
                await _context.SaveChangesAsync();
            }
            if (Type == "Box")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfBox, out int intmaxofbox);

                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(6, '0');
                    Max.MaxNumOfBox = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.HoneyWell_5_10_landscape(current, Type);//TODO
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

                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(7, '0');
                    Max.MaxNumOfFile = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.Zebra_5_2_5_Portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofFile = current;
                    lastIndexedFileText.Text = $"* Last File: {lastIndexofFile}";
                }
                await _context.SaveChangesAsync();
            }
            if (Type == "Document")
            {
                var Max = await _context.maxnumOfFileAndBoxes.FirstOrDefaultAsync(m => m.Id == 1);
                int.TryParse(Max.MaxNumOfDocument, out int intmaxoffile);
                for (int i = startIndex; i <= endIndex; i++)
                {
                    string current = i.ToString().PadLeft(9, '0');
                    Max.MaxNumOfDocument = current;
                    _context.maxnumOfFileAndBoxes.Update(Max);
                    GenerateHelper.Zebra_5_2_5_Portrait(current, Type);
                    // Update the last indexed file
                    lastIndexofDocument = current;
                    lastIndexedDocumentText.Text = $"* Last Document: {lastIndexofDocument}";
                }
                await _context.SaveChangesAsync();
            }
        }

        private void GenerateTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (appSettings.UsedBarcodePrints < appSettings.MaxBarcodePrints)
            {
                GenerateHelper.GenerateTextBarcode(InputbarCodeTextBox.Text, (bool)InputBarcodeCheckBox.IsChecked, (bool)InputBarcodeImgCheckBox.IsChecked);
                appSettings.UsedBarcodePrints = appSettings.UsedBarcodePrints + 1;//Count
                SettingsHelper.SaveAppSettings(appSettings);
                appSettings = SettingsHelper.LoadAppSettings();
                RemainCount.Text = $"* Remain: {appSettings.MaxBarcodePrints - appSettings.UsedBarcodePrints}";
            }
            else
            {
                MessageBox.Show("You have reached the maximum allowed barcode prints.");
            }
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
                if (appSettings.UsedBarcodePrints < appSettings.MaxBarcodePrints)
                {
                    for (int row = 2; row <= rowCount; row++)
                    {
                        string barcodeValue = worksheet.Cells[row, 1].Value?.ToString();

                        if (!string.IsNullOrEmpty(barcodeValue))
                        {
                            GenerateHelper.GenerateTextBarcode(barcodeValue, (bool)InputBarcodeCheckBox.IsChecked, (bool)InputBarcodeImgCheckBox.IsChecked);
                        }
                    }
                    appSettings.UsedBarcodePrints = appSettings.UsedBarcodePrints + rowCount;//Count
                    SettingsHelper.SaveAppSettings(appSettings);
                    appSettings = SettingsHelper.LoadAppSettings();
                    RemainCount.Text = $"* Remain: {appSettings.MaxBarcodePrints - appSettings.UsedBarcodePrints}";
                }
                else
                {
                    MessageBox.Show("You have reached the maximum allowed barcode prints.");
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


    }
}
