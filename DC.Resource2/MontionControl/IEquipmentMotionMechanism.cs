using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DC.Resource2;
using HslCommunication.Core;

namespace DC.Resource2
{
    public interface IEquipmentMotionMechanismRepository
    {
        List<MotionMechanism> List();
        void Update(MotionMechanism mechanism);

        int Add(MotionMechanism mechanism);
        void Delete(int id);
    }

    public class MotionMechanism
    {
        public int Id { get; set; }
        /// <summary>
        /// 设备类型,当前支持PLC,板卡
        /// </summary>
        public MechanismType MechanismType { get; set; }
        /// <summary>
        /// 设备厂商
        /// </summary>
        public OEM Oem { get; set; }
        /// <summary>
        /// 采用的协议类型，主要是考虑PLC会有多种协议支持
        /// </summary>
        public Protocol Protocol { get; set; }
        /// <summary>
        /// 厂商的设备系列
        /// </summary>
        public string Series { get; set; }
        /// <summary>
        /// 设备编号，必须保证唯一
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public ushort Port { get; set; }
        /// <summary>
        /// 数据格式，暂且不用
        /// </summary>
        public DataFormat DataFormat { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is MotionMechanism t)
            {
                return Id == t.Id && MechanismType == t.MechanismType &&
                    Protocol == t.Protocol && Series == t.Series && Code == t.Code
                    && IpAddress == t.IpAddress && t.Code == t.Code && IpAddress == t.IpAddress
                    && Port == t.Port /*&& DataFormat == t.DataFormat 暂且不用*/;
            }
            return false;
        }

        public string GetDescription()
        => $"{Oem.GetDescription()}-{Series}-{Code}";
    }

    public enum MechanismType
    {
        /// <summary>
        /// PLC
        /// </summary>
        [Description("PLC")]
        Plc = 0,
        /// <summary>
        /// 板卡
        /// </summary>
        [Description("板卡")]
        EmbeddedBoard = 1,
    }

    /// <summary>
    /// 厂商列表，PLC厂商命名时需要以Plc为前缀
    /// </summary>
    public enum OEM
    {
        [Description("汇川")]
        PlcInovance = 0,
        [Description("三菱")]
        PlcMelsec = 1,
        [Description("欧母龙")]
        PlcOmron = 2,
        [Description("西门子")]
        PlcSiemens = 3,
        [Description("信捷")]
        PlcXinJe = 4,
        [Description("雷塞")]
        BoardDmc = 100,
        [Description("固高")]
        BoardGoogol = 101,
    }

    public enum Protocol
    {
        [Description("未指定")] Unknown = 0,
        /// <summary>
        /// 基于TCP的Modbus
        /// </summary>
        [Description("ModbusTcp")]
        ModbusTcp = 1,
        /// <summary>
        /// 西门子协议
        /// </summary>
        [Description("西门子S7")]
        S7 = 2,
        /// <summary>
        /// Melsec专用协议
        /// </summary>
        [Description("三菱Mc")]
        Mc = 3,
        /// <summary>
        /// Omron专用协议
        /// </summary>
        [Description("欧母龙Fins")]
        Fins = 4,
    }
}
