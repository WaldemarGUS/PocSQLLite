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
            System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
            _sqlite = new SQLiteOffline(dbPath, new Uri("http://localhost"));
            _syncProvider = new SQLiteOfflineSyncProvider(_sqlite, new Uri("http://localhost"));
        }

        public async Task<List<Hp>> GetHps()
        {
            return await _sqlite.LoadAll<Hp>();
        }

        public async Task<Hp> GetById(System.Guid id)
        {
            return await _sqlite.LoadObjectByOID<Hp>(id);
        }

        public async Task Create(Hp hp)
        {
            await _sqlite.StoreObject(hp);
            _syncProvider.BeginSession();
            try
            {
                await _syncProvider.GetChangeSet(Guid.NewGuid());
            }
            finally
            {
                _syncProvider.EndSession();
            }
        }

        public async Task Update(Hp hp)
        {
            await _sqlite.StoreObject(hp);
            _syncProvider.BeginSession();
            try
            {
                await _syncProvider.GetChangeSet(Guid.NewGuid());
            }
            finally
            {
                _syncProvider.EndSession();
            }
        }

        public async Task Delete(Hp hp)
        {
            await _sqlite.Delete(hp);
            _syncProvider.BeginSession();
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

