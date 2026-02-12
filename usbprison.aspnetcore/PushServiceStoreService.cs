using SQLite;
using usbprison.aspnetcore.Model;

namespace usbprison.aspnetcore
{
    public class PushServiceStoreService
    {
        private readonly SQLiteAsyncConnection _db;
        public PushServiceStoreService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<PushSubscription>().Wait();
        }

        public Task StoreSubscriptionAsync(PushSubscription subscription)
        {
            return _db.InsertOrReplaceAsync(subscription);
        }

        public Task DiscardSubscriptionAsync(string endpoint)
        {
            return _db.DeleteAsync<PushSubscription>(endpoint);
        }
    }
}
