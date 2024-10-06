using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static DC.Resource2.Dmc2410;

namespace DC.Resource2
{
    public class Dmc2410MotionController : DmcMotionControllerBase, IMotionController
    {
        public Dmc2410MotionController(IAddressRepository addressCatalog)
        : base(addressCatalog) { }

        #region Impl
        public OperationResult Initialize(float pulsePerMM)
        {
            base.Initialize();

            _pulsePerMM = pulsePerMM;
            ushort sCardCount = d2410_board_init();
            if (sCardCount == 0)
            {
                return new OperationResult { ErrorCode = -5, Succeeded = false, Message = "没有找到DMC2420-PIC控制卡!" };
            }
            else
            {
                //新机
                ushort axis = 0;
                d2410_config_EMG_PIN(0, 1, 1);
                d2410_set_pulse_outmode(axis, 0);//设置脉冲输出模式
                d2410_config_EL_MODE(axis, 1);//设置限位信号立即停，高电平有效
                d2410_config_ALM_PIN(axis, 1, 0);//设置报警信号立即停，高电平有效
            }
            return OperationResult.Success;
        }

        public OperationResult Close()
        {
            d2410_board_close();
            return OperationResult.Success;
        }

        public OperationResult Move(int axisId, int nPulsePos, int acc, int dec, int speed, MotionType motionType)
        {
            ushort axis = (ushort)axisId;
            d2410_set_profile(axis, Min_Vel, speed, Tacc, Tdec);
            d2410_t_pmove(axis, nPulsePos, 1);//绝对运动
            return OperationResult.Success;
        }

        public OperationResult GoHome(int axisId = 0)
        {
            //雷塞必须手动判定当前位置，再决定指令“方向”
            //需要手写PLC的找原点逻辑
            //先回负限位，再向前运动一段距离超过原点，再反向回原点
            //整个过程中，如果触发外部急停、门禁。。信号，需要立即终止；

            ushort axis = (ushort)axisId;
            d2410_set_profile(axis, Min_Vel, Max_Vel, Tacc, Tdec);

            d2410_t_vmove(axis, 0);//1、回负限位，负方向运动
            do
            {
                if (IsMinussLimit(axis))
                {
                    d2410_decel_stop(axis, Tdec);
                    waitDone(axis);
                    break;
                }
                Thread.Sleep(1);
            } while (true);//2、检测到负限位信号，先缓停，再判断轴是否停止

            d2410_t_pmove(axis, (int)MMToPulse(100), 0);//3、相对运动一定距离，超过原点，默认100mm
            waitDone(axis);
            d2410_home_move(axis, 2, 0);//4、反向回原点
            waitDone(axis);
            d2410_set_position(0, 0);//设置绝对位置为0，清零
            return OperationResult.Success;
        }

        public OperationResult Stop(int axisId = 0)
        {
            ushort axis = (ushort)axisId;
            d2410_decel_stop(axis, Tdec);
            waitDone(axis);//等待
            return OperationResult.Success;
        }

        public OperationResult<bool> WaitDone(int axisId = 0)
        {
            ushort axis = (ushort)axisId;
            waitDone(axis);
            return new OperationResult<bool> { Succeeded = true, ErrorCode = 0, Content = true };
        }

        public OperationResult Write(string funcCode, byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.Length < 2) { throw new ArgumentException("输入必须至少2个字节"); }
            var (bitNo, iotype) = GetBitNo(funcCode);
            var value = BitConverter.ToUInt16(data, 0);
            d2410_write_outbit(0, bitNo, value);
            return OperationResult.Success;
        }

        public OperationResult Write<T>(string funcCode, T payload) where T : struct
        {
            if (typeof(T) != typeof(short)) throw new NotSupportedException();
            var (bitNo, iotype) = GetBitNo(funcCode);
            d2410_write_outbit(0, bitNo, Convert.ToUInt16(payload));
            return OperationResult.Success;
        }

        public OperationResult<byte[]> Read(string funcCode, int lengthInByte)
        {
            int res = ReadIO(funcCode);
            return new OperationResult<byte[]> { ErrorCode = 0, Content = BitConverter.GetBytes(res) };
        }

        private int ReadIO(string funcCode)
        {
            var (bitNo, iotype) = GetBitNo(funcCode);
            int res;
            if (iotype == IOType.Common)
            {
                //res = d2410_read_inbit(0, bitNo);
                res = d2410_read_inport(0);
            }
            else if (iotype == IOType.Axis)
            {
                res = d2410_axis_io_status(0);
            }
            else if (iotype == IOType.Rsts)
            {
                res = (int)d2410_get_rsts(0);
            }
            else if (iotype == IOType.Pos)
            {
                res = (int)d2410_get_position(0);
            }
            else if (iotype == IOType.CheckDone)
            {
                res = (int)d2410_check_done(0);
            }
            else { throw new NotSupportedException("未支持的IO类型"); }

            return res;
        }

        public OperationResult<T> Read<T>(string funcCode) where T : struct
        {
            if (typeof(T) != typeof(int) && typeof(T) != typeof(ushort)) throw new NotSupportedException();
            var res = ReadIO(funcCode);
            return new OperationResult<T> { ErrorCode = 0, Content = PlcMotionController.BytesToStuct<T>(BitConverter.GetBytes(res)) };
        }

