using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace DL91
{
    public class DB91Context : DbContext
    {
        public DbSet<DB91> DB91s { get; set; }

        public DbSet<DBCfg> DBCfgs { get; set; }

        public int getMaxID()
        {
            if (!DBCfgs.Any())
            {
                DBCfgs.Add(new DBCfg() { id = 1, maxID = 4 });
                SaveChanges();
            }
            return DBCfgs.First().maxID;
        }

        public void setMaxID(int newID)
        {
            getMaxID();
            DBCfgs.First().maxID = newID;
            SaveChanges();
        }

        private IConfiguration configuration;

        public DB91Context()
        {
            configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(configuration.GetConnectionString("db"));
        }
    }
}
