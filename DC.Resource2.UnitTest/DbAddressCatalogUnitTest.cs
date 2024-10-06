using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DC.Resource2;
using System.Linq;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;

namespace DC.Resource2.UnitTest
{
    [TestClass]
    public class DbAddressCatalogUnitTest
    {
        private const string _dbPath = "./resource.db";
        private readonly string _dbConnString = $"Data Source={_dbPath}";
        [TestInitialize]
        public void Init()
        {
            //if (File.Exists(_dbPath)) { File.Delete(_dbPath); }
            var migration = new ResouceDbMigration(null, _dbConnString);
            migration.Migrate();
        }

        [TestCleanup]
        public void Cleanup()
        {
            var repository = new AddressCatalogDbRepository(_dbConnString);
            repository.Clear();
        }

        [TestMethod]
        public void InsertTest()
        {
            var repository = new AddressCatalogDbRepository(_dbConnString);
            var expected = new AddressRecord { Address = "d300", AxisId = 1, FuncCode = "abc", IOType = IOType.Rsts, IsEnable = true, MechanismId = 1 };
            var id = repository.Add(expected);
            var list = repository.List();
            var target = list.Where(r => r.Id == id).FirstOrDefault();
            Assert.IsNotNull(target);
            expected.Id = id;
            Assert.AreEqual(expected, target);
        }

        [TestMethod]
        public void UpdateTest()
        {
            var repository = new AddressCatalogDbRepository(_dbConnString);
            var expected = new AddressRecord { Address = "d301", AxisId = 10, FuncCode = "xxxx", IOType = IOType.Axis, IsEnable = true, MechanismId = 1 };
            var id = repository.Add(expected);
            expected.Id = id;
            expected.Address = "d302";
            expected.AxisId = 11;
            expected.FuncCode = "yyyy";
            expected.IOType = IOType.CheckDone;
            expected.IsEnable = false;
            expected.MechanismId = 2;
            repository.Update(expected);
            var list = repository.List();
            var target = list.Where(r => r.Id == id).FirstOrDefault();
            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target);
        }

        [TestMethod]
        public void GetTest()
        {
            var repository = new AddressCatalogDbRepository(_dbConnString);
            var expected = new AddressRecord { Address = "d301", AxisId = 10, FuncCode = "xxxx", IOType = IOType.Axis, IsEnable = true, MechanismId = 1 };
            var id = repository.Add(expected);
            expected.Id = id;
            var target = repository.Get(id);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target);
        }

        [TestMethod]
        public void DeleteTest()
        {
            var repository = new AddressCatalogDbRepository(_dbConnString);
            var list = repository.List();

            var expected = new AddressRecord { Address = "d301", AxisId = 10, FuncCode = "xxxx", IOType = IOType.Axis, IsEnable = true, MechanismId = 1 };
            var id = repository.Add(expected);

            repository.Delete(id);
            list = repository.List();
            Assert.IsNull(list.Where(r => r.Id == id).FirstOrDefault());
        }

        [TestMethod]
        public void ClearTest()
        {
            var repository = new AddressCatalogDbRepository(_dbConnString);

            var expected = new AddressRecord { Address = "d301", AxisId = 10, FuncCode = "xxxx", IOType = IOType.Axis, IsEnable = true, MechanismId = 1 };
            repository.Add(expected);
            var expected1 = new AddressRecord { Address = "d300", AxisId = 1, FuncCode = "abc", IOType = IOType.Rsts, IsEnable = true, MechanismId = 1 };
            repository.Add(expected1);
            repository.Clear();
            var list = repository.List();
            Assert.AreEqual(0, list.Count);
        }
    }
}
