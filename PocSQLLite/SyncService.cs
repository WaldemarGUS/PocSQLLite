using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Synchronization.Services;

namespace PocSQLLite
{
    public class SyncService
    {
        private readonly SQLiteAsyncConnection _localDb;
        private readonly string _syncTableName = "SyncMetadata";

        public SyncService(string dbPath)
        {
            _localDb = new SQLiteAsyncConnection(dbPath);
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            await _localDb.CreateTableAsync<SyncMetadata>();
        }

        public async Task SyncData<T>(List<T> items, string tableName) where T : new()
        {
            try
            {
                // Get sync blob
                var syncBlob = await GetSyncBlob(tableName);
                
                // Create or update items
                foreach (var item in items)
                {
                    await _localDb.InsertOrReplaceAsync(item);
                }

                // Update sync metadata with new timestamp
                await UpdateSyncMetadata(tableName, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync error: {ex.Message}");
                throw;
            }
        }

        private async Task<SyncBlob> GetSyncBlob(string tableName)
        {
            var metadata = await _localDb.Table<SyncMetadata>()
                .Where(x => x.TableName == tableName)
                .FirstOrDefaultAsync();

            if (metadata == null)
                return new SyncBlob();

            return SyncBlob.DeSerialize(metadata.SyncData);
        }

        private async Task UpdateSyncMetadata(string tableName, DateTime syncTime)
        {
            var syncBlob = new SyncBlob
            {
                ClientId = "LocalClient",
                ClientScopeName = tableName,
                ClientLastSyncTimestamp = syncTime.ToString("o")
            };

            var metadata = new SyncMetadata
            {
                TableName = tableName,
                LastSyncTime = syncTime,
                SyncData = syncBlob.Serialize()
            };

            await _localDb.InsertOrReplaceAsync(metadata);
        }
    }

    public class SyncMetadata
    {
        [PrimaryKey]
        public string TableName { get; set; }
        public DateTime LastSyncTime { get; set; }
        public byte[] SyncData { get; set; }
    }
} 