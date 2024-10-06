using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class MotionControllerFactory
    {
        private readonly IAddressRepository _addressCatalog;
        private readonly IEquipmentMotionMechanismRepository _motionMechanismRepository;
        private readonly PlcControllerFactory _plcControllerFactory;
        private readonly ILogger _logger;

        public MotionControllerFactory(
            IEquipmentMotionMechanismRepository motionMechanismRepository,
            IAddressRepository addressCatalog,
            PlcControllerFactory plcFactory,
            ILogger logger)
        {
            _addressCatalog = addressCatalog ?? throw new ArgumentNullException(nameof(addressCatalog));
            _motionMechanismRepository = motionMechanismRepository ?? throw new ArgumentNullException(nameof(motionMechanismRepository));
            _plcControllerFactory = plcFactory ?? throw new ArgumentNullException(nameof(plcFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IMotionController Create()
        {
            var mechanisms = _motionMechanismRepository.List();
            if (mechanisms.Count == 0) { throw new InvalidOperationException("当前尚未配置运动机构"); }
            if (mechanisms.Count == 1)
            {
                var target = mechanisms[0];
                return Create(target);
            }
            else
            {
                var controllerList = new List<(int, IMotionController)>();
                foreach (var target in mechanisms)
                {
                    controllerList.Add((target.Id, Create(target)));
                }
                return new AggregatedMotionController(_addressCatalog, controllerList);
            }
        }

        private IMotionController Create(MotionMechanism target)
        {
            if (target.MechanismType == MechanismType.Plc)
            {
                var plcController = _plcControllerFactory.Create(target.Oem, target.Protocol, target.Series, target.IpAddress, target.Port);
                return new PlcMotionController(_addressCatalog, plcController, _logger);
            }
            else if (target.MechanismType == MechanismType.EmbeddedBoard)
            {
                var boardControllerFactory = new BoardControllerFactory(_logger, _addressCatalog);
                return boardControllerFactory.Create(target.Oem, target.Protocol, target.Series, target.IpAddress, target.Port);
            }
            else { throw new ArgumentException("未知的运动机构类型"); }
        }
    }
}
