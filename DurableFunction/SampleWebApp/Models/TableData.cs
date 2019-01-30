using Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleWebApp.Models
{
    public class TableData
    {
        private CloudStorageAccount _storageAccount;

        public TableData(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount;
        }

        public async Task<IEnumerable<UserEntity>> GetUserInfo()
        {
            var tableClient =_storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("mytableitems");
            var query = new TableQuery<UserEntity>();
            return await table.ExecuteQuerySegmentedAsync(query, null);
        }
    }
}
