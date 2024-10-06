using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public static class EnumExtensions
    {
        public static List<BindingItem<T>> ToBindingItems<T>() where T : Enum
        {
            var res = new List<BindingItem<T>>();
            var type = typeof(T);
            foreach (var item in Enum.GetValues(type).Cast<T>())
            {
                var memInfo = type.GetMember(item.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0)
                { res.Add(new BindingItem<T> { Name = ((DescriptionAttribute)attributes[0]).Description, Value = item }); }
            }
            return res;
        }

        public static string GetDescription<T>(this T value) where T : Enum
        {
            var type = typeof(T);
            var memInfo = type.GetMember(value.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0) { return ((DescriptionAttribute)attributes[0]).Description; }
            return string.Empty;
        }

        public static bool IsPlcOem(this OEM oem)
        => oem.ToString().StartsWith("Plc");
    }

    public class BindingItem<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is BindingItem<T> t)
            {
                return Name == t.Name && Value.Equals(t.Value);
            }
            return false;
        }
    }
}