        #endregion

        #region interFunc

        private ushort GetIOStatus(ushort axis)
        {
            ushort uIOStatus;
            uIOStatus = d2410_axis_io_status(axis);
            return uIOStatus;
        }

        private void waitDone(ushort axis, bool bIsCheckOtherSignal = false)
        {
            do
            {
                if (CheckDone(axis))
                {
                    break;
                }
                Thread.Sleep(1);
            } while (true);
        }

        private bool CheckDone(ushort axis)
        {
            ushort uOnOff;
            uOnOff = d2410_check_done(axis);
            if (uOnOff == 1)
            {
                return true;//轴已停止
            }
            else
            {
                return false;//轴正在运行
            }
        }


        #endregion

        #region outerFunc
        public bool IsMinussLimit(ushort axis)
        {
            bool flag = false;
            ushort uMinusslimit;
            uMinusslimit = GetIOStatus(axis);
            flag = (0 != (uMinusslimit & 0x2000));//0x2000应该是负限位信号
            return flag;
        }

        public void AirSweep()
        {
            d2410_write_outbit(0, 2, 0);//吹扫开启
            Thread.Sleep(2000);
            d2410_write_outbit(0, 2, 1);//吹扫关闭
        }

        public void EMGStopMotion()
        {
            d2410_emg_stop();
            waitDone(0);//等待
        }

