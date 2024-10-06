using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public interface IAddressRepository
    {
        List<AddressRecord> List();
        void Update(AddressRecord record);
        int Add(AddressRecord record);
        void Delete(int id);
    }

    public class AddressRecord
    {
        [Browsable(false)]
        public int Id { get; set; }
        [Browsable(false)]
        public int MechanismId { get; set; }
        [DisplayName("地址")]
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// IO类型，对于PLC使用默认的Common，DMC需要指定特定类型
        /// </summary>
        [DisplayName("IO类型")]
        public IOType IOType { get; set; }
        /// <summary>
        /// 轴ID，当前没有使用，后续扩展
        /// </summary>
        [Browsable(false)]
        public int AxisId { get; set; }
        /// <summary>
        /// 功能码
        /// </summary>
        [DisplayName("功能码")]
        public string FuncCode { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [DisplayName("启用")]
        public bool IsEnable { get; set; } = true;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is AddressRecord target)
            {
                return Id == target.Id && MechanismId == target.MechanismId
                    && Address == target.Address && IOType == target.IOType && AxisId == target.AxisId
                    && FuncCode == target.FuncCode && IsEnable == target.IsEnable;
            }
            return false;
        }
    }

    /// <summary>
    /// IO类别，对于PLC使用Common
    /// 引入此类型的目的主要是解决DMC卡针对不同的IO有不同的方法进行调整
    /// 当前仅对DMC板卡起作用
    /// </summary>
    public enum IOType
    {
        Common = 0,
        Axis = 1,
        Rsts = 2,
        Pos = 3,
        CheckDone = 4,
    }

}
