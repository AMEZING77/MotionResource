using DC.Resource2.MontionControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace DC.Resource2.UnitTest
{
    [TestClass]
    public class AddressValidatorUnitTest
    {
        private List<AddressRecord> _addressList;
        private AddressValidator _validator;
        private AddressRecord _target;

        [TestInitialize]
        public void Initialize()
        {
            var catalogMock = new Mock<IAddressRepository>();
            _addressList = new List<AddressRecord>();
            catalogMock.Setup(c => c.List()).Returns(_addressList);
            _validator = new AddressValidator(catalogMock.Object);
            _target = new AddressRecord
            {
                Address = "d301",
                AxisId = 10,
                FuncCode = "xxxx",
                IOType = IOType.Axis,
                IsEnable = true,
                MechanismId = 1
            };
        }

        [TestMethod]
        public void AddressShouldNotBeNull()
        {
            _target.Address = " ";
            var res = _validator.Validate(_target);
            Assert.IsTrue(res.Contains("地址不能为空"));
        }

        [TestMethod]
        public void FuncCodeShouldNotBeNull()
        {
            _target.FuncCode = " ";
            var res = _validator.Validate(_target);
            Assert.IsTrue(res.Contains("功能码不能为空"));
        }

        [TestMethod]
        public void FuncCodeShouldBeUnique()
        {
            _addressList.Add(new AddressRecord
            {
                Address = "d302",
                AxisId = 10,
                FuncCode = "xxxx",
                IOType = IOType.Axis,
                IsEnable = true,
                MechanismId = 1
            });
            var res = _validator.Validate(_target, false);
            Assert.IsTrue(res.Contains($"具有相同功能码【{_target.FuncCode}】的条目已存在"));
            res = _validator.Validate(_target, true);
            Assert.IsFalse(res.Contains($"具有相同功能码【{_target.FuncCode}】的条目已存在"));
        }

        [TestMethod]
        public void AddressMechIdCombinedShouldBeUnique()
        {
            _addressList.Add(new AddressRecord
            {
                Address = "d301",
                AxisId = 10,
                FuncCode = "yyyy",
                IOType = IOType.Axis,
                IsEnable = true,
                MechanismId = 1
            });
            var res = _validator.Validate(_target, false);
            Assert.IsTrue(res.Contains($"具有相同地址【{_target.Address}】的条目已存在"));
            res = _validator.Validate(_target, true);
            Assert.IsFalse(res.Contains($"具有相同地址【{_target.Address}】的条目已存在"));
        }
    }
}
