using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Synchronization.Services
{
    /// <summary>
    /// Simple implementation of SyncBlob class for Microsoft Sync Framework
    /// </summary>
    [DataContract]
    public class SyncBlob
    {
        [DataMember]
        public string ClientId { get; set; }

        [DataMember]
        public string ClientScopeName { get; set; }

        [DataMember]
        public string ClientScopeId { get; set; }

        [DataMember]
        public string ClientLastSyncTimestamp { get; set; }

        public SyncBlob()
        {
        }

        public static SyncBlob DeSerialize(byte[] blob)
        {
            if (blob == null || blob.Length == 0)
                return new SyncBlob();

            try
            {
                string jsonString = System.Text.Encoding.UTF8.GetString(blob);
                return JsonConvert.DeserializeObject<SyncBlob>(jsonString);
            }
            catch
            {
                return new SyncBlob();
            }
        }

        public byte[] Serialize()
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(this);
                return System.Text.Encoding.UTF8.GetBytes(jsonString);
            }
            catch
            {
                return new byte[0];
            }
        }
    }
} 