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

            _ = InitializeAsync();
            
        }

        private async Task InitializeAsync()
        {
            try
            {
                await _db.CreateTableAsync<PrisonLog>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
            }
            
            
        }

        public Task<int> AddLogAsync(PrisonLog log)
        {
            return _db.InsertAsync(log);
        }

        public Task<List<PrisonLog>> GetLogsAsync()
        {
            return _db.Table<PrisonLog>().ToListAsync();
        }
    }
}
