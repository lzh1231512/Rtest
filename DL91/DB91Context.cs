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

        public DbSet<DBType> DBTypes { get; set; }

        public DbSet<DBCfg> DBCfgs { get; set; }

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
