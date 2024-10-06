using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HslCommunication;
using HslCommunication.Core.Device;
using Serilog;
using Serilog.Events;

namespace DC.Resource2
{
    public class PlcMotionController : IMotionController
    {
        private static OperateResult SuccessResult = OperateResult.CreateSuccessResult();
        private DeviceTcpNet _plcController;
        private readonly IAddressRepository _addressCatalog;
        private List<AddressRecord> _addresses;
        private volatile bool _isConnected;
        private readonly ILogger _logger;
        private float _pulsePerMM;

        public PlcMotionController(
            IAddressRepository addressCatalog,
            DeviceTcpNet plcController,
            ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _addressCatalog = addressCatalog ?? throw new ArgumentNullException(nameof(addressCatalog));
            _plcController = plcController ?? throw new ArgumentNullException(nameof(plcController));
        }

        public OperationResult Initialize(float pulsePerMM)
        {
            _pulsePerMM = pulsePerMM;
            _addresses = _addressCatalog.List();
            var res = Connect();
            return new OperationResult
            {
                Succeeded = res.IsSuccess,
                ErrorCode = res.IsSuccess ? 0 : ErrorCodes.ConnectionFailed,
                Message = res.IsSuccess ? "" : $"连接PLC失败，error code={res.ErrorCode}, message={res.Message}"
            };
        }

        public OperationResult GoHome(int axisId)
        {
            _logger.Information($"开始回原,轴ID={axisId}...");
            var res = WriteSingle(FuncCodes.GoHome, 1);
            if (!res.IsSuccess)
            {
                return new OperationResult
                {
                    Succeeded = false,
                    Message = $"回原点写入1失败，axisId={axisId}，error code={res.ErrorCode}, message={res.Message}"
                };
            }
            res = WriteSingle(FuncCodes.GoHome, 0);
            if (!res.IsSuccess)
            {
                return new OperationResult
                {
                    Succeeded = false,
                    Message = $"回原点写入0失败，axisId={axisId}，error code={res.ErrorCode}, message={res.Message}"
                };
            }
            _logger.Information($"回原信号写入成功,轴ID={axisId}");
            return OperationResult.Success;
        }

        public OperationResult Stop(int axisId)
        {
            _logger.Information($"开始停止,轴ID={axisId}...");
            var res = WriteSingle(FuncCodes.Stop, 1);
            if (!res.IsSuccess)
            {
                var msg = $"停止运动写入1失败，axisId={axisId}，error code={res.ErrorCode}, message={res.Message}";
                _logger.Error(msg);
                return OperationResult.CreateError(ErrorCodes.StopFailed, msg);
            }
            res = WriteSingle(FuncCodes.Stop, 0);
            if (!res.IsSuccess)
            {
                var msg = $"停止运动写入0失败，axisId={axisId}，error code={res.ErrorCode}, message={res.Message}";
                _logger.Error(msg);
                return OperationResult.CreateError(ErrorCodes.StopFailed, msg);
            }
            //停止后要将运动信号置为0
            WriteSingle(FuncCodes.RelativeMove, 0);
            WriteSingle(FuncCodes.AbsoluteMove, 0);
            _logger.Information($"停止运动信号写入成功,轴ID={axisId}");
            return OperationResult.Success;
        }

        public OperationResult Write(string funcCode, byte[] data)
        {
            var addrRecord = _addresses.FirstOrDefault(addr => addr.FuncCode == funcCode && addr.IsEnable);
            if (addrRecord == null) { return new OperationResult { ErrorCode = -13, Message = $"功能码{funcCode}的地址不存在" }; }
            return OperationResult.From(WriteBytes(addrRecord.Address, data));
        }

        public OperationResult<byte[]> Read(string funcCode, int length)
        {
            var addrRecord = _addresses.FirstOrDefault(addr => addr.FuncCode == funcCode && addr.IsEnable);
            if (addrRecord == null) { return new OperationResult<byte[]> { ErrorCode = -13, Message = $"功能码{funcCode}的地址不存在" }; }
            return OperationResult.From(ReadBytes(addrRecord.Address, length));
        }


