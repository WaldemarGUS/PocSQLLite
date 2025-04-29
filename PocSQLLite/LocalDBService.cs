using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PocSQLLite
{
    public class LocalDBService
    {
        private const string DB_NAME = "demo_local_db.db3";
        private readonly SQLiteAsyncConnection _connection;
        private readonly SyncService _syncService;

        public LocalDBService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
            _connection = new SQLiteAsyncConnection(dbPath);
            _syncService = new SyncService(dbPath);
            _connection.CreateTableAsync<Customer>();
        }

        public async Task<List<Customer>> getcustomers()
        {
            return await _connection.Table<Customer>().ToListAsync();
        }

        public async Task<Customer> GetById(int id)
        {
            return await _connection.Table<Customer>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task Create(Customer customer)
        {
            await _connection.InsertAsync(customer);
            // Sync the changes
            await _syncService.SyncData(new List<Customer> { customer }, "Customer");
        }

        public async Task Update(Customer customer)
        {
            await _connection.UpdateAsync(customer);
            // Sync the changes
            await _syncService.SyncData(new List<Customer> { customer }, "Customer");
        }

        public async Task Delete(Customer customer)
        {
            await _connection.DeleteAsync(customer);
        }
    }
}

