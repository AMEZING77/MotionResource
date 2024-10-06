using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DC.Resource2
{
    public class BoardControllerFactory
    {
        private static readonly Dictionary<OEM, string[]> _supportedSeries;
        private readonly ILogger _logger;
        private readonly IAddressRepository _addressCatalog;

        static BoardControllerFactory()
        {
            _supportedSeries = new Dictionary<OEM, string[]>()
            {
                [OEM.BoardDmc] = new[] { "2210", "2410" },
                [OEM.BoardGoogol] = new[] { "Unspecified" },
            };
            SupportedSeries = new ReadOnlyDictionary<OEM, string[]>(_supportedSeries);
        }

        public BoardControllerFactory(ILogger logger,IAddressRepository addressCatalog)
        {
            _logger = logger;
            _addressCatalog = addressCatalog;
        }

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
        /// <remarks></remarks>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IMotionController Create(OEM oem, Protocol protocol, string series, string ipAddr, ushort port = 502)
        {
            if (!_supportedSeries.ContainsKey(oem)) { throw new NotSupportedException($"未支持的厂商{oem}"); }

            if (oem == OEM.BoardDmc)
            {
                if (series == "2210") { return new Dmc2210MotionController(_addressCatalog); }
                else if (series == "2410") { return new Dmc2410MotionController(_addressCatalog); }
                else { throw new NotSupportedException($"未支持的DMC板卡系列{series}"); }
            }
            else if (oem == OEM.BoardGoogol)
            {
                throw new NotImplementedException();
            }

            throw new NotSupportedException("未支持的板卡厂商");
        }
    }
}
