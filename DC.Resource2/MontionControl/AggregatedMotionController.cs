using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class AggregatedMotionController : IMotionController
    {
        private readonly IAddressRepository _addressCatalog;
        private readonly List<(int, IMotionController)> _motionControllers;
        private readonly List<AddressRecord> _addressRecords;
        public AggregatedMotionController(IAddressRepository addressCatalog, List<(int, IMotionController)> motionControllers)
        {
            _addressCatalog = addressCatalog ?? throw new ArgumentNullException(nameof(addressCatalog));
            _addressRecords = addressCatalog.List();
            _motionControllers = new List<(int, IMotionController)>(motionControllers) ?? throw new ArgumentNullException(nameof(_motionControllers));
            if (motionControllers.Count == 0) { throw new ArgumentException("传入的运动控制器数量不能为0"); }
        }

        public OperationResult Close()
        {
            var resultList = new List<OperationResult>();
            foreach (var (_, controller) in _motionControllers)
            {
                resultList.Add(controller.Close());
            }
            return PickFailedIfExists(resultList);
        }

        private OperationResult PickFailedIfExists(List<OperationResult> resList)
        {
            var target = resList.FirstOrDefault(r => !r.Succeeded);
            return target != null ? target : new OperationResult { Succeeded = true, ErrorCode = 0 };
        }

        public OperationResult GoHome(int axisId = 0)
        {
            var (res, controller) = Find(FuncCodes.GoHome);
            if (res != null) { return res; }
            return controller.GoHome(axisId);
        }

        private (OperationResult, IMotionController) Find(string funcCode)
        {
            var record = _addressRecords.FirstOrDefault(r => r.FuncCode == FuncCodes.GoHome && r.IsEnable);
            if (record == null) { return (new OperationResult() { ErrorCode = ErrorCodes.AddressNotExists, Succeeded = false, Message = $"{funcCode}地址条目不存在" }, null); }
            var target = _motionControllers.FirstOrDefault(c => c.Item1 == record.MechanismId);
            if (target == default) { return (new OperationResult() { ErrorCode = ErrorCodes.ControllerNotExists, Succeeded = false, Message = $"ID=【{record.MechanismId}】的运动控制器不存在" }, null); }
            return (null, target.Item2);
        }

        public OperationResult Initialize(float pulsePerMM)
        {
            var resultList = new List<OperationResult>();
            foreach (var (_, controller) in _motionControllers)
            {
                controller.Initialize(pulsePerMM);
            }
            return PickFailedIfExists(resultList);
        }


        public OperationResult Move(int axisId, int nPulsePos, int acc, int dec, int speed, MotionType motionType)
        {
            var (res, controller) = Find(FuncCodes.AbsoluteMove);
            if (res != null) { return res; }
            return controller.Move(axisId, nPulsePos, acc, dec, speed, motionType);
        }

        public OperationResult<byte[]> Read(string funcCode, int lengthInByte)
        {
            var (res, controller) = Find(funcCode);
            if (res != null) { return res.To<byte[]>(); }
            return controller.Read(funcCode, lengthInByte);
        }

        public OperationResult<T> Read<T>(string funcCode) where T : struct
        {
            var (res, controller) = Find(funcCode);
            if (res != null) { return res.To<T>(); }
            return controller.Read<T>(funcCode);
        }

        public OperationResult Stop(int axisId = 0)
        {
            var (res, controller) = Find(FuncCodes.Stop);
            if (res != null) { return res; }
            return controller.GoHome(axisId);
        }

        public OperationResult<bool> WaitDone(int axisId = 0)
        {
            var (res, controller) = Find(FuncCodes.AxisBusy);
            if (res != null) { return res.To<bool>(); }
            return controller.WaitDone(axisId);
        }

        public OperationResult Write(string funcCode, byte[] data)
        {
            var (res, controller) = Find(funcCode);
            if (res != null) { return res; }
            return controller.Write(funcCode, data);
        }

        public OperationResult Write<T>(string funcCode, T payload) where T : struct
        {
            var (res, controller) = Find(funcCode);
            if (res != null) { return res; }
            return controller.Write(funcCode, payload);
        }
    }
}
