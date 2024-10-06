using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DC.Resource2.MontionControl
{
    public class EmmValidator
    {
        private static Regex ipaddrRegex = new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        private readonly IEquipmentMotionMechanismRepository _repository;

        public EmmValidator(IEquipmentMotionMechanismRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public IList<string> Validate(MotionMechanism target, bool exceptSelf = true)
        {
            if (target == null) { throw new ArgumentNullException("target"); }
            var result = new List<string>();
            if (string.IsNullOrEmpty(target.IpAddress?.Trim())) { result.Add("IP地址不能为空"); }
            if (!ipaddrRegex.IsMatch(target.IpAddress)) { result.Add($"IP地址【{target.IpAddress}】非法"); }
            if (string.IsNullOrEmpty(target.Code)) { result.Add("编号不能为空"); }

            var list = _repository.List();
            if (list.Any(m => m.Code == target.Code
                    && (!exceptSelf || (exceptSelf && m.Id != target.Id))))
            { result.Add($"具体相同编号【{target.Code}】的运动控制机构已存在"); }
            if (list.Any(m => m.IpAddress == target.IpAddress && m.Port == m.Port
                    && (!exceptSelf || (exceptSelf && m.Id != target.Id))))
            { result.Add($"具体相同IP地址与端口号【{target.IpAddress}:{target.Port}】的运动控制机构已存在"); }

            return result;
        }
    }
}
