using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class DmcMotionControllerBase
    {
        protected float _pulsePerMM;

        protected long MMToPulse(float mm)
        {
            double interVar = (double)mm;
            return (long)Math.Round(_pulsePerMM * interVar);//取最近的整数脉冲
        }
        //雷塞参数基本固定了，不做外部接口了，用于回原点，去各个起点的统一参数
        protected long Min_Vel => MMToPulse(2);//最小速度
        protected long Max_Vel => MMToPulse(50);//最大速度
        protected double Tacc => 1;//加速时间
        protected double Tdec => 1;//减速时间

        protected (ushort,IOType) GetBitNo(string funcCode)
        {
            var record = _addresses.FirstOrDefault(addr=>addr.FuncCode == funcCode && addr.IsEnable);
            if (record == null) { throw new NotSupportedException($"功能码{funcCode}未配置"); }
            if (ushort.TryParse(record.Address, out var bitNo))
            {
                if (bitNo < 1 || bitNo > 32) { throw new ArgumentException("地址位必须在1-32之间"); }
                return (bitNo,record.IOType);
            }
            else { throw new ArgumentException("非法的地址位，必须是1-32之间的数字字符串"); }
        }

        protected virtual void Initialize()
        {
            _addresses = _addressCatalog.List();
        }

        protected readonly IAddressRepository _addressCatalog;
        protected List<AddressRecord> _addresses;
        public DmcMotionControllerBase(IAddressRepository addressCatalog)
        {
            _addressCatalog = addressCatalog ?? throw new ArgumentNullException(nameof(addressCatalog));
        }
    }
}
