using Tuvi.Core.DataStorage;
using Tuvi.Core.DataStorage.Impl;
using System;
using System.IO;
using TuviPgpLib;
using System.Threading.Tasks;

namespace SecurityManagementTests
{
    public class TestWithStorageBase
    {
        protected const string Password = "123456";
        protected const string IncorrectPassword = "65432211";
        protected const string NewPassword = "987654321";

        protected IPasswordProtectedStorage GetPasswordProtectedStorage()
        {
            return GetDataStorage();
        }

        protected IKeyStorage GetKeyStorage()
        {
            return GetDataStorage();
        }

        protected IDataStorage GetDataStorage()
        {
            return DataStorageProvider.GetDataStorage(DataBasePath);
        }

        protected void DeleteStorage()
        {
            if (File.Exists(DataBasePath))
            {
                File.Delete(DataBasePath);
            }
        }

        protected Task CreateStorageAsync()
        {
            return GetDataStorage().CreateAsync(Password);
        }

        private const string DataBaseFileName = "TestTuviMail.db";
        private readonly string DataBasePath = Path.Combine(Environment.CurrentDirectory, DataBaseFileName);
    }
}
