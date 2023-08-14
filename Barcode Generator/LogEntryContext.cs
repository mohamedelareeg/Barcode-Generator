using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barcode_Generator
{
    public class LogEntryContext : DbContext
    {
        public DbSet<MaxnumOfFileAndBox> maxnumOfFileAndBoxes { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Get the folder path
            string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");

            // Create the folder if it does not exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Set up the SQLite connection
            optionsBuilder.UseSqlite($"Data Source={folderPath}\\log.db"); ;
        }

    }
}
