using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class GoogolMotionController : IMotionController
    {
        public OperationResult Close()
        {
            throw new NotImplementedException();
        }

        public OperationResult GoHome(int axisId = 0)
        {
            throw new NotImplementedException();
        }

        public OperationResult Initialize(float pulsePerMM)
        {
            throw new NotImplementedException();
        }

        public OperationResult Move(int axisId, int nPulsePos, int acc, int dec, int speed, MotionType motionType)
        {
            throw new NotImplementedException();
        }

        public OperationResult<byte[]> Read(string funcCode, int lengthInByte)
        {
            throw new NotImplementedException();
        }

        public OperationResult<T> Read<T>(string funcCode) where T : struct
        {
            throw new NotImplementedException();
        }

        public OperationResult Stop(int axisId = 0)
        {
            throw new NotImplementedException();
        }

        public OperationResult<bool> WaitDone(int axisId = 0)
        {
            throw new NotImplementedException();
        }

        public OperationResult Write(string funcCode, byte[] data)
        {
            throw new NotImplementedException();
        }

        public OperationResult Write<T>(string funcCode, T payload) where T : struct
        {
            throw new NotImplementedException();
        }
    }
}
