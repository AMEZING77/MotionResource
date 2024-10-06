using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static DC.Resource2.Dmc2210;

namespace DC.Resource2
{
    public class Dmc2210MotionController : DmcMotionControllerBase, IMotionController
    {
        public Dmc2210MotionController(IAddressRepository addressCatalog)
            : base(addressCatalog) { }
        #region Impl

        public OperationResult Initialize(float pulsePerMM)
        {
            base.Initialize();
            _pulsePerMM = pulsePerMM;
            ushort sCardCount = d2210_board_init();
            if (sCardCount == 0)
            {
                return new OperationResult { ErrorCode = -5, Succeeded = false, Message = "没有找到DMC2420-PIC控制卡!" };
            }
            else
            {
                //新机
                ushort axis = 0;
                d2210_config_EMG_PIN(0, 1, 1);
                d2210_set_pulse_outmode(axis, 0);//设置脉冲输出模式
                d2210_config_EL_MODE(axis, 1);//设置限位信号立即停，高电平有效
                d2210_config_ALM_PIN(axis, 1, 0);//设置报警信号立即停，高电平有效
            }
            return OperationResult.Success;
        }

        public OperationResult Close()
        {
            d2210_board_close();
            return OperationResult.Success;
        }

        public OperationResult Move(int axisId, int nPulsePos, int acc, int dec, int speed, MotionType motionType)
        {
            ushort axis = (ushort)axisId;
            d2210_set_profile(axis, Min_Vel, speed, Tacc, Tdec);
            d2210_t_pmove(axis, nPulsePos, 1);//绝对运动
            return OperationResult.Success;
        }

        public OperationResult GoHome(int axisId = 0)
        {
            //雷塞必须手动判定当前位置，再决定指令“方向”
            //需要手写PLC的找原点逻辑
            //先回负限位，再向前运动一段距离超过原点，再反向回原点
            //整个过程中，如果触发外部急停、门禁。。信号，需要立即终止；

            ushort axis = (ushort)axisId;
            d2210_set_profile(axis, Min_Vel, Max_Vel, Tacc, Tdec);
            d2210_t_vmove(axis, 0);//1、回负限位，负方向运动
            do
            {
                if (IsMinussLimit(axis))
                {
                    d2210_decel_stop(axis, Tdec);
                    waitDone(axis);
                    break;
                }
                Thread.Sleep(1);
            } while (true);//2、检测到负限位信号，先缓停，再判断轴是否停止

            d2210_t_pmove(axis, (int)MMToPulse(100), 0);//3、相对运动一定距离，超过原点，默认100mm
            waitDone(axis);
            d2210_home_move(axis, 2, 0);//4、反向回原点
            waitDone(axis);
            d2210_set_position(0, 0);//设置绝对位置为0，清零
            return OperationResult.Success;
        }

        public OperationResult Stop(int axisId = 0)
        {
            ushort axis = (ushort)axisId;
            d2210_decel_stop(axis, Tdec);
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
            d2210_write_outbit(0, bitNo, value);
            return OperationResult.Success;
        }

        public OperationResult Write<T>(string funcCode, T payload) where T : struct
        {
            if (typeof(T) != typeof(short)) throw new NotSupportedException();
            var (bitNo, iotype) = GetBitNo(funcCode);
            d2210_write_outbit(0, bitNo, Convert.ToUInt16(payload));
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
                //res = d2210_read_inbit(0, bitNo);
                res = d2210_read_inport(0);
            }
            else if (iotype == IOType.Axis)
            {
                res = d2210_axis_io_status(0);
            }
            else if (iotype == IOType.Rsts)
            {
                res = (int)d2210_get_rsts(0);
            }
            else if (iotype == IOType.Pos)
            {
                res = (int)d2210_get_position(0);
            }
            else if (iotype == IOType.CheckDone)
            {
                res = (int)d2210_check_done(0);
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
            uIOStatus = d2210_axis_io_status(axis);
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
            uOnOff = d2210_check_done(axis);
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
            d2210_write_outbit(0, 2, 0);//吹扫开启
            Thread.Sleep(2000);
            d2210_write_outbit(0, 2, 1);//吹扫关闭
        }

        public void EMGStopMotion()
        {
            d2210_emg_stop();
            waitDone(0);//等待
        }

        #endregion

    }

    internal static class Dmc2210
    {
        //---------------------   板卡初始和配置函数  ----------------------
        //修改日期： 2013/3/21
        /********************************************************************************
        ** 函数名称: d2210_board_init
        ** 功能描述: 控制板初始化，设置初始化和速度等设置
        ** 输　  入: 无
        ** 返 回 值: 0：无卡； 1-8：成功(实际卡数) 
        **           1001 + j: j号卡初始化出错 从1001开始。
        ** 修    改:  
        ** 修改日期: 
        *********************************************************************************/
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_board_init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2210_board_init();

