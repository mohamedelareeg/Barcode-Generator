using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Barcode_Generator.Model
{
    [Serializable]
    public class SerializableDraggableItem
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ElementType Type { get; set; }
        public string ImageBase64 { get; set; }


        public SerializableDraggableItem(DraggableItem item)
        {
            X = Canvas.GetLeft(item.Element);
            Y = Canvas.GetTop(item.Element);
            Width = item.Width;
            Height = item.Height;
            Type = item.Type;
            if (item.Type == ElementType.Image)
            {
                Image image = item.Element as Image;
                if (image != null && image.Source is BitmapImage bitmapImage)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                        encoder.Save(memoryStream);
                        ImageBase64 = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }
    }

    [Serializable]
    public class CanvasData
    {
        public double CanvasWidth { get; set; }
        public double CanvasHeight { get; set; }
        public List<SerializableDraggableItem> DraggableItems { get; set; }
    }

    public enum ElementType
    {
        Text,
        Barcode,
        Image
    }
    public class DraggableItem
    {
        public UIElement Element { get; set; }
        public ElementType Type { get; set; }
        private double width;
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                if (Element is FrameworkElement frameworkElement)
                    frameworkElement.Width = width;
            }
        }

        private double height;
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                if (Element is FrameworkElement frameworkElement)
                    frameworkElement.Height = height;
            }
        }
    }
}
