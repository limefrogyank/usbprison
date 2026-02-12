using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace usbprison.lib.Services
{
    public class DatabaseService
    {
        private readonly string _databasePath;
        private readonly SQLiteAsyncConnection _db;

        public SQLiteAsyncConnection DB => _db;

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
                await _db.CreateTableAsync<TrackedDeviceModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
            }

            //Observable.Timer(DateTimeOffset.Now + TimeSpan.FromSeconds(20), TimeSpan.FromDays(1)).Subscribe(async x =>
            //{
            //    var weekAgo = DateTime.Now - TimeSpan.FromDays(7);
            //    // delete records older than 1 week
            //    await _db.Table<PrisonLog>().Where(x => x.Timestamp < weekAgo).DeleteAsync();
            //});


        }


        public async Task<int> AddLogsAsync(IEnumerable<PrisonLog> logs)
        {
            // instead of adding a log, let's check if it would change anything and only add it if there's a new status (per device && per machine [future])
            int results = 0;
            foreach (var log in logs)
            {
                if (log == null) continue;
                // should be a single log for each device coming from the MonitoringService
                var mostRecent = await _db.Table<PrisonLog>().Where(x => x.DeviceId == log.DeviceId).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync();
                if (mostRecent == null || mostRecent.Status != log.Status)
                {
                    results += await _db.InsertAsync(log);
                }
            }
            return results;
        }

        public async Task<IObservable<PrisonLog>> GetLogsAsync(DateTime since)
        {
            ReplaySubject<PrisonLog> replaySubject = new ReplaySubject<PrisonLog>();
            var list = await _db.Table<PrisonLog>().Where(x => x.Timestamp >= since).OrderBy(x => x.Timestamp).ToListAsync();
            foreach (var item in list)
                replaySubject.OnNext(item);

            return replaySubject.AsObservable();
        }

        public async Task<IObservable<PrisonLog>> GetLogsForTrackedDeviceAsync(string deviceId, DateTime since)
        {
            var list = await _db.Table<PrisonLog>().Where(x => x.DeviceId == deviceId && x.Timestamp >= since).OrderBy(x => x.Timestamp).ToListAsync();

            ReplaySubject<PrisonLog> replaySubject = new ReplaySubject<PrisonLog>();
            foreach (var item in list)
                replaySubject.OnNext(item);
            replaySubject.OnCompleted();

            return replaySubject.AsObservable();
        }

        public async Task<Dictionary<string, IObservable<PrisonLog>>> GetLogsByTrackedDeviceAsync(DateTime since)
        {
            var dic = new Dictionary<string, IObservable<PrisonLog>>();
            var list = await _db.Table<PrisonLog>().Where(x => x.Timestamp >= since).OrderBy(x => x.Timestamp).ToListAsync();
            var devices = list.Select(x => x.DeviceId).Distinct().ToList();
            foreach (var device in devices)
            {
                var deviceSpecific = list.Where(x => x.DeviceId == device);
                ReplaySubject<PrisonLog> replaySubject = new ReplaySubject<PrisonLog>();
                foreach (var item in deviceSpecific)
                    replaySubject.OnNext(item);
                replaySubject.OnCompleted();
                dic.Add(device, replaySubject.AsObservable());
            }


            return dic;
        }

    }
}