        public OperationResult Move(int axisId, int nPulsePos, int nFacc, int nFdec, int nSpeed, MotionType motionType)
        {
            var motionParam = new MotionParam
            {
                sAccelebrationRadio = (short)nFacc,
                sDecekebrationRadio = (short)nFdec,
                nSpeed = nSpeed,
            };
            if (motionType == MotionType.Relative)
            {
                motionParam.nRelatiaveDistance = nPulsePos;
                motionParam.sRelatiaveStart = 0;
            }
            else if (motionType == MotionType.Absolute)
            {
                motionParam.nAbsoluteDistance = nPulsePos;
                motionParam.sAbsoluteStart = 0;
            }//对指定运动的指定点位处理，不使用默认构造

            var res = WriteStruct(FuncCodes.RelativeMove, motionParam);
            if (!res.IsSuccess) { return new OperationResult { ErrorCode = res.ErrorCode, Message = $"写入相对移动地址失败，{res.Message}" }; }

            //写完之后，读一下地址里面的数据有没有更新
            var readRes = ReadStruct<MotionParam>(FuncCodes.RelativeMove);
            if (readRes.IsSuccess)
            {
                if (motionType == MotionType.Absolute)
                {
                    _logger.Write(readRes.Content.nAbsoluteDistance == nPulsePos ? LogEventLevel.Information : LogEventLevel.Error,
                        readRes.Content.nAbsoluteDistance == nPulsePos
                        ? "运动控制，PLC 绝对运动的距离这个地址位写入成功"
                        : "运动控制，PLC 绝对运动的距离这个地址位写入失败");
                }
            }
            else { return new OperationResult { ErrorCode = readRes.ErrorCode, Message = $"读取时出现错误, {readRes.Message}" }; }

            //存在疑问，是否需要在此处加延时，因为上面都已确认地址刷新成功了；
            //plc时序问题导致的
            //Thread.Sleep(300);
            if (motionType == MotionType.Absolute) { motionParam.sAbsoluteStart = 1; }
            else { motionParam.sRelatiaveStart = 1; }
            res = WriteStruct(FuncCodes.RelativeMove, motionParam);
            if (!res.IsSuccess) { return new OperationResult { ErrorCode = res.ErrorCode, Message = res.Message }; }

            //存在疑问，是否需要在此处加延时，因为上面写入之后，下面立即刷新，是否会导致触发高电平的时长不够；
            res = motionType == MotionType.Absolute
                ? WriteSingle(FuncCodes.AbsoluteMove, 0)
                : WriteSingle(FuncCodes.RelativeMove, 0);
            return new OperationResult { Succeeded = res.IsSuccess, ErrorCode = res.ErrorCode, Message = res.Message };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MotionParam
        {
            /// <summary>
            /// D500
            /// </summary>
            public short sRelatiaveStart;
            /// <summary>
            /// D501
            /// </summary>
            public short sRelatiaveDown;
            /// <summary>
            /// D502
            /// </summary>
            public short sAbsoluteStart;
            /// <summary>
            /// D503
            /// </summary>
            public short sAbsoluteDown;
            /// <summary>
            /// D504
            /// </summary>
            public short other1;
            /// <summary>
            /// D505
            /// </summary>
            public short other2;
            /// <summary>
            /// D506
            /// </summary>
            public short other3;
            /// <summary>
            /// D507
            /// </summary>
            public short other4;
            /// <summary>
            /// D508
            /// </summary>
            public short other5;
            /// <summary>
            /// D509
            /// </summary>
            public short other6;
            /// <summary>
            /// 加速度比 D510
            /// </summary>
            public short sAccelebrationRadio;
            /// <summary>
            /// 减速度比 D511
            /// </summary>
            public short sDecekebrationRadio;
            /// <summary>
            /// 速度 D512
            /// </summary>
            public int nSpeed;
            /// <summary>
            /// 相对运动距离 D514
            /// </summary>
            public int nRelatiaveDistance;
            /// <summary>
            /// 绝对运动距离 D516
            /// </summary>
            public int nAbsoluteDistance;
        }

        /// <summary>
        /// 将结构体转换成字节数组
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        private static byte[] StructToBytes<T>(T structObj, out int size)
        {
            size = Marshal.SizeOf(structObj);
            byte[] bytes = new byte[size];
            GCHandle pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                //将结构体拷到分配好的内存空间
                Marshal.StructureToPtr(structObj, pointer, false);
            }
            finally { pinnedArray.Free(); }
            return bytes;
        }

