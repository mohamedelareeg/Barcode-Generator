using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.Common;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Windows;
using System.Security.RightsManagement;
using System.Printing;

namespace Barcode_Generator.Helper
{
    public static class GenerateHelper
    {
        private static string GetDefaultPrinterName()
        {
            LocalPrintServer localPrintServer = new LocalPrintServer();
            PrintQueue defaultPrintQueue = localPrintServer.DefaultPrintQueue;
            return defaultPrintQueue.Name;
        }

        public static void GenerateTextBarcode(string barcodeData, bool showBarcode, bool isShowImg)
        {

            // Load your image
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpg");
            // Create a PrintDocument for printing
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);

            // Get the current default printer name
            string currentPrinterName = GetDefaultPrinterName();

            // Get the selected printer settings
            PrinterSettings settings = new PrinterSettings();
            settings.PrinterName = currentPrinterName;
            /*
            // Set the PageSettings with a specific paper size and margins
            PageSettings pageSettings = new PageSettings();
            pageSettings.PaperSize = new PaperSize("Custom", 500, 250); 
            pageSettings.Margins = new Margins(10, 10, 10, 10); 
            pd.DefaultPageSettings = pageSettings;
            */
            // Set the printer settings
            pd.PrinterSettings.PrinterName = settings.PrinterName;
            pd.DefaultPageSettings.PrinterSettings = pd.PrinterSettings;
            pd.Print();
            void pd_PrintPage(object sender, PrintPageEventArgs ev)
            {
                PrinterSettings settings = ev.PageSettings.PrinterSettings;

                Graphics g = ev.Graphics;
                Rectangle printableArea = new Rectangle(
                    0,
                    0,
                    (int)settings.DefaultPageSettings.PrintableArea.Width,
                    (int)settings.DefaultPageSettings.PrintableArea.Height
                );
                float imageHeightPercentage = 0.09f;
                float barcodeHeightPercentage = 0.12f; 
                float textHeightPercentage = 0.01f; 

                int imageHeight = (int)(printableArea.Height * imageHeightPercentage);
                int barcodeHeight = (int)(printableArea.Height * barcodeHeightPercentage);
                int textHeight = (int)(printableArea.Height * textHeightPercentage);

                int totalHeight = imageHeight + barcodeHeight + textHeight;
                int topMargin = (int)(printableArea.Height * 0.07);

                int imageWidth = (int)((int)settings.DefaultPageSettings.PrintableArea.Width * 0.3); // Adjust the percentage as needed
                int imageLeft = (printableArea.Width - imageWidth) / 2;
                
                if (isShowImg)
                {
                    Image image = Image.FromFile(imagePath);
                    g.DrawImage(image, new Rectangle(
                        imageLeft,
                        topMargin,
                        imageWidth,
                        imageHeight
                    ));
                    topMargin += imageHeight + 10;
                }

                // Draw the barcode in the middle
                if (showBarcode)
                {
                    BarcodeWriter barcodeWriter = new BarcodeWriter();
                    barcodeWriter.Format = BarcodeFormat.CODE_128;
                    barcodeWriter.Options = new EncodingOptions
                    {
                        PureBarcode = true
                    };
                    Bitmap barcodeBitmap = barcodeWriter.Write(barcodeData);
                    int barcodeWidth = (int)((int)settings.DefaultPageSettings.PrintableArea.Width * 0.5); // Adjust the percentage as needed
                    int barcodeLeft = (printableArea.Width - barcodeWidth) / 2;
                    g.DrawImage(barcodeBitmap, new Rectangle(
                        barcodeLeft,
                        topMargin,
                        barcodeWidth,
                        barcodeHeight
                    ));
                    topMargin += barcodeHeight;
                }

                // Draw the text at the bottom
                if (!string.IsNullOrEmpty(barcodeData))
                {
                    Font font = new Font("Arial", 12);
                    string text = barcodeData;
                    SizeF textSize = g.MeasureString(text, font);
                    int textLeft = (printableArea.Width - (int)textSize.Width) / 2;
                    g.DrawString(text, font, Brushes.Black, new PointF(
                        textLeft,
                        topMargin
                    ));
                }
            }




        }
        public static void HoneyWell_5_2_5_portrait(string data, string type)//HoneyWell File/Box
        {
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpg");
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
                barcodeWriter.Format = BarcodeFormat.CODE_128;
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
               // ev.Graphics.DrawString("تم الفهرسة", printFont1, br, textX + 70, textY);
            }

        }
        public static void HoneyWell_5_10_landscape(string data, string type)//HoneyWell File/Box
        {
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpg");
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
                barcodeWriter.Format = BarcodeFormat.CODE_128;
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

        public static void GenerateBarCode(string data, string type)//HoneyWell File/Box
        {
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpg");

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
                    barcodeWriter.Format = BarcodeFormat.CODE_128;
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
                    ev.Graphics.DrawString("Info", printFont1, br, textX + 70, textY);
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
                    barcodeWriter.Format = BarcodeFormat.CODE_128;
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

        public static void Zebra_5_5_Landscape(string data, string type )//ForZebrea 5*5 portrait
        {
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpg");

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            pd.Print();
            // Load the image
            void pd_PrintPage(object sender, PrintPageEventArgs ev)
            {
                // Get printer settings and page settings
                PrinterSettings settings = ev.PageSettings.PrinterSettings;
                PageSettings pageSettings = ev.PageSettings;
                float printableAreaWidth = pageSettings.Bounds.Width - pageSettings.Margins.Left - pageSettings.Margins.Right;
                float printableAreaHeight = pageSettings.Bounds.Height - pageSettings.Margins.Top - pageSettings.Margins.Bottom;

                // Calculate center coordinates within the printable area
                float centerX = pageSettings.Margins.Left + printableAreaWidth / 2;
                float centerY = pageSettings.Margins.Top + printableAreaHeight / 2;

                // Load the image
                System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);

                // Calculate the image's width to fit the printable area
                float aspectRatioImage = (float)img.Width / img.Height;
                float imageWidth = printableAreaWidth * 0.5f; // Adjust as needed
                float imageHeight = imageWidth / aspectRatioImage * 0.8f;

                // Calculate the image's position from the right side
                float imageX = pageSettings.Bounds.Width - pageSettings.Margins.Right - imageWidth;
                float imageY = pageSettings.Margins.Top;

                // Calculate the image's corner positions
                PointF imageTopLeft = new PointF(imageX, imageY);
                PointF imageTopRight = new PointF(imageX + imageWidth, imageY);
                PointF imageBottomLeft = new PointF(imageX, imageY + imageHeight);
                PointF imageBottomRight = new PointF(imageX + imageWidth, imageY + imageHeight);

                // Generate barcode using ZXing.Net
                BarcodeWriter barcodeWriter = new BarcodeWriter();
                barcodeWriter.Format = BarcodeFormat.CODE_128;
                barcodeWriter.Options = new EncodingOptions
                {
                    Width = (int)(printableAreaWidth * 0.6f), // Adjust as needed
                    Height = (int)(printableAreaHeight * 0.2f), // Adjust as needed
                    Margin = 40, // No margin
                    PureBarcode = true,

                };
                System.Drawing.Image barcodeImage = barcodeWriter.Write(data);

                // Calculate the barcode's width to fit the printable area
                float aspectRatioBarcode = (float)barcodeImage.Width / barcodeImage.Height;
                float barcodeWidth = printableAreaWidth; // Adjust as needed
                float barcodeHeight = barcodeWidth / aspectRatioBarcode;

                // Calculate the barcode's position to center it
                float barcodeX = centerX - barcodeWidth / 3;
                float barcodeY = centerY - barcodeHeight / 3;

                // Calculate the barcode's corner positions
                PointF barcodeTopLeft = new PointF(barcodeX, barcodeY);
                PointF barcodeTopRight = new PointF(barcodeX + barcodeWidth, barcodeY);
                PointF barcodeBottomLeft = new PointF(barcodeX, barcodeY + barcodeHeight);
                PointF barcodeBottomRight = new PointF(barcodeX + barcodeWidth, barcodeY + barcodeHeight);

                // Define font for text
                Font printFont1 = new Font("Times New Roman", 12, System.Drawing.FontStyle.Bold);

                // Calculate text size
                SizeF textSize = ev.Graphics.MeasureString(data, printFont1);

                // Calculate text position from the right side
                float textX = pageSettings.Bounds.Width - pageSettings.Margins.Right - textSize.Width;
                float textY = centerY + barcodeHeight / 2;

                // Calculate text's corner positions
                PointF textTopLeft = new PointF(textX, textY);
                PointF textTopRight = new PointF(textX + textSize.Width, textY);
                PointF textBottomLeft = new PointF(textX, textY + textSize.Height);
                PointF textBottomRight = new PointF(textX + textSize.Width, textY + textSize.Height);

                // All content fits within the printable area

                // Draw the image, barcode, and text
                ev.Graphics.DrawImage(img, imageX + 70, imageY + 50, imageWidth, imageHeight);
                ev.Graphics.DrawImage(barcodeImage, barcodeX + 100 , barcodeY - 10, barcodeWidth-20, barcodeHeight);
                ev.Graphics.DrawString(data, printFont1, Brushes.Black, textX + 25, textY);
            }





        }
    
        public static void Zebra_5_2_5_Portrait(string data, string type)
        {
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpg");

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            pd.Print();

            void pd_PrintPage(object sender, PrintPageEventArgs ev)
            {
                // Center the image horizontally
                int centerX = (ev.MarginBounds.Left + ev.MarginBounds.Right) / 2;
                System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);

                SolidBrush br = new SolidBrush(System.Drawing.Color.Black);

                ev.Graphics.DrawImage(img, 150, 10, 110, 30);

                // Generate barcode using ZXing.Net
                BarcodeWriter barcodeWriter = new BarcodeWriter();
                barcodeWriter.Format = BarcodeFormat.CODE_128;
                barcodeWriter.Options = new EncodingOptions
                {
                    Width = 100,
                    Height = 25,
                    PureBarcode = true
                };
                System.Drawing.Image barcodeImage = barcodeWriter.Write(data);

                // Adjust the position to fit the label
                int barcodeX = centerX - (barcodeImage.Width / 2) - 10;
                int barcodeY = 50; // Adjust the vertical position as needed

                ev.Graphics.DrawImage(barcodeImage, barcodeX, barcodeY, barcodeImage.Width, barcodeImage.Height);
                //ev.Graphics.DrawImage(barcodeImage, 145, 185);

                Font printFont1 = new Font("Times New Roman", 10, System.Drawing.FontStyle.Bold);

                // Adjust the position to fit the label
                int textX = centerX - (barcodeImage.Width / 2); // You may need to adjust the offset
                int textY = 80; // Adjust the vertical position as needed

                ev.Graphics.DrawString(data, printFont1, br, textX+25, textY );
            }
        }
    
    }

}
