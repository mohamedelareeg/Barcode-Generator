using Microsoft.Win32;
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
using System.Windows.Shapes;
using ZXing.QrCode;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Drawing;
using System.Windows.Interop;
using static System.Net.Mime.MediaTypeNames;

namespace Barcode_Generator
{
    /// <summary>
    /// Interaction logic for BarcodeDesigner.xaml
    /// </summary>
    public partial class BarcodeDesigner : Window
    {
        private bool isDragging = false;
        private ListBoxItem draggedItem;
        private System.Windows.Point startDragPoint;
        private string selectedImageTag;

        public BarcodeDesigner()
        {
            InitializeComponent();
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

        private void designCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string elementType = e.Data.GetData(DataFormats.StringFormat) as string;

                System.Windows.Point dropPosition = e.GetPosition(designCanvas);

                // If the data is not a string, try to get it from the Tag property of the dragged ListBoxItem
                if (elementType == null && e.Data.GetDataPresent(DataFormats.Text))
                {
                    elementType = e.Data.GetData(DataFormats.Text) as string;
                }

                if (!string.IsNullOrEmpty(elementType))
                {
                    switch (elementType)
                    {
                        case "Text":
                            CreateTextBlock("Sample Text", dropPosition);
                            break;
                        case "Barcode":
                            CreateBarcodeImage("1234567890", dropPosition);
                            break;
                        case "Image":
                            OpenAndAddImage(dropPosition);
                            break;
                    }
                   
                }
            }
        }


        private void CreateTextBlock(string text, System.Windows.Point position)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);
            designCanvas.Children.Add(textBlock);
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
            designCanvas.Children.Add(barcodeImage);
        }

        private void OpenAndAddImage(System.Windows.Point position)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                Canvas.SetLeft(image, position.X);
                Canvas.SetTop(image, position.Y);
                designCanvas.Children.Add(image);
            }
        }


        private void DesignCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is UIElement element && element != designCanvas)
            {
                isDragging = true;
                startDragPoint = e.GetPosition(designCanvas);
                draggedItem = FindAncestor<ListBoxItem>(element); 

                // Capture the mouse to the canvas
                designCanvas.CaptureMouse();

                e.Handled = true;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                    return ancestor;

                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);

            return null;
        }


        private void DesignCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedItem != null) 
            {
                System.Windows.Point newPoint = e.GetPosition(designCanvas);
                double deltaX = newPoint.X - startDragPoint.X;
                double deltaY = newPoint.Y - startDragPoint.Y;

                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // Resize the dragged item
                    if (draggedItem.Content is TextBlock textBlock)
                    {
                        double newWidth = textBlock.ActualWidth + deltaX;
                        double newHeight = textBlock.ActualHeight + deltaY;
                        textBlock.Width = newWidth;
                        textBlock.Height = newHeight;
                    }
                }
                else
                {
                    // Move the dragged item
                    Canvas.SetLeft(draggedItem, Canvas.GetLeft(draggedItem) + deltaX);
                    Canvas.SetTop(draggedItem, Canvas.GetTop(draggedItem) + deltaY);
                    startDragPoint = newPoint;
                }
            }
        }

        private void DesignCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && draggedItem != null)
            {
                // Release the mouse capture from the canvas
                designCanvas.ReleaseMouseCapture();

                isDragging = false;
                draggedItem = null;

                e.Handled = true;
            }
        }


        private void designCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            // Mark the event as handled
            e.Handled = true;
        }
        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggedItem = sender as ListBoxItem;
            startDragPoint = e.GetPosition(null);
        }

        private void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedItem != null && selectedImageTag != null)
            {
                // Calculate the distance from the start point
                Vector diff = startDragPoint - e.GetPosition(null);

                // Check if the dragged item is not null and if the mouse has moved enough
                if (e.LeftButton == MouseButtonState.Pressed && draggedItem.Content != null &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    // Start drag-and-drop with the stored selectedImageTag
                    DataObject data = new DataObject(DataFormats.StringFormat, selectedImageTag);
                    DragDrop.DoDragDrop(draggedItem, data, DragDropEffects.Copy);
                }
            }
        }
        private void ToolboxListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolboxListBox.SelectedItem is ListBoxItem selectedItem)
            {
                if (selectedItem.Content is System.Windows.Controls.Image selectedImage && selectedImage.Tag != null)
                {
                    selectedImageTag = selectedImage.Tag.ToString(); 
                }
            }
        }


     
    }
 


}
