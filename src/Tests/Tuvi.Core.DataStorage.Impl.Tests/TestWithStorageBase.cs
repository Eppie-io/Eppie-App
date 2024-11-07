using System;
using System.IO;
using System.Threading.Tasks;
using Tuvi.Core.DataStorage.Impl;

namespace Tuvi.Core.DataStorage.Tests
{
    public class TestWithStorageBase
    {
        protected const string Password = "123456";
        protected const string IncorrectPassword = "65432211";
        protected const string NewPassword = "987654321";

        protected IDataStorage GetDataStorage()
        {
            return DataStorageProvider.GetDataStorage(DataBasePath);
        }

        protected async Task<IDataStorage> OpenDataStorageAsync()
        {
            var db = GetDataStorage();
            await db.OpenAsync(Password).ConfigureAwait(false);
            return db;
        }

        protected async Task<IDataStorage> CreateDataStorageAsync()
        {
            var db = GetDataStorage();
            await db.CreateAsync(Password).ConfigureAwait(false);
            return db;
        }

        protected bool DatabaseFileExists()
        {
            return File.Exists(DataBasePath);
        }

        protected void DeleteStorage()
        {
            if (DatabaseFileExists())
            {
                File.Delete(DataBasePath);
            }
        }
 
        private const string DataBaseFileName = "TestTuviMail.db";
        private readonly string DataBasePath = Path.Combine(Environment.CurrentDirectory, DataBaseFileName);
    }
}
