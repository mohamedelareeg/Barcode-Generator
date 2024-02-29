using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Drawing.Printing;
using System.Runtime.Serialization;
using System.Drawing;
using System.Windows.Interop;
using ZXing.Common;
using ZXing;
using ZXing.Windows.Compatibility;
using DocumentFormat.OpenXml.Presentation;
using Barcode_Generator.Helper;
using Barcode_Generator.Model;

namespace Barcode_Generator
{
    public partial class CanvasListWindow : Window
    {
        #region Fields

        private string canvasFolderPath;
        private List<CanvasData> canvasDataList;

        #endregion
        #region Constructor

        public CanvasListWindow()
        {
            InitializeComponent();
            canvasFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exports");
            if (!Directory.Exists(canvasFolderPath))
            {
                Directory.CreateDirectory(canvasFolderPath);
            }
            LoadCanvasDataList();
            DisplayCanvasDataList();
        }

        #endregion
        #region Methods

        private void LoadCanvasDataList()
        {
            canvasDataList = new List<CanvasData>();
            if (Directory.Exists(canvasFolderPath))
            {
                string[] files = Directory.GetFiles(canvasFolderPath, "*.canvas");
                foreach (string file in files)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file); // Get the file name
                        IFormatter formatter = new BinaryFormatter();
                        using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            CanvasData canvasData = (CanvasData)formatter.Deserialize(stream);
                            canvasData.FileName = fileName; // Store the file name in CanvasData
                            canvasDataList.Add(canvasData);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading canvas data from {file}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DisplayCanvasDataList()
        {
            canvasListBox.ItemsSource = canvasDataList;
        }

        private int ConvertInchesToPixels(double inches)
        {
            double dpi = 96.0; // Typical screen DPI
            return (int)(inches * dpi);
        }

        private void CreateTextBlock(string text, System.Windows.Point position)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);
            canvasPreview.Children.Add(textBlock);
        }
        private void CreateBarcodeImage(string content, System.Windows.Point position)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.CODE_39;
            barcodeWriter.Options = new EncodingOptions
            {
                Width = 100,
                Height = 30,
                PureBarcode = true
            };

            // Generate the barcode image
            Bitmap barcodeBitmap = (Bitmap)barcodeWriter.Write(content);

            // Convert the Bitmap to a BitmapSource for WPF
            BitmapSource barcodeBitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                barcodeBitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Create a WPF Image control to display the barcode
            System.Windows.Controls.Image barcodeImage = new System.Windows.Controls.Image();
            barcodeImage.Source = barcodeBitmapSource;
            Canvas.SetLeft(barcodeImage, position.X);
            Canvas.SetTop(barcodeImage, position.Y);
            canvasPreview.Children.Add(barcodeImage);

        }
        private void CreateImageFromBase64(string imageBase64, System.Windows.Point position)
        {
            byte[] imageData = Convert.FromBase64String(imageBase64);
            BitmapImage bitmapImage = new BitmapImage();

            using (MemoryStream memoryStream = new MemoryStream(imageData))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
            }

            System.Windows.Controls.Image image = new System.Windows.Controls.Image
            {
                Source = bitmapImage
            };

            Canvas.SetLeft(image, position.X);
            Canvas.SetTop(image, position.Y);
            canvasPreview.Children.Add(image);


        }
        #endregion
        #region Event Handlers
        private void CanvasListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canvasListBox.SelectedItem is CanvasData selectedCanvasData)
            {
                int canvasWidth = ConvertInchesToPixels(5);
                int canvasHeight = ConvertInchesToPixels(2.5);
                canvasPreview.Width = canvasWidth;
                canvasPreview.Height = canvasHeight;
                canvasPreview.Children.Clear();
                foreach (SerializableDraggableItem serializableItem in selectedCanvasData.DraggableItems)
                {
                    if (serializableItem.Type == ElementType.Text)
                    {
                        CreateTextBlock("Sample Text", new System.Windows.Point(serializableItem.X, serializableItem.Y));
                    }
                    else if (serializableItem.Type == ElementType.Barcode)
                    {
                        CreateBarcodeImage("1234567890", new System.Windows.Point(serializableItem.X, serializableItem.Y));
                    }
                    else if (serializableItem.Type == ElementType.Image)
                    {
                        //CreateImageFromBase64(serializableItem.ImageBase64, new System.Windows.Point(serializableItem.X, serializableItem.Y));
                    }
                }
            }
        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (canvasListBox.SelectedItem is CanvasData selectedCanvasData)
            {
                PrintHelper.PrintCanvas(selectedCanvasData);
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
        #endregion
    }
}
