using System.Collections.Generic;
using System.Threading.Tasks;
using SiaqodbSyncProvider;

namespace PocSQLLite
{
    public class LocalDBService
    {
        private const string DB_NAME = "demo_local_db.db3";
        private readonly SQLiteOffline _sqlite;
        private readonly SQLiteOfflineSyncProvider _syncProvider;

        public LocalDBService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
            _sqlite = new SQLiteOffline(dbPath);
            _syncProvider = new SQLiteOfflineSyncProvider(_sqlite, new Uri("http://localhost"));
        }

        public async Task<List<SiaqodbSyncProvider.Customer>> getcustomers()
        {
            return await _sqlite.LoadAll<SiaqodbSyncProvider.Customer>();
        }

        public async Task<SiaqodbSyncProvider.Customer> GetById(int id)
        {
            return await _sqlite.LoadObjectByOID<SiaqodbSyncProvider.Customer>(id);
        }

        public async Task Create(SiaqodbSyncProvider.Customer customer)
        {
            customer.Id = 0; // Ensure new record
            await _sqlite.StoreObject(customer);
            await _syncProvider.BeginSession();
            try
            {
                await _syncProvider.GetChangeSet(Guid.NewGuid());
            }
            finally
            {
                _syncProvider.EndSession();
            }
        }

        public async Task Update(SiaqodbSyncProvider.Customer customer)
        {
            if (customer.Id == 0)
            {
                throw new ArgumentException("Cannot update a customer with Id 0");
            }
            await _sqlite.StoreObject(customer);
            await _syncProvider.BeginSession();
            try
            {
                await _syncProvider.GetChangeSet(Guid.NewGuid());
            }
            finally
            {
                _syncProvider.EndSession();
            }
        }

        public async Task Delete(SiaqodbSyncProvider.Customer customer)
        {
            await _sqlite.DeleteObject(customer);
            await _syncProvider.BeginSession();
            try
            {
                await _syncProvider.GetChangeSet(Guid.NewGuid());
            }
            finally
            {
                _syncProvider.EndSession();
            }
        }
    }
}

