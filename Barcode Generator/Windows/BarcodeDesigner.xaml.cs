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
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Drawing;
using System.Windows.Interop;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Barcode_Generator.Helper;
using Barcode_Generator.Model;

namespace Barcode_Generator
{
    public partial class BarcodeDesigner : Window
    {
        #region Dragging Variables

        private bool isDragging = false;
        private ListBoxItem draggedItem;
        private DraggableItem draggedElement;
        private System.Windows.Point startDragPoint;
        private string selectedImageTag;
        private List<DraggableItem> draggedItems = new List<DraggableItem>();

        #endregion

        public BarcodeDesigner()
        {
            InitializeComponent();
       
            InitializeCanvasSize();
        }
        #region Initialization



        private void InitializeCanvasSize()
        {
            int canvasWidth = ConvertInchesToPixels(5);
            int canvasHeight = ConvertInchesToPixels(2.5);
            designCanvas.Width = canvasWidth;
            designCanvas.Height = canvasHeight;
        }

        #endregion
        #region Event Handlers

        private void LoadCanvas_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Canvas Data Files (*.canvas)|*.canvas|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                LoadCanvasFromFile(openFileDialog.FileName);
            }
        }

        private void SaveCanvas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Canvas Data Files (*.canvas)|*.canvas|All Files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveCanvasToFile(saveFileDialog.FileName);
            }
        }
        private void LoadCanvasFromFile(string fileName)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                CanvasData canvasData = (CanvasData)formatter.Deserialize(stream);

                // Update canvas size
                designCanvas.Width = canvasData.CanvasWidth;
                designCanvas.Height = canvasData.CanvasHeight;

                // Clear existing items
                designCanvas.Children.Clear();
                draggedItems.Clear();

                // Create and add items from loaded data
                foreach (SerializableDraggableItem serializableItem in canvasData.DraggableItems)
                {
                    DraggableItem draggableItem = new DraggableItem
                    {
                        Type = serializableItem.Type,
                        Width = serializableItem.Width,
                        Height = serializableItem.Height
                    };

                    if (draggableItem.Type == ElementType.Text)
                    {
                        CreateTextBlock("Sample Text", new System.Windows.Point(serializableItem.X, serializableItem.Y));
                    }
                    else if (draggableItem.Type == ElementType.Barcode)
                    {
                        CreateBarcodeImage("1234567890", new System.Windows.Point(serializableItem.X, serializableItem.Y));
                    }
                    else if (draggableItem.Type == ElementType.Image)
                    {
                        CreateImageFromBase64(serializableItem.ImageBase64, new System.Windows.Point(serializableItem.X, serializableItem.Y));
                    }

                    // Store the draggable item in the list
                    draggedItems.Add(draggableItem);
                }
            }
        }

        private void SaveCanvasToFile(string fileName)
        {
            // Create a list to store serializable versions of draggable items
            List<SerializableDraggableItem> serializableItems = draggedItems.Select(item => new SerializableDraggableItem(item)).ToList();

            // Create a canvas data object
            CanvasData canvasData = new CanvasData
            {
                CanvasWidth = designCanvas.Width,
                CanvasHeight = designCanvas.Height,
                DraggableItems = serializableItems
            };

            // Serialize and save the canvas data to a file
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, canvasData);
            }
        }
        #endregion
        #region Mouse Events
        private int ConvertInchesToPixels(double inches)
        {
            double dpi = 96.0; // Typical screen DPI
            return (int)(inches * dpi);
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
        #endregion
        #region Element Creation
        private void CreateTextBlock(string text, System.Windows.Point position)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);
            designCanvas.Children.Add(textBlock);
            CreateDraggableItem(textBlock, ElementType.Text, position);
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
            CreateDraggableItem(barcodeImage, ElementType.Barcode, position);
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
            designCanvas.Children.Add(image);

            CreateDraggableItem(image, ElementType.Image, position);
        }
        private void OpenAndAddImage(System.Windows.Point position)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));

                // Set the image size to 200x300
                image.Width = 200;
                image.Height = 300;

                image.Source = bitmapImage;
                Canvas.SetLeft(image, position.X);
                Canvas.SetTop(image, position.Y);
                designCanvas.Children.Add(image);
                CreateDraggableItem(image, ElementType.Image, position);
            }
        }

        private void CreateDraggableItem(UIElement element, ElementType type, System.Windows.Point position)
        {
            /*
            // Position the element on the canvas
            Canvas.SetLeft(element, position.X);
            Canvas.SetTop(element, position.Y);
            designCanvas.Children.Add(element);
            */
          
            double initialWidth = (element as FrameworkElement)?.Width ?? 0;
            double initialHeight = (element as FrameworkElement)?.Height ?? 0;

            // Create a draggable item and store it in the list
            DraggableItem draggableItem = new DraggableItem { 
                Element = element,
                Type = type,
                Width = initialWidth,
                Height = initialHeight
            };
            draggedItems.Add(draggableItem);

            // Add event handlers for moving and selecting the item
            element.MouseDown += DraggableItem_MouseDown;
            element.MouseMove += DraggableItem_MouseMove;
            element.MouseUp += DraggableItem_MouseUp;
            element.PreviewMouseLeftButtonDown += DraggableItem_PreviewMouseLeftButtonDown;
            element.PreviewMouseMove += DesignCanvas_PreviewMouseMove;
        }
        #endregion
        #region Draggable Item Events
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
        private void DesignCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                // Calculate the distance from the start point
                Vector diff = startDragPoint - e.GetPosition(designCanvas);

                // Check if the mouse has moved enough to start dragging
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DataObject data = new DataObject(typeof(DraggableItem), draggedElement);
                    DragDrop.DoDragDrop(designCanvas, data, DragDropEffects.Move);

                    e.Handled = true;
                }
            }
        }
        private void DraggableItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = true;
                startDragPoint = e.GetPosition(null);
               
               
                draggedElement = draggedItems.FirstOrDefault(item => item.Element == sender as UIElement);
                e.Handled = true;
            }
        }

        private void DraggableItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startDragPoint = e.GetPosition(null);
                     e.Handled = true;
        }

        private void DraggableItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Windows.Point currentPosition = e.GetPosition(null);
                double deltaX = currentPosition.X - startDragPoint.X;
                double deltaY = currentPosition.Y - startDragPoint.Y;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // Update the position of the dragged item
                    var draggableItem = draggedItems.FirstOrDefault(item => item.Element == sender as UIElement);
                    if (draggableItem != null)
                    {
                        Canvas.SetLeft(draggableItem.Element, Canvas.GetLeft(draggableItem.Element) + deltaX);
                        Canvas.SetTop(draggableItem.Element, Canvas.GetTop(draggableItem.Element) + deltaY);
                    }
                    ModifySelectedItemProperties(draggableItem);
                    startDragPoint = currentPosition;
                }
            }
        }

        private void DraggableItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            draggedElement = null;
        }
        #endregion
        #region Element Deletion

        private void DeleteSelectedItem()
        {
            if (draggedElement != null)
            {
                designCanvas.Children.Remove(draggedElement.Element);
                draggedItems.RemoveAll(item => item.Element == draggedElement.Element);
                draggedElement = null;
            }
        }

        #endregion
        #region Element Modification

        private void ModifySelectedItemProperties(DraggableItem draggableItem)
        {
            if (draggableItem != null)
            {
                if (draggableItem.Type == ElementType.Text)
                {
                    // Modify TextBlock properties
                    AddPropertyControls(draggableItem);
                }
                else if (draggableItem.Type == ElementType.Image)
                {
                    // Modify Image properties
                    AddPropertyControls(draggableItem);
                }
                else if (draggableItem.Type == ElementType.Barcode)
                {
                    // Modify Barcode properties
                    AddPropertyControls(draggableItem);
                }
            }
        }

        #endregion
        #region Property Controls
        private void AddPropertyControls(DraggableItem draggableItem)
        {
            propertiesStackPanel.Children.Clear();

            if (draggableItem != null)
            {
                if (draggableItem.Type == ElementType.Barcode || draggableItem.Type == ElementType.Image)
                {
                    propertiesStackPanel.Children.Add(CreateWidthHeightControl(draggableItem));
                }
                else if (draggableItem.Type == ElementType.Text)
                {
                    propertiesStackPanel.Children.Add(CreateTextPropertiesControl(draggableItem));
                }
            }
        }
        private FrameworkElement CreateTextPropertiesControl(DraggableItem draggableItem)
        {
            TextBlock textLabel = new TextBlock();
            textLabel.Text = "Text Properties";
            textLabel.Margin = new Thickness(0, 10, 0, 5);
            textLabel.FontWeight = FontWeights.Bold;

            StackPanel controlPanel = new StackPanel();
            controlPanel.Margin = new Thickness(5);

            // Text size control
            TextBlock textSizeLabel = new TextBlock();
            textSizeLabel.Text = "Text Size:";
            TextBox textSizeTextBox = new TextBox();
            textSizeTextBox.Text = ((draggableItem.Element as TextBlock)?.FontSize ?? 12).ToString();
            textSizeTextBox.TextChanged += (sender, e) =>
            {
                if (double.TryParse(textSizeTextBox.Text, out double newTextSize))
                {
                    if (draggableItem.Element is TextBlock textBlock)
                    {
                        textBlock.FontSize = newTextSize;
                    }
                }
            };

            // Bold control
            CheckBox boldCheckBox = new CheckBox();
            boldCheckBox.Content = "Bold";
            boldCheckBox.IsChecked = (draggableItem.Element as TextBlock)?.FontWeight == FontWeights.Bold;
            boldCheckBox.Checked += (sender, e) =>
            {
                if (draggableItem.Element is TextBlock textBlock)
                {
                    textBlock.FontWeight = FontWeights.Bold;
                }
            };
            boldCheckBox.Unchecked += (sender, e) =>
            {
                if (draggableItem.Element is TextBlock textBlock && textBlock.FontWeight == FontWeights.Bold)
                {
                    textBlock.FontWeight = FontWeights.Normal;
                }
            };

            controlPanel.Children.Add(textLabel);
            controlPanel.Children.Add(textSizeLabel);
            controlPanel.Children.Add(textSizeTextBox);
            controlPanel.Children.Add(boldCheckBox);

            return controlPanel;
        }
        private FrameworkElement CreateWidthHeightControl(DraggableItem draggableItem)
        {
            double initialWidth = draggableItem.Element.DesiredSize.Width;
            double initialHeight = draggableItem.Element.DesiredSize.Height;

            StackPanel controlPanel = new StackPanel();
            controlPanel.Margin = new Thickness(5);

            // Width control
            TextBlock widthLabel = new TextBlock();
            widthLabel.Text = "Width:";
            TextBox widthTextBox = new TextBox();
            widthTextBox.Text = initialWidth.ToString();
            widthTextBox.TextChanged += (sender, e) =>
            {
                if (double.TryParse(widthTextBox.Text, out double newWidth))
                {
                    draggableItem.Width = newWidth;
                    if (draggableItem.Element != null)
                    {
                        Canvas.SetLeft(draggableItem.Element, Canvas.GetLeft(draggableItem.Element) - (newWidth - initialWidth) / 2);
                        draggableItem.Width = newWidth;
                    }
                }
            };

            // Height control
            TextBlock heightLabel = new TextBlock();
            heightLabel.Text = "Height:";
            TextBox heightTextBox = new TextBox();
            heightTextBox.Text = initialHeight.ToString();
            heightTextBox.TextChanged += (sender, e) =>
            {
                if (double.TryParse(heightTextBox.Text, out double newHeight))
                {
                    draggableItem.Height = newHeight;
                    if (draggableItem.Element != null)
                    {
                        Canvas.SetTop(draggableItem.Element, Canvas.GetTop(draggableItem.Element) - (newHeight - initialHeight) / 2);
                        draggableItem.Height = newHeight;
                    }
                }
            };

            controlPanel.Children.Add(widthLabel);
            controlPanel.Children.Add(widthTextBox);
            controlPanel.Children.Add(heightLabel);
            controlPanel.Children.Add(heightTextBox);

            return controlPanel;
        }

        #endregion
        #region Design Canvas Events
        private void DesignCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                System.Windows.Point currentPosition = e.GetPosition(designCanvas);
                double deltaX = currentPosition.X - startDragPoint.X;
                double deltaY = currentPosition.Y - startDragPoint.Y;
                /*
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // Update the position of the dragged item
                    Canvas.SetLeft(draggedItem, Canvas.GetLeft(draggedItem) + deltaX);
                    Canvas.SetTop(draggedItem, Canvas.GetTop(draggedItem) + deltaY);

                    startDragPoint = currentPosition;
                }
                */
            }
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

        #endregion
        #region DragEnter Event
        private void designCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string elementType = e.Data.GetData(DataFormats.Text) as string;
                if (Enum.TryParse(elementType, out ElementType type))
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            // Mark the event as handled
            e.Handled = true;
        }
        #endregion
        #region ListBox Item Events
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
        #endregion
        #region Toolbox ListBox Event

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
        #endregion
    }
}