        internal static T BytesToStuct<T>(byte[] bytes) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                throw new ArgumentException($"输入的Buffer长度{bytes.Length}小于结构体{typeof(T).Name}的大小");
            }
            GCHandle pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                //将内存空间转换为目标结构体
                return Marshal.PtrToStructure<T>(pointer);
            }
            finally { pinnedArray.Free(); }
        }

        private static void ReverseEndianess(byte[] bytes)
        {
            if (bytes == null) { throw new ArgumentNullException(nameof(bytes)); }
            if (bytes.Length < 2) { return; }
            for (int i = 0; i < bytes.Length; i += 2)
            {
                byte b = bytes[i];
                bytes[i] = bytes[i + 1];
                bytes[i + 1] = b;
            }
        }

        private OperateResult WriteBytes(string address, byte[] bytes)
        {
            if (_isConnected)
            {
                var writeRes = Write();
                if (!writeRes.IsSuccess) { return ReconnectThenWrite(); }
                return writeRes;
            }
            else { return ReconnectThenWrite(); }

            OperateResult Write()
            {
                var writeRes = _plcController.Write(address, bytes);
                if (!writeRes.IsSuccess)
                {
                    if (writeRes.ErrorCode < 0) { _isConnected = false; }
                    _logger.Error($"运动控制，PLC写入地址{address}失败, error code={writeRes.ErrorCode}, message={writeRes.Message}");
                }
                return writeRes;
            }

            OperateResult ReconnectThenWrite()
            {
                var res = Connect();
                if (res.IsSuccess)
                {
                    return Write();
                }
                else { return res; }
            }
        }

        private OperateResult Connect()
        {
            //从HSL的源码来看，ConnectServer并不是线程安全的，多线程并发读写PLC状态场景非常常见
            //因此这里加入锁以防止线程安全问题
            lock (_plcController)
            {
                //double check，如果某个线程重连成功，其它线程不必要再次重连
                if (_isConnected) { return SuccessResult; }
                _plcController.ConnectClose();
                var result = _plcController.ConnectServer();
                _isConnected = result.IsSuccess;
                _logger.Write(_isConnected ? LogEventLevel.Information : LogEventLevel.Error,
                    _isConnected ? "PLC连接成功" : $"PLC连接失败, error code={result.ErrorCode}, message={result.Message}");
                return result;
            }
        }

        private OperateResult<byte[]> ReadBytes(string strAdress, int length)
        {
            length = length / 2;
            if (_isConnected)
            {
                var readRes = Read();
                //如果读取失败，重新试一次
                if (!readRes.IsSuccess) { return ReconnectThenRead(); }
                return readRes;
            }
            else { return ReconnectThenRead(); }

            OperateResult<byte[]> Read()
            {
                var readRes = _plcController.Read(strAdress, (ushort)length);
                if (!readRes.IsSuccess)
                {
                    if (readRes.ErrorCode < 0) { _isConnected = false; }
                    _logger.Error($"运动控制，PLC读取地址{strAdress}失败, error code={readRes.ErrorCode}, message={readRes.Message}");
                    return readRes;
                }
                else
                {
                    //ReverseEndianess(readRes.Content);
                    return readRes;
                }
            }

            OperateResult<byte[]> ReconnectThenRead()
            {
                var res = Connect();
                if (!res.IsSuccess)
                { return new OperateResult<byte[]>(res.ErrorCode, res.Message); }
                return Read();
            }
        }


        private OperateResult WriteSingle(string funcCode, ushort value)
        {
            var bytes = new byte[2];
            //这里使用BigEndian写入，类似于激光机中交换连续2个Byte实现
            BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
            var addrRecord = _addresses.FirstOrDefault(addr => addr.FuncCode == funcCode && addr.IsEnable);
            if (addrRecord == null) { return new OperateResult { ErrorCode = -1, Message = $"功能码{funcCode}的地址不存在" }; }
            return WriteBytes(addrRecord.Address, bytes);
        }

        private OperateResult WriteStruct<T>(string funcCode, T value)
        {
            var bytes = StructToBytes<T>(value, out var size);
            ReverseEndianess(bytes);
            var addrRecord = _addresses.FirstOrDefault(addr => addr.FuncCode == funcCode && addr.IsEnable);
            if (addrRecord == null) { return new OperateResult { ErrorCode = -13, Message = $"功能码{funcCode}的地址不存在" }; }
            return WriteBytes(addrRecord.Address, bytes);
        }

        private OperateResult<T> ReadStruct<T>(string funcCode) where T : struct
        {
            var addrRecord = _addresses.FirstOrDefault(addr => addr.FuncCode == funcCode && addr.IsEnable);
            if (addrRecord == null) { return new OperateResult<T> { ErrorCode = -13, Message = $"功能码{funcCode}的地址不存在" }; }
            var readRes = ReadBytes(addrRecord.Address, Marshal.SizeOf(typeof(T)));
            if (!readRes.IsSuccess) { return new OperateResult<T>(readRes.ErrorCode, readRes.Message); }
            ReverseEndianess(readRes.Content);
            T res = BytesToStuct<T>(readRes.Content);
            return new OperateResult<T> { IsSuccess = true, Content = res };
        }

        public OperationResult<bool> WaitDone(int axisId)
        => OperationResult.From(ReadStruct<bool>(FuncCodes.AxisBusy));

        public OperationResult Write<T>(string funcCode, T payload) where T : struct
        => OperationResult.From(WriteStruct<T>(funcCode, payload));

        public OperationResult<T> Read<T>(string funcCode) where T : struct
        => OperationResult.From(ReadStruct<T>(funcCode));

        public OperationResult Close()
        {
            return OperationResult.From(_plcController?.ConnectClose());
        }
    }
}
