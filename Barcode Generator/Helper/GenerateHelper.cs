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

namespace Barcode_Generator.Helper
{
    public static class GenerateHelper
    {
        public static void GenerateTextBarcode(string data, bool showBarcode, bool isShowImg)
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


        public static void GenerateBarCode(string data, string type)
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
    }
}
