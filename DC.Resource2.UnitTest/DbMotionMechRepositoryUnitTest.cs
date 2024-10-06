using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[assembly: DoNotParallelize]

namespace DC.Resource2.UnitTest
{
    [TestClass]
    [TestCategory("dbMotionCRUD")]
    public class DbMotionMechRepositoryUnitTest
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
            var repository = new EquipmentMotionMechanismDbRepository(_dbConnString);
            repository.Clear();
        }

        [TestMethod]
        public void InsertTest()
        {
            var repository = new EquipmentMotionMechanismDbRepository(_dbConnString);
            var expected = new MotionMechanism
            {
                Code = "01",
                IpAddress = "127.0.0.1",
                MechanismType = MechanismType.Plc,
                Oem = OEM.PlcMelsec,
                Protocol = Protocol.ModbusTcp,
                Port = 234,
                Series = "abc",
            };
            var id = repository.Add(expected);
            expected.Id = id;
            var list = repository.List();
            var target = list.Where(mm => mm.Id == id).FirstOrDefault();
            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target);
        }

        [TestMethod]
        public void GetTest()
        {
            var repository = new EquipmentMotionMechanismDbRepository(_dbConnString);
            repository.Clear();
            var expected = new MotionMechanism
            {
                Code = "01",
                IpAddress = "127.0.0.1",
                MechanismType = MechanismType.Plc,
                Oem = OEM.PlcMelsec,
                Protocol = Protocol.ModbusTcp,
                Port = 234,
                Series = "abc",
            };
            var id = repository.Add(expected);
            expected.Id = id;
            var target = repository.Get(id);
            Assert.AreEqual(expected, target);
        }

        [TestMethod]
        public void GetReturnNull()
        {
            var repository = new EquipmentMotionMechanismDbRepository(_dbConnString);
            repository.Clear();
            var target = repository.Get(1);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void UpdateTest()
        {
            var repository = new EquipmentMotionMechanismDbRepository(_dbConnString);
            var expected = new MotionMechanism
            {
                Code = "01",
                IpAddress = "127.0.0.1",
                MechanismType = MechanismType.Plc,
                Oem = OEM.PlcMelsec,
                Protocol = Protocol.ModbusTcp,
                Port = 234,
                Series = "abc",
            };
            var id = repository.Add(expected);
            expected.Id = id;

            expected.Code = "02";
            expected.IpAddress = "127.0.0.2";
            expected.MechanismType = MechanismType.EmbeddedBoard;
            expected.Oem = OEM.PlcXinJe;
            expected.Protocol = Protocol.Fins;
            expected.Port = 456;
            expected.Series = "def";
            repository.Update(expected);

            var list = repository.List();
            var target = list.Where(mm => mm.Id == id).FirstOrDefault();
            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target);
        }

        [TestMethod]
        public void DeleteTest_RelatedAddressListIsRemoved()
        {
            var repository = new EquipmentMotionMechanismDbRepository(_dbConnString);
            repository.Clear();
            var expected = new MotionMechanism
            {
                Code = "01",
                IpAddress = "127.0.0.1",
                MechanismType = MechanismType.Plc,
                Oem = OEM.PlcMelsec,
                Protocol = Protocol.ModbusTcp,
                Port = 234,
                Series = "abc",
            };
            var id = repository.Add(expected);
            expected.Id = id;

            var addressRepository = new AddressCatalogDbRepository(_dbConnString);
            addressRepository.Add(new AddressRecord
            {
                Address = "d300",
                AxisId = 1,
                FuncCode = "abc",
                IOType = IOType.Rsts,
                IsEnable = true,
                MechanismId = id
            });

            repository.Delete(id);
            var target = repository.List().Where(m => m.Id == id).FirstOrDefault();
            Assert.IsNull(target);
            var addressList = addressRepository.List();
            Assert.AreEqual(0, addressList.Count);
        }

        [TestMethod]
        public void ClearTest()
        {
            var repository = new EquipmentMotionMechanismDbRepository(_dbConnString);
            var expected = new MotionMechanism
            {
                Code = "01",
                IpAddress = "127.0.0.1",
                MechanismType = MechanismType.Plc,
                Oem = OEM.PlcMelsec,
                Protocol = Protocol.ModbusTcp,
                Port = 234,
                Series = "abc",
            };
            var id = repository.Add(expected);
            repository.Clear();
            var list = repository.List();
            Assert.AreEqual(0, list.Count);
        }
    }
}
