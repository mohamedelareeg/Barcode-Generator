
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
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

namespace Barcode_Generator
{
    /// <summary>
    /// Interaction logic for DefaultSettingsWindow.xaml
    /// </summary>
    public partial class SerialNumberDialog : Window
    {
        public string SerialNumber { get; private set; }

        public bool IsDialog { get; set; }
        public SerialNumberDialog()
        {
            InitializeComponent();
            IsDialog = false;

        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate and save the entered values
            if (!string.IsNullOrWhiteSpace(txtSerialNumber.Text))
            {
                SerialNumber = txtSerialNumber.Text;
                DialogResult = true;
                Close();

            }
            else
            {
                MessageBox.Show("Please enter valid value Serial Number.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsDialog)
            {
                SerialNumber = null;
                DialogResult = false;
            }
            Close();
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                GetWindow(this).DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            GetWindow(this).Close();
        }
    }
}
