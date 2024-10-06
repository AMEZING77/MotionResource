using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2.MontionControl
{
    public class AddressValidator
    {
        private IAddressRepository _addressCatalog;
        public AddressValidator(IAddressRepository addressCatalog)
        {
            _addressCatalog = addressCatalog ?? throw new ArgumentNullException(nameof(addressCatalog));
        }

        public IList<string> Validate(AddressRecord target, bool exceptSelf = true)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(target.Address?.Trim()))
            { result.Add("地址不能为空"); }
            if (string.IsNullOrEmpty(target.FuncCode?.Trim()))
            { result.Add("功能码不能为空"); }
            var list = _addressCatalog.List();
            if (list.Any(a => a.FuncCode == target.FuncCode && (!exceptSelf || (exceptSelf && a.Id != target.Id))))
            { result.Add($"具有相同功能码【{target.FuncCode}】的条目已存在"); }
            if (list.Any(a => a.Address == target.Address && a.MechanismId == target.MechanismId
                && (!exceptSelf || (exceptSelf && a.Id != target.Id))))
            { result.Add($"具有相同地址【{target.Address}】的条目已存在"); }
            return result;
        }
    }
}
