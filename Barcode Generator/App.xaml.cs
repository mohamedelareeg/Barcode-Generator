using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;

namespace Barcode_Generator
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using (var context = new LogEntryContext())
            {
                context.Database.EnsureCreated();
            }
            // Set the license context for OfficeOpenXml
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
      

    }
}
