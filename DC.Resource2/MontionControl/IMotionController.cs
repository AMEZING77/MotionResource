using HslCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public interface IMotionController
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pulsePerMM"></param>
        /// <returns></returns>
        OperationResult Initialize(float pulsePerMM);
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        OperationResult Close();
        /// <summary>
        /// 运动
        /// </summary>
        /// <param name="axisId">轴ID</param>
        /// <param name="nPulsePos">脉冲位置</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <param name="speed">运动速度(脉冲单位)</param>
        /// <param name="motionType">运动类型</param>
        /// <returns></returns>
        OperationResult Move(int axisId, int nPulsePos, int acc, int dec, int speed, MotionType motionType);
        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="axisId">轴ID</param>
        /// <returns></returns>
        OperationResult GoHome(int axisId = 0);
        /// <summary>
        /// 停止运动
        /// </summary>
        /// <param name="axisId">轴ID</param>
        /// <returns></returns>
        OperationResult Stop(int axisId = 0);
        /// <summary>
        /// 等待轴运动完成
        /// </summary>
        /// <returns></returns>
        OperationResult<bool> WaitDone(int axisId = 0);
        /// <summary>
        /// 向指定功能码代表的地址写入地址
        /// </summary>
        /// <param name="funcCode">功能码</param>
        /// <param name="data">数据</param>
        /// <remarks>对于非PLC控制，不配置功能码，funcCode表示IO的编号</remarks>
        /// <returns></returns>
        OperationResult Write(string funcCode, byte[] data);
        OperationResult Write<T>(string funcCode, T payload) where T : struct;
        /// <summary>
        /// 从指定功能码代表的地址读取数据
        /// </summary>
        /// <param name="funcCode">功能码</param>
        /// <param name="lengthInByte">读取字节长度</param>
        /// <returns></returns>
        OperationResult<byte[]> Read(string funcCode, int lengthInByte);
        OperationResult<T> Read<T>(string funcCode) where T : struct;
    }

    //public interface IMotionStatus
    //{
    //    OperationResult<short> Heatbeat();
    //    OperationResult<bool> ForwardLimit();
    //    OperationResult<bool> BackwardLimit();
    //    OperationResult<bool> HomeLimit();
    //    OperationResult<bool> ManualAuto();
    //    OperationResult<bool> ServoState();
    //    OperationResult<long> MotionPulse();
    //}

    //public interface IDIDO
    //{
    //    OperationResult<bool> Fan(int id);
    //    OperationResult<bool> Door(int id);
    //    OperationResult<bool> AirPressure(int id);
    //    OperationResult<bool> EmergenceStop(int id);

    //    OperationResult<int> Encoder();
    //    OperationResult<bool> CodedDisc();
    //    OperationResult AirSweep(int id);
    //}

    public enum MotionType
    {
        Absolute = 0, Relative = 1,
    }

    public class OperationResult
    {
        public bool Succeeded { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }

        public static OperationResult CreateError(int errorCode, string message)
        => new OperationResult { ErrorCode = errorCode, Message = message, Succeeded = false };

        public static OperationResult Success => new OperationResult { ErrorCode = 0, Succeeded = true };

        public static OperationResult<T> From<T>(OperateResult<T> res, string messageAppend = null)
        {
            if (res.IsSuccess) { return new OperationResult<T> { Succeeded = true, ErrorCode = 0, Content = res.Content }; }
            return new OperationResult<T> { ErrorCode = res.ErrorCode, Message = $"{messageAppend}, {res.Message}", Succeeded = false };
        }

        public static OperationResult From(OperateResult res, string messageAppend = null)
        {
            if (res.IsSuccess) { return new OperationResult { Succeeded = true, ErrorCode = 0 }; }
            return new OperationResult { ErrorCode = res.ErrorCode, Message = $"{messageAppend}, {res.Message}", Succeeded = false };
        }

        public OperationResult<T> To<T>(string messsageAppend = null)
        {
            return new OperationResult<T> { ErrorCode = ErrorCode, Message = $"{messsageAppend}, {Message}", Succeeded = Succeeded };
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Content { get; set; }
    }
}
