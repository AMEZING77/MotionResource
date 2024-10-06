using DC.Resource2.MontionControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DC.Resource2.UnitTest
{
    [TestClass]
    public class EnumExtensionsUnitTest
    {
        [TestMethod]
        public void ToBindingItemsTest()
        {
            var target = EnumExtensions.ToBindingItems<MechanismType>();
            var expected = new List<BindingItem<MechanismType>>()
            {
                new BindingItem<MechanismType>{Name="PLC",Value=MechanismType.Plc},
                new BindingItem<MechanismType>{Name="板卡",Value=MechanismType.EmbeddedBoard},
            };
            Assert.IsTrue(target.SequenceEqual(expected));
        }

        [TestMethod]
        public void DescriptionTest()
        {
            var oem = OEM.PlcSiemens;
            Assert.AreEqual("西门子", oem.GetDescription());
        }
    }
}
