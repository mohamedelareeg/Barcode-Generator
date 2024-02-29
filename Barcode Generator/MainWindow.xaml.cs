using Barcode_Generator.Context;
using Barcode_Generator.Helper;
using Barcode_Generator.Model;
using Barcode_Generator.Windows;
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
using ZXing.QrCode.Internal;
using ZXing.Windows.Compatibility;

namespace Barcode_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Application Settings

        private AppSettings appSettings;

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            //InitializeAppSettings();
        }
        private void InitializeAppSettings()
        {
            appSettings = SettingsHelper.LoadAppSettings();
            // Check device identifier and license expiration
            if (string.IsNullOrEmpty(appSettings.DeviceIdentifier))
            {
                var serialNumberDialog = new SerialNumberDialog();
                if (serialNumberDialog.ShowDialog() == true)
                {
                    string enteredSerialNumber = serialNumberDialog.SerialNumber;


                    if (enteredSerialNumber == "PY7CV-F8ZSF-FV5YT-F8ZHC-FY8TW")
                    {
                        appSettings.MaxBarcodePrints = 1000;
                        appSettings.DeviceIdentifier = SettingsHelper.GenerateDeviceIdentifier();
                        appSettings.LicenseExpirationDate = DateTime.Now.AddYears(1);
                        SettingsHelper.SaveAppSettings(appSettings);
                    }
                    else
                    {
                        MessageBox.Show("Invalid serial number. Please enter a valid serial number to use the application.", "Serial Number Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            appSettings = SettingsHelper.LoadAppSettings();
            string generatedDeviceIdentifier = SettingsHelper.GenerateDeviceIdentifier();

            if (appSettings.DeviceIdentifier == generatedDeviceIdentifier)
            {
                DateTime currentDate = DateTime.Now;

                if (currentDate < appSettings.LicenseExpirationDate)
                {
                   
                }
                else
                {
                  
                    MessageBox.Show("Your license has expired. Please renew your license to continue using the application.", "License Expiration", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0); 
                }
            }
            else
            {

                MessageBox.Show("The application cannot be run on this device. Please contact support for assistance.", "Device Identifier Mismatch", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }
        private void btn_Generator(object sender, RoutedEventArgs e)
        {
            BarcodeGenerator barcodeGeneratorWindow = new BarcodeGenerator();
            barcodeGeneratorWindow.ShowDialog();
        }

        private void btn_Designer(object sender, RoutedEventArgs e)
        {
            BarcodeDesigner barcodeDesignerWindow = new BarcodeDesigner();
            barcodeDesignerWindow.ShowDialog();
        }

        private void btn_Saved(object sender, RoutedEventArgs e)
        {
            CanvasListWindow savedDesignsWindow = new CanvasListWindow();
            savedDesignsWindow.ShowDialog();
        }

        private void btn_Settings(object sender, RoutedEventArgs e)
        {
        }
    }
}


