using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    internal class Constants
    {
        public const string dbDir = "../Settings";
        public static readonly string dbConnString = $"Data Source={dbDir}/resource.db";
    }
}
