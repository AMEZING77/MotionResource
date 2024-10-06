using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class EquipmentMotionMechanismMemoryRepository : IEquipmentMotionMechanismRepository
    {
        public int Add(MotionMechanism mechanism)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<MotionMechanism> List()
        {
            return new List<MotionMechanism>()
            {
                 new MotionMechanism()
                {
                    Id=1,
                    MechanismType=MechanismType.Plc,
                    Oem=OEM.PlcInovance,
                    Protocol=Protocol.ModbusTcp,
                    Series="H3U",
                    Code="PLC激光机",
                    IpAddress="192.168.200.11",
                    Port=502,
                    DataFormat=DataFormat.ABCD,
                },
                //new MotionMechanism()
                //{
                //    Id=1,
                //    MechanismType=MechanismType.Plc,
                //    Oem=OEM.PlcOmron,
                //    Protocol=Protocol.Fins,
                //    Series="",
                //    Code="PLC激光机",
                //    IpAddress="192.168.250.1",
                //    Port=9600,
                //    DataFormat=DataFormat.ABCD,
                //},
                // new MotionMechanism()
                //{
                //    Id=2,
                //    MechanismType=MechanismType.EmbeddedBoard,
                //    Oem=OEM.BoardDmc,
                //    Protocol=Protocol.Unknown,
                //    Series="2210",
                //    Code="雷塞激光机",
                //    IpAddress="",
                //    Port=0,
                //    DataFormat=DataFormat.ABCD,
                //},

            };
        }

        public void Update(MotionMechanism mechanism)
        {
            throw new NotImplementedException();
        }
    }

    public class MemoryAddressCatalog : IAddressRepository
    {
        public int Add(AddressRecord record)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<AddressRecord> List()
        {
            return new List<AddressRecord>()
            {
                new AddressRecord
                {
                    Address="D500",
                    FuncCode="相对运动",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D502",
                    FuncCode="绝对运动",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D313",
                    FuncCode="停止",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D314",
                    FuncCode="回原",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D302",
                    FuncCode="轴忙",
                    IsEnable=true,
                },

                new AddressRecord
                {
                    Address="D628",
                    FuncCode="维修模式",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D623",
                    FuncCode="红灯",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D624",
                    FuncCode="黄灯",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D625",
                    FuncCode="绿灯",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D626",
                    FuncCode="蜂鸣",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D641",
                    FuncCode="闪烁红灯",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D642",
                    FuncCode="闪烁黄灯",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D629",
                    FuncCode="辊压机停止",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D627",
                    FuncCode="激光机测量状态",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D630",
                    FuncCode="数字量打标",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D316",
                    FuncCode="轴复位",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D620",
                    FuncCode="标准片吹扫",
                    IsEnable=true,
                },

                new AddressRecord
                {
                    Address="D611",
                    FuncCode="原点限位",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D609",
                    FuncCode="小电柜急停",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D606",
                    FuncCode="手自动",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D605",
                    FuncCode="总气压报警",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D615",
                    FuncCode="激光头冷却气压报警",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D616",
                    FuncCode="标准片吹扫气压报警",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D618",
                    FuncCode="安全模块",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D700",
                    FuncCode="机架急停",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D701",
                    FuncCode="接触器1",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D702",
                    FuncCode="接触器2",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D602",
                    FuncCode="伺服",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D603",
                    FuncCode="负限位",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D604",
                    FuncCode="正限位",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D600",
                    FuncCode="风扇1",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D601",
                    FuncCode="风扇2",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D614",
                    FuncCode="门禁1",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D300",
                    FuncCode="电机位置脉冲",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D302",
                    FuncCode="轴忙",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D303",
                    FuncCode="轴出错",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D612",
                    FuncCode="码盘",
                    IsEnable=true,
                },
                new AddressRecord
                {
                    Address="D613",
                    FuncCode="辊压机状态",
                    IsEnable=true,
                },

            };

            //return new List<AddressRecord>()
            //{
            //    new AddressRecord()
            //    {
            //    Address="1",
            //    IOType=IOType.Axis,
            //    FuncCode="Axis",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="1",
            //    IOType=IOType.Rsts,
            //    FuncCode="Rsts",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="1",
            //    IOType=IOType.Common,
            //    FuncCode="Common",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="1",
            //    IOType=IOType.Pos,
            //    FuncCode="Pos",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="1",
            //    IOType=IOType.CheckDone,
            //    FuncCode="CheckDone",
            //    IsEnable=true,
            //    },

            //    new AddressRecord()
            //    {
            //    Address="3",
            //    FuncCode="红灯",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="4",
            //    FuncCode="黄灯",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="5",
            //    FuncCode="绿灯",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="7",
            //    FuncCode="蜂鸣",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="6",
            //    FuncCode="辊压机停止",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="6",
            //    FuncCode="激光机测量状态",
            //    IsEnable=true,
            //    },
            //    new AddressRecord()
            //    {
            //    Address="2",
            //    FuncCode="标准片吹扫",
            //    IsEnable=true,
            //    },


            //};
        }

        public void Update(AddressRecord record)
        {
            throw new NotImplementedException();
        }
    }

}