        #endregion

    }
    internal static class Dmc2410
    {
        //---------------------板卡初始和配置函数DMC2480 ----------------------
        /********************************************************************************
        ** 函数名称: d2410_board_init
        ** 功能描述: 控制板初始化，设置初始化和速度等设置
        ** 输　  入: 无
        ** 返 回 值: 0：无卡； 1-8：成功(实际卡数) 
        **           1001 + j: j号卡初始化出错 从1001开始。
        ** 修    改:  
        ** 修改日期: 
        *********************************************************************************/
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_board_init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2410_board_init();

        /********************************************************************************
        ** 函数名称: d2410_board_close
        ** 功能描述: 关闭所有卡
        ** 输　  入: 无
        ** 返 回 值: 无
        ** 日    期: 
        *********************************************************************************/
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_board_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_board_close();

        /********************************************************************************
        ** 函数名称: 控制卡复位
        ** 功能描述: 复位所有卡，只能在初始化完成之后调用．
        ** 输　  入: 无
        ** 返 回 值: 无
        ** 日    期: 
        *********************************************************************************/
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_board_rest", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_board_rest();

        //脉冲输入输出配置

        /// <summary>
        /// 设置指定轴的脉冲输出模式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="outmode">脉冲输出方式选择</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_pulse_outmode(ushort axis, ushort outmode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="outmode"></param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_pulse_outmode(ushort axis, ref ushort outmode);

        //专用信号设置函数   
        /// <summary>
        /// 设置指定轴的 INP 信号
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="enable">INP 信号使能， 0： 禁止， 1： 允许</param>
        /// <param name="inp_logic">INP 信号的有效电平， 0： 低有效， 1： 高有效</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_INP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_INP_PIN(ushort axis, ushort enable, ushort inp_logic);

        /// <summary>
        /// 设置 ALM 的有效电平及其工作方式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="alm_logic">ALM 信号的有效电平， 0： 低电平有效， 1： 高电平有效</param>
        /// <param name="alm_action">ALM 信号的制动方式， 0： 立即停止， 1： 减速停止(保留)</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_ALM_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_ALM_PIN(ushort axis, ushort alm_logic, ushort alm_action);

        /// <summary>
        /// 读取 ALM 的有效电平及其工作方式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="alm_logic">返回 ALM 信号有效电平</param>
        /// <param name="alm_action">返回 ALM 信号的制动方式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_config_ALM_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_config_ALM_PIN(ushort axis, ref ushort alm_logic, ref ushort alm_action);

        /// <summary>
        /// d2410_config_ALM_PIN 扩展函数，增加 ALM 使能状态、控制方式的设定
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="alm_enbale">ALM 信号使能状态， 0： 禁止， 1： 允许</param>
        /// <param name="alm_logic">ALM 信号的有效电平， 0： 低电平有效， 1： 高电平有效</param>
        /// <param name="alm_all">ALM 信号控制方式， 0：停止单轴， 1：停止所有轴</param>
        /// <param name="alm_action">ALM 信号的制动方式， 0： 立即停止， 1： 减速停止(保留)</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_ALM_PIN_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_ALM_PIN_Extern(ushort axis, ushort alm_enbale, ushort alm_logic, ushort alm_all, ushort alm_action);

        /// <summary>
        /// d2410_get_config_ALM_PIN 扩展函数，增加 ALM 使能状态、控制方式的读取
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="alm_enbale">返回 ALM 信号使能状态</param>
        /// <param name="alm_logic">返回 ALM 信号的有效电平</param>
        /// <param name="alm_all">返回 ALM 信号控制方式</param>
        /// <param name="alm_action">返回 ALM 信号的制动方式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_config_ALM_PIN_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_config_ALM_PIN_Extern(ushort axis, ref ushort alm_enbale, ref ushort alm_logic, ref ushort alm_all, ref ushort alm_action);

        /// <summary>
        /// 设置 EL 信号的有效电平及制动方式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="el_mode">EL 有效电平和制动方式：</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_EL_MODE", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_EL_MODE(ushort axis, ushort el_mode);

        /// <summary>
        /// 设置 ORG 信号的有效电平，以及允许/禁止滤波功能
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="org_logic">ORG 信号的有效电平， 0： 低电平有效， 1： 高电平有效</param>
        /// <param name="filter">允许/禁止滤波功能， 0： 禁止， 1： 允许</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_HOME_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_HOME_pin_logic(ushort axis, ushort org_logic, ushort filter);

        /// <summary>
        /// 控制指定轴的伺服使能端子的输出
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="on_off">设定管脚电平状态， 0： 低电平， 1： 高电平</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_write_SEVON_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_write_SEVON_PIN(ushort axis, ushort on_off);

        /// <summary>
        /// 读取指定轴的伺服使能端子的电平
        /// </summary>
        /// <param name="axis">axis 指定轴号， 取值范围： 0~3</param>
        /// <returns>伺服使能端子电平， 0： 低电平， 1： 高电平</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_SEVON_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_read_SEVON_PIN(ushort axis);

        /// <summary>
        /// 读取指定轴的 RDY 端子的电平
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>RDY 端子电平， 0： 低电平， 1： 高电平</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_RDY_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_read_RDY_PIN(ushort axis);

        /// <summary>
        /// 设置 EL 信号的使能状态
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="enable">EL 信号的使能状态， 0：不使能， 1：使能</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_Enable_EL_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_Enable_EL_PIN(ushort axis, ushort enable);


        //通用输入/输出控制函数

        /// <summary>
        /// 读取指定控制卡的某一位输入口的电平
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <param name="bitno">指定输入口位号， 取值范围： 1~32</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_read_inbit(ushort cardno, ushort bitno);

        /// <summary>
        /// 控制指定控制卡的某一位输出口的输出
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <param name="bitno">指定输出口位号， 取值范围： 1~32</param>
        /// <param name="on_off">输出电平， 0： 低电平， 1： 高电平</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_write_outbit(ushort cardno, ushort bitno, ushort on_off);

        /// <summary>
        /// 读取指定控制卡的某一位输出口的电平
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <param name="bitno">指定输出口位号， 取值范围： 1~32</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_read_outbit(ushort cardno, ushort bitno);

        /// <summary>
        /// 读取指定控制卡的全部通用输入口的电平状态
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_read_inport(ushort cardno);

        /// <summary>
        /// 读取指定控制卡的全部通用输出口的电平状态
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <returns>bit0~bit31 位值分别代表第 1~32 号输出端口值</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_read_outport(ushort cardno);

        /// <summary>
        /// 指定控制卡的全部通用输出口的电平状态
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <param name="port_value">bit0~bit19 位值分别代表第 1~20 号输出端口值</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_write_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_write_outport(ushort cardno, uint port_value);

        //制动函数
        /// <summary>
        /// 指定轴减速停止
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Tdec">保留参数，固定为 0</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_decel_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_decel_stop(ushort axis, double Tdec);

        /// <summary>
        /// 使指定轴立即停止，没有任何减速的过程
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_imd_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_imd_stop(ushort axis);

        /// <summary>
        /// 使所有的运动轴紧急停止
        /// </summary>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_emg_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_emg_stop();
        //相比2210此处少了一个方法 -- SUNWEI

        //位置设置和读取函数
        /// <summary>
        /// 读取指定轴的指令脉冲位置
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>指定轴的命令脉冲数，单位： pulse</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_get_position(ushort axis);

        /// <summary>
        /// 设置指定轴的指令脉冲位置
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="current_position">绝对位置值</param>
        /// <returns>错误代码</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_position(ushort axis, int current_position);

        /// <summary>
        /// 读取指定轴运动的目标脉冲位置
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>指定轴的目标脉冲位置，单位： pulse</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_get_target_position(ushort axis);

        //状态检测函数
        /// <summary>
        /// 检测指定轴的运动状态
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>0： 指定轴正在运行， 1： 指定轴已停止</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2410_check_done(ushort axis);

        /// <summary>
        /// 读取指定轴相关专用 IO 信号的状态
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_axis_io_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2410_axis_io_status(ushort axis);

        /// <summary>
        /// 读取指定轴的外部信号状态
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_rsts", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_rsts(ushort axis);

        //速度设置和读取函数         
        /// <summary>
        /// 设定插补运动速度曲线
        /// </summary>
        /// <param name="Min_Vel">保留参数</param>
        /// <param name="Max_Vel">最大速度，单位： pulse/s</param>
        /// <param name="Tacc">加速时间，单位： s</param>
        /// <param name="Tdec">减速时间，单位： s</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_vector_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_vector_profile(double Min_Vel, double Max_Vel, double Tacc, double Tdec);

        /// <summary>
        /// 设定梯形速度曲线
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Min_Vel">起始速度，单位： pulse/s</param>
        /// <param name="Max_Vel">最大速度，单位： pulse/s</param>
        /// <param name="Tacc">总加速时间，单位： s</param>
        /// <param name="Tdec">总减速时间，单位： s</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_profile(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec);

        /// <summary>
        /// d2410_set_profile 扩展函数，增加停止速度的设定
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Min_Vel">起始速度，单位： pulse/s</param>
        /// <param name="Max_Vel">最大速度，单位： pulse/s</param>
        /// <param name="Tacc">总加速时间，单位： s</param>
        /// <param name="Tdec">总减速时间，单位： s</param>
        /// <param name="Stop_Vel">停止速度，单位： pulse/s</param>
        /// <returns></returns>
	    [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_profile_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_profile_Extern(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);

        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_s_profile(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, int Sacc, int Sdec);

        /// <summary>
        /// 设定 S 形速度曲线
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Min_Vel">起始速度，单位： pulse/s</param>
        /// <param name="Max_Vel">最大速度，单位： pulse/s</param>
        /// <param name="Tacc">总加速时间，单位： s</param>
        /// <param name="Tdec">总减速时间，单位： s</param>
        /// <param name="Tsacc">S 段时间，单位： s， 范围[0,50] ms</param>
        /// <param name="Tsdec">保留参数</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_st_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_st_profile(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Tsacc, double Tsdec);

        /// <summary>
        /// 读取当前速度值
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>指定轴的速度，单位： pulse/s</returns>
	    [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_current_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern double d2410_read_current_speed(ushort axis);

        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_vector_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern double d2410_read_vector_speed(ushort card);

        /// <summary>
        /// d2410_set_st_profile 扩展函数，增加停止速度的设定
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Min_Vel">最大速度，单位： pulse/s</param>
        /// <param name="Max_Vel">最大速度，单位： pulse/s</param>
        /// <param name="Tacc">总加速时间，单位： s</param>
        /// <param name="Tdec">总减速时间，单位： s</param>
        /// <param name="Tsacc">S 段时间，单位： s， 范围[0,50] ms</param>
        /// <param name="Tsdec">保留参数</param>
        /// <param name="Stop_Vel">停止速度</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_st_profile_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_st_profile_Extern(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Tsacc, double Tsdec, double Stop_Vel);

        //在线变速/变位

        /// <summary>
        /// 在线改变指定轴的当前运动速度。该函数只适用于单轴运动中的变速
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Curr_Vel">新的运行速度，单位： pulse/s</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_change_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_change_speed(ushort axis, double Curr_Vel);

        /// <summary>
        /// 在单轴绝对运动中改变目标位置
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="dist">绝对位置值，单位： pulse</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_reset_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_reset_target_position(ushort axis, int dist);

        //单轴定长运动

        /// <summary>
        /// 使指定轴以对称梯形速度曲线做点位运动
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Dist">目标位置，单位： pulse</param>
        /// <param name="posi_mode">运动模式， 0： 相对坐标模式， 1： 绝对坐标模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_t_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_t_pmove(ushort axis, int Dist, ushort posi_mode);

        /// <summary>
        /// 使指定轴以非对称梯形速度曲线做点位运动
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Dist">目标位置，单位： pulse</param>
        /// <param name="posi_mode">运动模式， 0： 相对坐标模式， 1： 绝对坐标模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_ex_t_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_ex_t_pmove(ushort axis, int Dist, ushort posi_mode);

        /// <summary>
        /// 使指定轴以对称 S 形速度曲线做点位运动
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Dist">目标位置，单位： pulse</param>
        /// <param name="posi_mode">运动模式， 0： 相对坐标模式， 1： 绝对坐标模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_s_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_s_pmove(ushort axis, int Dist, ushort posi_mode);

        /// <summary>
        /// 使指定轴以非对称 S 形速度曲线做点位运动
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="Dist">目标位置，单位： pulse</param>
        /// <param name="posi_mode">运动模式， 0： 相对坐标模式， 1：绝对坐标模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_ex_s_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_ex_s_pmove(ushort axis, int Dist, ushort posi_mode);

        //单轴连续运动

        /// <summary>
        /// 使指定轴以 S 形速度曲线加速到高速，并持续运行下去
        /// </summary>
        /// <param name="axis">指指定轴号， 取值范围： 0~3</param>
        /// <param name="dir">指定运动的方向， 0： 负方向， 1： 正方向</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_s_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_s_vmove(ushort axis, ushort dir);

        /// <summary>
        /// 使指定轴以 T 形速度曲线加速到高速，并持续运行下去
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="dir">指定运动的方向， 0： 负方向， 1： 正方向</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_t_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_t_vmove(ushort axis, ushort dir);

        //直线插补
        /// <summary>
        /// 指定任意两轴以对称的梯形速度曲线做插补运动
        /// </summary>
        /// <param name="axis1">指定两轴插补的第一轴</param>
        /// <param name="Dist1">指定两轴插补的第二轴</param>
        /// <param name="axis2">指定第一轴的位移值，单位： pulse</param>
        /// <param name="Dist2">指定第二轴的位移值，单位： pulse</param>
        /// <param name="posi_mode">运动模式， 0： 相对坐标模式， 1： 绝对坐标模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_t_line2", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_t_line2(ushort axis1, int Dist1, ushort axis2, int Dist2, ushort posi_mode);

        /// <summary>
        /// 指定任意三轴以对称的梯形速度曲线做插补运动
        /// </summary>
        /// <param name="axis">轴号列表的指针</param>
        /// <param name="Dist1">指定 axis[0]轴的位移值，单位： pulse</param>
        /// <param name="Dist2">指定 axis[1]轴的位移值，单位： pulse</param>
        /// <param name="Dist3">指定 axis[2]轴的位移值，单位： pulse</param>
        /// <param name="posi_mode">运动模式， 0： 相对坐标模式， 1： 绝对坐标模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_t_line3", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_t_line3(ushort[] axis, int Dist1, int Dist2, int Dist3, ushort posi_mode);

        /// <summary>
        /// 指定四轴以对称的梯形速度曲线做插补运动
        /// </summary>
        /// <param name="cardno">指定插补运动的板卡号, 范围（ 0~7）</param>
        /// <param name="Dist1">指定第一轴的位移值，单位： pulse</param>
        /// <param name="Dist2">指定第二轴的位移值，单位： pulse</param>
        /// <param name="Dist3">指定第三轴的位移值，单位： pulse</param>
        /// <param name="Dist4">指定第四轴的位移值，单位： pulse</param>
        /// <param name="posi_mode">运动模式， 0： 相对坐标模式， 1： 绝对坐标模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_t_line4", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_t_line4(ushort cardno, int Dist1, int Dist2, int Dist3, int Dist4, ushort posi_mode);

        //圆弧插补
        /// <summary>
        /// 两轴圆弧插补运动，圆心位置+终点位置，绝对坐标模式
        /// </summary>
        /// <param name="axis">轴号列表指针</param>
        /// <param name="target_pos">目标绝对位置列表指针，单位： pulse</param>
        /// <param name="cen_pos">圆心绝对位置列表指针，单位： pulse</param>
        /// <param name="arc_dir">圆弧方向， 0： 顺时针， 1： 逆时针</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_arc_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_arc_move(ushort[] axis, int[] target_pos, int[] cen_pos, ushort arc_dir);

        /// <summary>
        /// 两轴圆弧插补运动，圆心位置+终点位置，相对坐标模式
        /// </summary>
        /// <param name="axis">轴号列表指针</param>
        /// <param name="rel_pos">目标相对位置列表指针, 单位： pulse</param>
        /// <param name="rel_cen">圆心相对位置列表指针, 单位： pulse</param>
        /// <param name="arc_dir">圆弧方向， 0： 顺时针， 1： 逆时针</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_rel_arc_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_rel_arc_move(ushort[] axis, int[] rel_pos, int[] rel_cen, ushort arc_dir);

        //手轮运动
        /// <summary>
        /// 设置输入手轮脉冲信号的计数方式
        /// </summary>
        /// <param name="axis">axis 指定轴号， 取值范围： 0~3</param>
        /// <param name="inmode">inmode 表示输入方式， 0： A、 B 相位正交计数， 1： 脉冲+方向</param>
        /// <param name="multi">手轮的倍率,正数表示默认方向， 负数表示与默认方向相反</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_handwheel_inmode(ushort axis, ushort inmode, double multi);

        /// <summary>
        /// 启动指定轴的手轮脉冲运动
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_handwheel_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_handwheel_move(ushort axis);

        //找原点
        /// <summary>
        /// 设定指定轴的回原点模式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="mode">回原点的信号模式</param>
        /// <param name="EZ_count">保留参数</param>
        /// <returns>错误代码</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_home_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_home_mode(ushort axis, ushort mode, ushort EZ_count);
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_config_home_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_config_home_mode(ushort axis, ushort mode, ushort EZ_count);

        /// <summary>
        /// 单轴回原点运动
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="home_mode">回原点方式， 1： 正方向回原点， 2： 负方向回原点</param>
        /// <param name="vel_mode">回原点速度， 0： 低速回原点， 1： 高速回原点</param>
        /// <returns>错误代码</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_home_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_home_move(ushort axis, ushort home_mode, ushort vel_mode);

        /// <summary>
        /// 设置回原点时遇限位开关是否自动反找
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="enable">使能是否遇限位反找， 0：不使能， 1：使能</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_home_el_return", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_home_el_return(ushort axis, ushort enable);

        //原点锁存
        /// <summary>
        /// 设置原点锁存方式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="enable">原点锁存使能状态， 0： 禁止， 1： 允许</param>
        /// <param name="logic">原点锁存方式， 0：下降沿锁存， 1：上升沿锁存</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_homelatch_mode(ushort axis, ushort enable, ushort logic);

        /// <summary>
        /// 读取原点锁存方式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="enable">返回原点锁存使能设置</param>
        /// <param name="logic">返回原点锁存方式设置</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_homelatch_mode(ushort axis, ref ushort enable, ref ushort logic);

        /// <summary>
        /// 读取原点锁存标志
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>原点锁存标志， 0：未触发， 1：触发</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_get_homelatch_flag(ushort axis);

        /// <summary>
        /// 复位原点锁存标志
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_reset_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_reset_homelatch_flag(ushort axis);

        /// <summary>
        /// 读取原点锁存值
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>原点锁存值</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_homelatch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_get_homelatch_value(ushort axis);

        //多组位置比较函数
        /// <summary>
        /// 设置一维位置比较器
        /// </summary>
        /// <param name="card">卡号</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <param name="enable">比较功能状态， 0：禁止比较功能， 1：使能比较功能</param>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="cmp_source">比较源， 0：比较指令位置， 1：比较编码器位置</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_config_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_compare_config_Extern(ushort card, ushort queue, ushort enable, ushort axis, ushort cmp_source);

        /// <summary>
        /// 读取一维位置比较器设置
        /// </summary>
        /// <param name="card">卡号</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <param name="enable">返回比较功能状态</param>
        /// <param name="axis">返回比较轴号</param>
        /// <param name="cmp_source">返回比较源</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_get_config_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_compare_get_config_Extern(ushort card, ushort queue, ref ushort enable, ref ushort axis, ref ushort cmp_source);

        /// <summary>
        /// 清除已添加的所有一维位置比较点
        /// </summary>
        /// <param name="card">卡号</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_clear_points_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_compare_clear_points_Extern(ushort card, ushort queue);

        /// <summary>
        /// 添加一维位置比较点
        /// </summary>
        /// <param name="card">卡号</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <param name="pos">位置坐标</param>
        /// <param name="dir">比较方向， 0： 小于等于， 1： 大于等于</param>
        /// <param name="action">比较点触发功能， 参数值见表 8.4</param>
        /// <param name="actpara">比较点触发功能参数， 参数值见表 8.4</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_add_point_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_compare_add_point_Extern(ushort card, ushort queue, uint pos, ushort dir, ushort action, uint actpara);

        /// <summary>
        /// 读取当前一维比较点位置
        /// </summary>
        /// <param name="card">卡号</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_get_current_point_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_compare_get_current_point_Extern(ushort card, ushort queue);

        /// <summary>
        /// 查询已经比较过的点个数
        /// </summary>
        /// <param name="card">卡号</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_get_points_runned_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_compare_get_points_runned_Extern(ushort card, ushort queue);

        /// <summary>
        /// 查询可以加入的一维比较点数量
        /// </summary>
        /// <param name="card">卡号</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <param name="queue">比较队列号，取值范围： 0、 1</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_get_points_remained_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_compare_get_points_remained_Extern(ushort card, ushort queue);

        //-------------------二维低速位置比较-----------------------
        /// <summary>
        /// 设置二维位置比较器
        /// </summary>
        /// <param name="CardNo">控制卡卡号</param>
        /// <param name="enable">二维位置比较功能状态， 0： 禁止， 1： 使能</param>
        /// <param name="cmp_source">二维位置比较源， 0： 指令位置计数器， 1：编码器计数器</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_2d_set_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short d2410_compare_2d_set_config(ushort CardNo, ushort enable, ushort cmp_source);

        /// <summary>
        /// 读取二维位置比较器设置
        /// </summary>
        /// <param name="CardNo">控制卡卡号</param>
        /// <param name="enable">返回比较功能状态</param>
        /// <param name="cmp_source">返回比较源</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_2d_get_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short d2410_compare_2d_get_config(ushort CardNo, ref ushort enable, ref ushort cmp_source);

        /// <summary>
        /// 清除已添加的所有二维位置比较点
        /// </summary>
        /// <param name="CardNo">控制卡卡号</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_2d_clear_points", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short d2410_compare_2d_clear_points(ushort CardNo);

        /// <summary>
        /// 添加二维位置比较点
        /// </summary>
        /// <param name="CardNo">控制卡卡号</param>
        /// <param name="axis">指定卡上的即将进行位置比较的轴列表(两个轴)</param>
        /// <param name="pos">二维位置比较位置列表，单位： pulse</param>
        /// <param name="dir">比较模式列表， 0： 小于等于， 1： 大于等于</param>
        /// <param name="action">二维位置比较点触发功能编号，见表 8.6</param>
        /// <param name="actpara">二维位置比较点触发功能参数，见表 8.6</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_2d_add_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short d2410_compare_2d_add_point(ushort CardNo, ushort[] axis, int[] pos, ushort[] dir, ushort action, uint actpara);

        /// <summary>
        /// 读取当前二维位置比较点位置
        /// </summary>
        /// <param name="CardNo">控制卡卡号</param>
        /// <param name="pos">返回当前二维位置比较点位置，单位： pulse</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_2d_get_current_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short d2410_compare_2d_get_current_point(ushort CardNo, ref int pos);

        /// <summary>
        /// 查询已经比较过的二维比较点个数
        /// </summary>
        /// <param name="CardNo">控制卡卡号</param>
        /// <param name="pointNum">返回已经比较过的二维位置比较点数</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_2d_get_points_runned", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short d2410_compare_2d_get_points_runned(ushort CardNo, ref int pointNum);

        /// <summary>
        /// 查询可以加入的二维比较点个数
        /// </summary>
        /// <param name="CardNo">控制卡卡号</param>
        /// <param name="pointNum">返回可以加入的二维位置比较点数</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_compare_2d_get_points_remained", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short d2410_compare_2d_get_points_remained(ushort CardNo, ref int pointNum);

        //高速位置比较
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_CMP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_CMP_PIN(ushort axis, ushort cmp_enable, int cmp_pos, ushort CMP_logic);
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_config_CMP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_config_CMP_PIN(ushort axis, ref ushort cmp_enable, ref int cmp_pos, ref ushort CMP_logic);
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_read_CMP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_read_CMP_PIN(ushort axis);
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_write_CMP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_write_CMP_PIN(ushort axis, ushort on_off);

        //编码器计数功能
        /// <summary>
        /// 读取指定轴编码器反馈位置脉冲计数值
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2410_get_encoder(ushort axis);

        /// <summary>
        /// 设置指定轴编码器反馈脉冲计数值
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="encoder_value">编码器计数值，单位： pulse</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_encoder(ushort axis, uint encoder_value);

        /// <summary>
        /// 设置指定轴的 EZ 信号的有效电平及其作用
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="ez_logic">EZ 信号有效电平， 0：高有效， 1：低有效</param>
        /// <param name="ez_mode">保留参数</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_EZ_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_EZ_PIN(ushort axis, ushort ez_logic, ushort ez_mode);

        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_counter_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_counter_flag(ushort cardno);

        [DllImport("Dmc2410.dll", EntryPoint = "d2410_reset_counter_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_reset_counter_flag(ushort cardno);

        [DllImport("Dmc2410.dll", EntryPoint = "d2410_reset_clear_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_reset_clear_flag(ushort cardno);

        //高速锁存
        /// <summary>
        /// 设置指定轴 LTC 信号的有效电平及其工作方式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="ltc_logic">LTC 信号有效电平， 0： 低有效， 1： 高有效</param>
        /// <param name="ltc_mode">保留参数</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_LTC_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_LTC_PIN(ushort axis, ushort ltc_logic, ushort ltc_mode);


        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_config_EZ_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_config_EZ_PIN(ushort axis, ref ushort ltc_logic, ref ushort ltc_mode);

        /// <summary>
        /// 2410_config_LTC_PIN 扩展函数，增加滤波时间的设定
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="ltc_logic">LTC 信号有效电平， 0： 低有效， 1： 高有效</param>
        /// <param name="ltc_mode">保留参数</param>
        /// <param name="ltc_filter">滤波时间，单位： ms</param>
        /// <returns></returns>
	    [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_LTC_PIN_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_LTC_PIN_Extern(ushort axis, ushort ltc_logic, ushort ltc_mode, double ltc_filter);

        /// <summary>
        /// d2410_get_config_LTC_PIN 扩展函数，增加滤波时间的读取
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="ltc_logic">返回 LTC 信号有效电平</param>
        /// <param name="ltc_mode">保留参数</param>
        /// <param name="ltc_filter">返回滤波时间</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_config_LTC_PIN_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_config_LTC_PIN_Extern(ushort axis, ref ushort ltc_logic, ref ushort ltc_mode, ref double ltc_filter);

        /// <summary>
        /// 设置 LTC 锁存方式
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <param name="all_enable">锁存方式， 0： 单独锁存， 1： 四轴同时锁存</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_latch_mode(ushort cardno, ushort all_enable);

        /// <summary>
        /// 设置编码器的计数方式
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="mode">编码器的计数方式：</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_counter_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_counter_config(ushort axis, ushort mode);

        /// <summary>
        /// 读取编码器锁存器的值
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_latch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_latch_value(ushort axis);

        /// <summary>
        /// 读取指定控制卡的锁存器的标志位
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_latch_flag(ushort cardno);

        /// <summary>
        /// 复位指定控制卡的锁存器的标志位
        /// </summary>
        /// <param name="cardno">指定控制卡号</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_reset_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_reset_latch_flag(ushort cardno);

        /// <summary>
        /// 选择全部锁存时的外触发信号通道
        /// </summary>
        /// <param name="cardno">k卡号</param>
        /// <param name="num">信号通道选择号， 0： LTC1 锁存四个轴（默认值）， 1： LTC2 锁存四个轴</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_triger_chunnel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_triger_chunnel(ushort cardno, ushort num);

        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_speaker_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_speaker_logic(ushort cardno, ushort logic);

        //EMG设置
        /// <summary>
        /// EMG 信号设置，急停信号有效后会立即停止所有轴
        /// </summary>
        /// <param name="cardno">卡号</param>
        /// <param name="enable">保留参数</param>
        /// <param name="emg_logic">有效电平， 0：低有效， 1： 高有效</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_EMG_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_EMG_PIN(ushort cardno, ushort enable, ushort emg_logic);

        //软件限位功能
        /// <summary>
        /// 设置软件限位
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="ON_OFF">使能状态， 0：禁止， 1：允许</param>
        /// <param name="source_sel">保留参数，固定值为 0</param>
        /// <param name="SL_action">限位制动方式： 0：立即停止， 1：减速停止</param>
        /// <param name="N_limit">负限位脉冲数</param>
        /// <param name="P_limit">正限位脉冲数</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_config_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_config_softlimit(ushort axis, ushort ON_OFF, ushort source_sel, ushort SL_action, int N_limit, int P_limit);

        /// <summary>
        /// 读取软件限位设置
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="ON_OFF">返回使能状态设置</param>
        /// <param name="source_sel">保留参数</param>
        /// <param name="SL_action">返回限位制动方式设置</param>
        /// <param name="N_limit">返回负限位脉冲数设置</param>
        /// <param name="P_limit">返回正限位脉冲数设置</param>
        /// <returns></returns>
	    [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_config_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_config_softlimit(ushort axis, ref ushort ON_OFF, ref ushort source_sel, ref ushort SL_action, ref int N_limit, ref int P_limit);

        /// <summary>
        /// 设置位置误差带
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="factor">编码器系数（脉冲当量）</param>
        /// <param name="error">位置误差带</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_factor_error(ushort axis, double factor, int error);

        /// <summary>
        /// 读取位置误差带设置
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <param name="factor">返回编码器系数（脉冲当量）</param>
        /// <param name="error">返回位置误差带</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_get_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_get_factor_error(ushort axis, ref double factor, ref int error);

        /// <summary>
        /// 检测指令到位
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns>0：指令位置在设定的目标位置的误差带之外；1：指令位置在设定的目标位置的误差带之内</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_check_success_pulse", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_check_success_pulse(ushort axis);

        /// <summary>
        /// 检测指令到位
        /// </summary>
        /// <param name="axis">指定轴号， 取值范围： 0~3</param>
        /// <returns> 0：编码器位置在设定的目标位置的误差带之外；1：编码器位置在设定的目标位置的误差带之内</returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_check_success_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_check_success_encoder(ushort axis);

        /// <summary>
        /// 实现 profile， pmove 整合，缩短指令时间， 适用于高速场合
        /// </summary>
        /// <param name="axis">指定轴号</param>
        /// <param name="dist">终点位置,单位： pulse</param>
        /// <param name="Min_Vel">起始速度</param>
        /// <param name="Max_Vel">最大速度</param>
        /// <param name="Tacc">加速时间</param>
        /// <param name="Tdec">减速时间</param>
        /// <param name="stop_Vel">停止速度</param>
        /// <param name="s_para">平滑时间， 单位： s， 范围[0~0.5]</param>
        /// <param name="posi_mode">运动模式， 0： 相对模式， 1： 绝对模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_pmove_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2410_pmove_extern(ushort axis, double dist, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_Vel, double s_para, ushort posi_mode);

        /// <summary>
        /// 实现 profile， pmove 整合，缩短指令时间，并且实现软着陆
        /// </summary>
        /// <param name="axis">指定轴号</param>
        /// <param name="MidPos">第一段的终点位置,单位： pulse</param>
        /// <param name="TargetPos">第二段的终点位置,单位： pulse</param>
        /// <param name="Min_Vel">起始速度</param>
        /// <param name="Max_Vel">最大速度</param>
        /// <param name="stop_Vel">停止速度</param>
        /// <param name="acc">加速时间</param>
        /// <param name="dec">减速时间</param>
        /// <param name="posi_mode">运动模式， 0：相对模式， 1：绝对模式</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_t_pmove_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2410_t_pmove_extern(ushort axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, ushort posi_mode);

        /// <summary>
        /// 强行改变指定轴的当前目标位置并且实现软着陆
        /// </summary>
        /// <param name="axis">指定轴号</param>
        /// <param name="mid_pos">第一段的终点位置,单位： pulse</param>
        /// <param name="aim_pos">第二段的终点位置,单位： pulse</param>
        /// <param name="vel">保留参数，固定值为 0</param>
        /// <param name="posi_mode">保留参数，固定值为 0</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_update_target_position_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2410_update_target_position_extern(ushort axis, double mid_pos, double aim_pos, double vel, ushort posi_mode);

        /// <summary>
        /// 设定插补运动速度曲线
        /// </summary>
        /// <param name="cardno">板卡号, 范围（ 0~7）</param>
        /// <param name="Min_Vel">最小速度，单位： pulse/s</param>
        /// <param name="Max_Vel">最大速度，单位： pulse/s</param>
        /// <param name="Tacc">加速时间，单位： s</param>
        /// <param name="Tdec">减速时间，单位： s</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "d2410_set_vector_profile_Extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2410_set_vector_profile_Extern(ushort cardno, double Min_Vel, double Max_Vel, double Tacc, double Tdec);

        /// <summary>
        /// 获取控制卡硬件 ID 号
        /// </summary>
        /// <param name="CardNum">返回初始化成功的卡数</param>
        /// <param name="CardTypeList">返回控制卡固件类型数组</param>
        /// <param name="CardIdList">返回控制卡硬件 ID 号数组，卡号按从小到大顺序排列</param>
        /// <returns></returns>
        [DllImport("Dmc2410.dll", EntryPoint = "dmc_get_CardInfList", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_CardInfList(ref ushort CardNum, uint[] CardTypeList, ushort[] CardIdList);
    }
}
