using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.Common;
using ZXing;
using ZXing.Windows.Compatibility;
using Barcode_Generator.Model;

namespace Barcode_Generator.Helper
{
    public static class PrintHelper
    {
        public static void PrintCanvas(CanvasData canvasData)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler((sender, e) =>
            {
                foreach (SerializableDraggableItem serializableItem in canvasData.DraggableItems)
                {
                    double x = serializableItem.X;
                    double y = serializableItem.Y;

                    if (serializableItem.Type == ElementType.Text)
                    {
                        // Modify this logic to get the appropriate value for the TextBlock
                        string textValue = "Sample Text"; // Replace with actual text value
                        PrintTextBlock(e.Graphics, textValue, x, y);
                    }
                    else if (serializableItem.Type == ElementType.Barcode)
                    {
                        // Modify this logic to get the appropriate value for the barcode
                        string barcodeValue = "1234567890"; // Replace with actual barcode value
                        PrintBarcode(e.Graphics, barcodeValue, x, y);
                    }
                    else if (serializableItem.Type == ElementType.Image)
                    {
                        // Modify this logic to get the appropriate image base64 data and value
                        string base64ImageData = serializableItem.ImageBase64; // Replace with actual base64 data
                        string imageValue = "Image Value"; // Replace with actual image value
                        //PrintBase64ImageAndValue(e.Graphics, base64ImageData, imageValue, x, y);
                    }
                }
            });
            pd.Print();
        }
        public static void PrintTextBlock(Graphics graphics, string text, double x, double y)
        {
            // Customize text formatting as needed
            Font font = new Font("Times New Roman", 12, FontStyle.Regular);
            SolidBrush brush = new SolidBrush(Color.Black);

            graphics.DrawString(text, font, brush, (float)x, (float)y);
        }

        public static void PrintBarcode(Graphics graphics, string barcodeValue, double x, double y)
        {
            // Generate barcode using ZXing.Net
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.CODE_39;
            barcodeWriter.Options = new EncodingOptions
            {
                Width = 100,
                Height = 30,
                PureBarcode = true
            };
            Image barcodeImage = barcodeWriter.Write(barcodeValue);

            // Print barcode image at specified coordinates
            graphics.DrawImage(barcodeImage, (float)x, (float)y);
        }

        public static void PrintBase64ImageAndValue(Graphics graphics, string base64ImageData, string imageValue, double x, double y)
        {
            // Convert base64 image data to Image
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64ImageData)))
            {
                Image image = Image.FromStream(ms);

                // Print image at specified coordinates
                graphics.DrawImage(image, (float)x, (float)y);

                // Customize text formatting as needed
                Font font = new Font("Times New Roman", 12, FontStyle.Bold);
                SolidBrush brush = new SolidBrush(Color.Black);

                // Print image value text below the image
                graphics.DrawString(imageValue, font, brush, (float)x, (float)y + image.Height + 10);
            }
        }

    }
}
