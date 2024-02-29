using Barcode_Generator.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barcode_Generator.Context
{
    public class LogEntryContext : DbContext
    {
        #region DbSets
        public DbSet<MaxnumOfFileAndBox> maxnumOfFileAndBoxes { get; set; }
        #endregion

        #region DbContext Configuration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            optionsBuilder.UseSqlite($"Data Source={folderPath}\\log.db");
        }
        #endregion
    }
}
