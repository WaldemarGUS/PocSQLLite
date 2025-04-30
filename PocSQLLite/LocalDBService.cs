using System.Collections.Generic;
using System.Threading.Tasks;
using SiaqodbSyncProvider;
using System.Diagnostics;

namespace PocSQLLite
{
    public class LocalDBService
    {
        private const string DB_NAME = "demo_local_db.db3";
        private readonly SQLiteOffline _sqlite;

        public LocalDBService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
            Debug.WriteLine($"Database path: {dbPath}");
            _sqlite = new SQLiteOffline(dbPath, new Uri("http://localhost"));
        }

        public async Task<List<Hp>> GetHps()
        {
            Debug.WriteLine("Getting all Hps...");
            var result = await _sqlite.LoadAll<Hp>();
            Debug.WriteLine($"Retrieved {result?.Count ?? 0} Hps");
            return result;
        }

        public async Task<Hp> GetById(System.Guid id)
        {
            Debug.WriteLine($"Getting Hp by ID: {id}");
            var result = await _sqlite.LoadObjectByOID<Hp>(id);
            Debug.WriteLine(result != null ? "Hp found" : "Hp not found");
            return result;
        }

        public async Task Create(Hp hp)
        {
            Debug.WriteLine($"Creating new Hp: {hp.HpNr}");
            await _sqlite.StoreObject(hp);
            Debug.WriteLine("Hp stored locally");
        }

        public async Task Update(Hp hp)
        {
            Debug.WriteLine($"Updating Hp: {hp.HpNr}");
            await _sqlite.StoreObject(hp);
            Debug.WriteLine("Hp updated locally");
        }

        public async Task Delete(Hp hp)
        {
            Debug.WriteLine($"Deleting Hp: {hp.HpNr}");
            await _sqlite.Delete(hp);
            Debug.WriteLine("Hp deleted locally");
        }
    }
}

