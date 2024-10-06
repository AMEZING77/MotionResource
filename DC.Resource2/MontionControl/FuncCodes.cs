using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    internal class FuncCodes
    {
        public const string AbsoluteMove = "绝对运动";
        public const string RelativeMove = "相对运动";
        public const string Stop = "停止";
        public const string GoHome = "回原";
        public const string AxisBusy = "轴忙";

        public const string Heatbeat = "心跳";
        public const string ForwardLimit = "前限位";
        public const string BackwardLimit = "后限位";
        public const string HomeLimit = "原点限位";
        public const string ManualAuto = "手自动";
        public const string ServoState = "伺服状态";
        public const string CurrentPulse = "当前脉冲";
        public const string Fan = "风扇";
        public const string Door = "门";
        public const string AirPressure = "气压";
        public const string EmergenceStop = "急停";
        public const string Encoder = "编码器";
        public const string CodedDisc = "码盘";
        public const string AirSweep = "吹扫";
    }
}
