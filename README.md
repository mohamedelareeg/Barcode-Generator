# Barcode Generator with Barcode Designer

This is a comprehensive barcode generation application built using C# and WPF. It combines the functionality of generating barcodes for assets, boxes, files, and documents with a powerful Barcode Designer feature for creating custom barcode labels with ease.

## Features

- **Barcode Generation:** Generate barcodes for various purposes including assets, boxes, files, and documents.
- **Barcode Designer:** Design custom barcode labels with intuitive tools and functionalities.
- **Printer Compatibility:** Supports popular barcode printers such as Honeywell and Zebra.
- **Flexible Printing Options:** Print single barcodes, ranges, or directly from Excel files for convenience.
- **Usage Tracking:** Keep track of used barcode prints for better inventory management.

## Getting Started

To start using the Barcode Generator with Barcode Designer:

1. **Clone the Repository:** Clone the repository to your local machine.
2. **Open in Visual Studio:** Open the solution in Visual Studio.
3. **Build and Run:** Build the solution and run the application.
4. **Configure Settings:** Configure settings such as maximum barcode prints and installed printers.
5. **Explore Barcode Designer:** Explore the Barcode Designer feature to create custom barcode labels.
6. **Generate Barcodes:** Start generating barcodes for your assets, boxes, files, or documents.

## Dependencies

- **Entity Framework Core:** Entity Framework Core is used for database interactions and management.
- **EPPlus:** EPPlus library is utilized for Excel file handling and manipulation.

## Usage

- **Select Barcode Type:** Choose the type of barcode to generate based on your requirements.
- **Enter Barcode Information:** Enter the necessary information for the barcode generation process.
- **Customize with Barcode Designer:** Utilize the Barcode Designer feature to customize barcode labels with text, images, and more.
- **Print Barcodes:** Select the printer and print the generated barcodes directly or export them to Excel for batch printing.

## Sample Code

```csharp
private void CreateBarcodeImage(string content, System.Windows.Point position)
{
    // Create barcode writer with required format and options
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
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Special thanks to [ZXing.Net](https://github.com/micjahn/ZXing.Net) for providing robust barcode generation support.