        /********************************************************************************
        ** 函数名称: d2210_board_close
        ** 功能描述: 关闭所有卡
        ** 输　  入: 无
        ** 返 回 值: 无
        ** 日    期: 
        *********************************************************************************/
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_board_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_board_close();

        //脉冲输入输出配置
        /********************************************************************************
        ** 函数名称: d2210_set_pulse_outmode
        ** 功能描述: 脉冲输出方式的设置
        ** 输　  入: axis - (0 - 3), outmode: 0 - 7
        **          
        ** 返 回 值: 无 
        ** 修改日期：2007.1.27
        *********************************************************************************/
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_pulse_outmode(ushort axis, ushort outmode);

        //专用信号设置函数
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_SD_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_SD_PIN(ushort axis, ushort enable, ushort sd_logic, ushort sd_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_PCS_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_PCS_PIN(ushort axis, ushort enable, ushort pcs_logic);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_INP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_INP_PIN(ushort axis, ushort enable, ushort inp_logic);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_ERC_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_ERC_PIN(ushort axis, ushort enable, ushort erc_logic, ushort erc_width, ushort erc_off_time);

        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_ALM_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_ALM_PIN(ushort axis, ushort alm_logic, ushort alm_action);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_EL_MODE", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_EL_MODE(ushort axis, ushort el_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_HOME_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_HOME_pin_logic(ushort axis, ushort org_logic, ushort filter);

        [DllImport("Dmc2210.dll", EntryPoint = "d2210_write_SEVON_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_write_SEVON_PIN(ushort axis, ushort on_off);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_SEVON_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_read_SEVON_PIN(ushort axis);

        [DllImport("Dmc2210.dll", EntryPoint = "d2210_write_ERC_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_write_ERC_PIN(ushort axis, ushort sel);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_RDY_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_read_RDY_PIN(ushort axis);

        //通用输入/输出控制函数
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_read_inbit(ushort cardno, ushort bitno);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_write_outbit(ushort cardno, ushort bitno, ushort on_off);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_read_outbit(ushort cardno, ushort bitno);

        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_read_inport(ushort cardno);

        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_read_outport(ushort cardno);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_write_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_write_outport(ushort cardno, uint port_value);

        //制动函数
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_decel_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_decel_stop(ushort axis, double Tdec);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_imd_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_imd_stop(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_emg_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_emg_stop();
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_simultaneous_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_simultaneous_stop(ushort axis);

        //位置设置和读取函数
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_get_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_get_position(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_position(ushort axis, int current_position);

        //状态检测函数
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2210_check_done(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_prebuff_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2210_prebuff_status(ushort axis);

        [DllImport("Dmc2210.dll", EntryPoint = "d2210_axis_io_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2210_axis_io_status(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_axis_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort d2210_axis_status(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_get_rsts", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2210_get_rsts(ushort axis);

        //速度设置
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_variety_speed_range", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_variety_speed_range(ushort axis, ushort chg_enable, double Max_Vel);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_current_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern double d2210_read_current_speed(ushort axis);

        [DllImport("Dmc2210.dll", EntryPoint = "d2210_change_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_change_speed(ushort axis, double Curr_Vel);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_vector_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_vector_profile(double Min_Vel, double Max_Vel, double Tacc, double Tdec);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_profile(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_s_profile(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, int Sacc, int Sdec);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_st_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_st_profile(ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Tsacc, double Tsdec);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_reset_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_reset_target_position(ushort axis, int dist);

        //单轴定长运动
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_t_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_t_pmove(ushort axis, int Dist, ushort posi_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_ex_t_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_ex_t_pmove(ushort axis, int Dist, ushort posi_mode);


        [DllImport("Dmc2210.dll", EntryPoint = "d2210_s_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_s_pmove(ushort axis, int Dist, ushort posi_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_ex_s_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_ex_s_pmove(ushort axis, int Dist, ushort posi_mode);

        //单轴连续运动
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_s_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_s_vmove(ushort axis, ushort dir);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_t_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_t_vmove(ushort axis, ushort dir);

        //线性插补
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_t_line2", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_t_line2(ushort axis1, int Dist1, ushort axis2, int Dist2, ushort posi_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_t_line3", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_t_line3(ref ushort axis, int Dist1, int Dist2, int Dist3, ushort posi_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_t_line4", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_t_line4(ushort cardno, int Dist1, int Dist2, int Dist3, int Dist4, ushort posi_mode);


        //手轮运动
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_handwheel_inmode(ushort axis, ushort inmode, ushort count_dir);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_handwheel_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_handwheel_move(ushort axis, double vh);

        //找原点
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_home_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_home_mode(ushort axis, ushort mode, ushort EZ_count);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_home_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_home_move(ushort axis, ushort home_mode, ushort vel_mode);

        //圆弧插补
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_arc_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_arc_move(ref ushort axis, ref int target_pos, ref int cen_pos, ushort arc_dir);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_rel_arc_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_rel_arc_move(ref ushort axis, ref int rel_pos, ref int rel_cen, ushort arc_dir);

        //不同脉冲当量的圆弧插补
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_get_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_get_equiv(ushort axis, ref double equiv);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_set_equiv(ushort axis, double new_equiv);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_arc_move_unitmm", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_arc_move_unitmm(ref ushort axis, ref double target_pos, ref double cen_pos, ushort arc_dir);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_rel_arc_move_unitmm", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_rel_arc_move_unitmm(ref ushort axis, ref double rel_pos, ref double rel_cen, ushort arc_dir);

        //设置和读取位置比较信号
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_CMP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_CMP_PIN(ushort axis, ushort cmp1_enable, ushort cmp2_enable, ushort CMP_logic);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_read_CMP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_read_CMP_PIN(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_write_CMP_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_write_CMP_PIN(ushort axis, ushort on_off);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_comparator", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_comparator(ushort axis, ushort cmp1_condition, ushort cmp2_condition, ushort source_sel, ushort SL_action);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_comparator_data", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_comparator_data(ushort axis, uint cmp1_data, uint cmp2_data);



        //---------------------   编码器计数功能PLD  ----------------------//
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_get_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2210_get_encoder(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_encoder(ushort axis, uint encoder_value);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_EZ_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_EZ_PIN(ushort axis, ushort ez_logic, ushort ez_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_LTC_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_LTC_PIN(ushort axis, ushort ltc_logic, ushort ltc_mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_latch_mode(ushort cardno, ushort all_enable);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_counter_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_counter_config(ushort axis, ushort mode);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_get_latch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2210_get_latch_value(ushort axis);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_get_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2210_get_latch_flag(ushort cardno);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_reset_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_reset_latch_flag(ushort cardno);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_get_counter_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint d2210_get_counter_flag(ushort cardno);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_reset_counter_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_reset_counter_flag(ushort cardno);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_reset_clear_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_reset_clear_flag(ushort cardno);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_triger_chunnel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_triger_chunnel(ushort cardno, ushort num);
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_speaker_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_set_speaker_logic(ushort cardno, ushort logic);

        //other
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_config_EMG_PIN", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void d2210_config_EMG_PIN(ushort cardno, ushort enable, ushort emg_logic);


        //增加同时起停操作
        /********************************************************************************
        ** 函数名称: d2210_set_t_move_all
        ** 功能描述: 多轴同步运动设定
        ** 输　  入: TotalAxes: 轴数,  pAxis:轴列表, pDist:位移列表
                     posi_mode: 0-相对, 1-绝对
        ** 返 回 值: 1:正确 , -1:参数错
        ** 
        ** 全局变量: 无
        ** 修改内容: 
        ** 修改日期:   
        *********************************************************************************/
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_t_move_all", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_set_t_move_all(ushort TotalAxes, ref ushort pAxis, ref uint pDist, ushort posi_mode);

        /********************************************************************************
        ** 函数名称: d2210_start_move_all
        ** 功能描述: 多轴同步运动
        ** 输　  入: TotalAxes: 第一轴轴号
        ** 返 回 值: 1:正确 , -1:参数错
        ** 
        ** 全局变量: 无
        ** 修改内容: 
        ** 修改日期:      
        *********************************************************************************/
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_start_move_all", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_start_move_all(ushort FirstAxis);

        /********************************************************************************
        ** 函数名称: d2210_set_sync_option
        ** 功能描述: 多轴同步选项设定, 注意: 使用后必须关闭此功能, 将sync_option1清0.
        ** 输　  入: axis:轴号
                     sync_stop_on: 1:当CSTOP 信号来时,轴停止; 
                     cstop_output_on: 当异常停止时输出 CSTOP信号
                     sync_option1: 0:立即启动, 1: 等待CSTA信号 或是启动命令 
                     sync_option2: 无用
        ** 返 回 值: 1:正确 , -1:参数错
        ** 
        ** 全局变量: 无
        ** 修改内容: 
        ** 修改日期:     
        *********************************************************************************/
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_sync_option", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_set_sync_option(ushort axis, ushort sync_stop_on, ushort cstop_output_on, ushort sync_option1, ushort sync_option2);

        /********************************************************************************
        ** 函数名称: d2210_set_sync_stop_mode
        ** 功能描述: 设置同步停止的减速方式
        ** 输　  入: axis: 轴号
                     stop_mode:  0- 立即停止, 1-减速停止
        ** 返 回 值: 1:正确 , -1:参数错
        ** 
        ** 全局变量: 无
        ** 修改内容: 
        ** 修改日期:      
        *********************************************************************************/
        [DllImport("Dmc2210.dll", EntryPoint = "d2210_set_sync_stop_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int d2210_set_sync_stop_mode(ushort axis, ushort stop_mode);
    }
}
