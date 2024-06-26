﻿using Microsoft.EntityFrameworkCore;
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

        private string connectionString;
        public DB91Context()
        {
        }

        public DB91Context(string connectionString) : base()
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (connectionString == null)
            {
                connectionString = "Data Source=" + ConfigurationHelper.dbPath;
            }
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
