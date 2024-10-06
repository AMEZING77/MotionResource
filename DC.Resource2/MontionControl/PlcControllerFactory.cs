using HslCommunication.Core.Device;
using HslCommunication.Profinet.Siemens;
using HslCommunication.Profinet.Inovance;
using HslCommunication.Profinet.Omron;
using HslCommunication.Profinet.XINJE;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslCommunication.ModBus;
using HslCommunication.Profinet.Melsec;

namespace DC.Resource2
{
    public class PlcSpec
    {
        public string[] Protocols { get; set; }
        public string[] Specs { get; set; }
    }

    public class PlcControllerFactory
    {
        private static readonly Dictionary<OEM, Protocol[]> _supportedProtocol;
        private static readonly Dictionary<OEM, string[]> _supportedSeries;
        static PlcControllerFactory()
        {
            //当前仅支持基于TCP的协议类型
            _supportedProtocol = new Dictionary<OEM, Protocol[]>()
            {
                [OEM.PlcOmron] = new[] { Protocol.Fins },
                [OEM.PlcMelsec] = new[] { Protocol.ModbusTcp, Protocol.Mc },
                [OEM.PlcSiemens] = new[] { Protocol.S7 },
                [OEM.PlcInovance] = new[] { Protocol.ModbusTcp },
                [OEM.PlcXinJe] = new[] { Protocol.ModbusTcp },
            };
            _supportedSeries = new Dictionary<OEM, string[]>()
            {
                [OEM.PlcOmron] = new[] { "未指定" },
                [OEM.PlcMelsec] = new[] { "未指定" },
                [OEM.PlcSiemens] = Enum.GetNames(typeof(SiemensPLCS)),
                [OEM.PlcInovance] = Enum.GetNames(typeof(InovanceSeries)),
                [OEM.PlcXinJe] = Enum.GetNames(typeof(XinJESeries)),
            };
            HslCommunication.Authorization.SetAuthorizationCode("942b4acc-96a7-4d19-9090-76b3ce868be7");
            SupportedProtocol = new ReadOnlyDictionary<OEM, Protocol[]>(_supportedProtocol);
            SupportedSeries = new ReadOnlyDictionary<OEM, string[]>(_supportedSeries);
        }

        public PlcControllerFactory()
        {
        }

        public static ReadOnlyDictionary<OEM, Protocol[]> SupportedProtocol { get; }
        public static ReadOnlyDictionary<OEM, string[]> SupportedSeries { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oem"></param>
        /// <param name="protocol"></param>
        /// <param name="series"></param>
        /// <param name="ipAddr"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        /// <remarks>当前仅支持TCP通信，暂不考虑UDP、SerialPort、MQTT等其它协议</remarks>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public DeviceTcpNet Create(OEM oem, Protocol protocol, string series, string ipAddr, ushort port = 502)
        {
            if (!_supportedProtocol.ContainsKey(oem)) { throw new NotSupportedException($"未支持的PLC厂商{oem}"); }
            if (!_supportedProtocol[oem].Contains(protocol))
            { throw new NotSupportedException($"{oem}当前尚不支持{protocol}"); }
            if (protocol == Protocol.Unknown) { protocol = Protocol.ModbusTcp; }

            if (oem == OEM.PlcInovance)
            {
                if (!SupportedSeries[oem].Contains(series)) { throw new NotSupportedException($"尚未支持的汇川PLC型号{series}"); }
                var controller = new InovanceTcpNet(ipAddr);
                controller.ByteTransform.DataFormat = DataFormat.CDAB;
                controller.Series = (InovanceSeries)Enum.Parse(typeof(InovanceSeries), series);
                controller.ConnectTimeOut = 1000;
                return controller;
            }
            else if (oem == OEM.PlcSiemens)
            {
                if (string.IsNullOrEmpty(series)) { throw new ArgumentException("西门子PLC必须指定具体型号"); }
                if (!SupportedSeries[oem].Contains(series)) { throw new NotSupportedException($"尚未支持的西门子PLC型号{series}"); }
                var controller = new SiemensS7Net((SiemensPLCS)Enum.Parse(typeof(SiemensPLCS), series));
                return controller;
            }
            else if (oem == OEM.PlcMelsec)
            {
                if (protocol == Protocol.ModbusTcp)
                {
                    var controller = new ModbusTcpNet(ipAddr);
                    controller.ByteTransform.DataFormat = DataFormat.CDAB;
                    controller.AddressStartWithZero = true;
                    controller.ConnectTimeOut = 1000;
                    return controller;
                }
                else if (protocol == Protocol.Mc)
                {
                    return new MelsecMcNet(ipAddr, port);
                }
                else { throw new NotSupportedException($"未支持的三菱PLC协议{protocol}"); }
            }
            else if (oem == OEM.PlcXinJe)
            {
                var controller = new XinJETcpNet(ipAddr)
                {
                    AddressStartWithZero = true,
                    Station = 1,
                    Series = XinJESeries.XD,
                    ConnectTimeOut = 1000
                };
                controller.ByteTransform.DataFormat = DataFormat.ABCD;
                return controller;
            }
            else if (oem == OEM.PlcOmron)
            {
                var controller = new OmronFinsNet(ipAddr, port)
                {
                    ConnectTimeOut = 1000,
                    SA1 = 0x64,//PC网络号，PC的IP地址的最后一个数
                    DA1 = 0x01,//PLC网络号，PLC的IP地址的最后一个数
                    DA2 = 0x00//PLC单元号，通常为0
                };
                return controller;
            }
            throw new NotSupportedException("未支持的PLC厂商");
        }
    }
}
