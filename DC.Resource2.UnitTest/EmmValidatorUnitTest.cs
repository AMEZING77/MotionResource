using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using DC.Resource2.MontionControl;
using System.Collections.Generic;

namespace DC.Resource2.UnitTest
{
    [TestClass]
    public class EmmValidatorUnitTest
    {
        EmmValidator _validator;
        MotionMechanism _target;
        Mock<IEquipmentMotionMechanismRepository> _repoMock;
        List<MotionMechanism> _motionMechanismList;

        [TestInitialize]
        public void Initialize()
        {
            _repoMock = new Mock<IEquipmentMotionMechanismRepository>();
            _motionMechanismList = new List<MotionMechanism>();
            _repoMock.Setup(m => m.List()).Returns(_motionMechanismList);
            _validator = new EmmValidator(_repoMock.Object);
            _target = new MotionMechanism
            {
                Code = "plc01",
                IpAddress = "127.0.0.1",
                Oem = OEM.PlcInovance,
                MechanismType = MechanismType.Plc,
                Port = 402,
                Protocol = Protocol.Mc,
                Series = "acbc"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _motionMechanismList.Clear();
        }

        [TestMethod]
        public void CodeShouldNotBeNull()
        {
            _target.Code = "";
            var res = _validator.Validate(_target);
            Assert.IsTrue(res.Contains("编号不能为空"));
        }

        [TestMethod]
        public void IPAddressShouldNotBeNull()
        {
            _target.IpAddress = "  ";
            var res = _validator.Validate(_target);
            Assert.IsTrue(res.Contains("IP地址不能为空"));
        }

        [TestMethod]
        public void IPAdressValidity()
        {
            _target.IpAddress = "324";
            var res = _validator.Validate(_target);
            Assert.IsTrue(res.Contains($"IP地址【{_target.IpAddress}】非法"));
        }

        [TestMethod]
        public void CodeShouldBeUnique()
        {
            _motionMechanismList.Add(
                new MotionMechanism
                {
                    Code = "plc01",
                    IpAddress = "127.0.0.1",
                    Oem = OEM.PlcInovance,
                    MechanismType = MechanismType.Plc,
                    Port = 402,
                    Protocol = Protocol.Mc,
                    Series = "acbc"
                }
            );
            var res = _validator.Validate(_target, false);
            Assert.IsTrue(res.Contains($"具体相同编号【{_target.Code}】的运动控制机构已存在"));
            res = _validator.Validate(_target, true);
            Assert.IsFalse(res.Contains($"具体相同编号【{_target.Code}】的运动控制机构已存在"));
        }

        [TestMethod]
        public void IPPortCombinedShouldBeUnique()
        {
            _motionMechanismList.Add(
                new MotionMechanism
                {
                    Code = "plc02",
                    IpAddress = "127.0.0.1",
                    Oem = OEM.PlcInovance,
                    MechanismType = MechanismType.Plc,
                    Port = 402,
                    Protocol = Protocol.Mc,
                    Series = "acbc"
                }
            );
            var res = _validator.Validate(_target, false);
            Assert.IsTrue(res.Contains($"具体相同IP地址与端口号【{_target.IpAddress}:{_target.Port}】的运动控制机构已存在"));
            res = _validator.Validate(_target, true);
            Assert.IsFalse(res.Contains($"具体相同IP地址与端口号【{_target.IpAddress}:{_target.Port}】的运动控制机构已存在"));
        }
    }
}
