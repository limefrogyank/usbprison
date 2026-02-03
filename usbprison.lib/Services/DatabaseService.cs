using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace usbprison.lib.Services
{
    public class DatabaseService
    {
        private readonly string _databasePath;
        private readonly SQLiteAsyncConnection _db;

        public DatabaseService(string path) 
        {
            _databasePath = path;
            _db = new SQLiteAsyncConnection(_databasePath);
            
            

            
            
        }
    }
}
