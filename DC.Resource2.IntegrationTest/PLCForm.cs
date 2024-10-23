using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DC.Resource2.IntegrationTest
{

    public partial class PLCForm : Form
    {
        public PLCForm()
        {
            InitializeComponent();
        }

        OperationResult result = OperationResult.Success;
        IMotionController controller;


        private ILogger InitalLog()
        {
            string logPath = @"..\Log\";
            var Infopath = $"{logPath}/Info";
            var Debugpath = $"{logPath}/Debug";
            var Errorpath = $"{logPath}/Error";
            var Warnpath = $"{logPath}/Warn";
            var Fatalpath = $"{logPath}/Fatal";
            if (!Directory.Exists(Infopath))
                Directory.CreateDirectory(Infopath);
            if (!Directory.Exists(Debugpath))
                Directory.CreateDirectory(Debugpath);
            if (!Directory.Exists(Errorpath))
                Directory.CreateDirectory(Errorpath);
            if (!Directory.Exists(Warnpath))
                Directory.CreateDirectory(Warnpath);
            if (!Directory.Exists(Fatalpath))
                Directory.CreateDirectory(Fatalpath);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()//指定最小日志级别
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Async(
                    a => a.File($"{Debugpath}/debuglog.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(90)), bufferSize: 512))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Async(
                    a => a.File($"{Infopath}/infolog.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(90)), bufferSize: 512))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.Async(
                    a => a.File($"{Warnpath}/warnlog.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(90)), bufferSize: 512))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Async(
                    a => a.File($"{Errorpath}/errorlog.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(90)), bufferSize: 512))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.Async(
                    a => a.File($"{Fatalpath}/fatallog.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(90)), bufferSize: 512))
                .CreateLogger();
            return Log.Logger;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IEquipmentMotionMechanismRepository equipmentMotionMechanismRepository = new EquipmentMotionMechanismDbRepository();
            IAddressRepository addressCatalog = new AddressCatalogDbRepository();
            PlcControllerFactory plcControllerFactory = new PlcControllerFactory();
            ILogger logger = InitalLog();

            MotionControllerFactory motion = new MotionControllerFactory(equipmentMotionMechanismRepository, addressCatalog, plcControllerFactory, logger);
            controller = motion.Create();
            Console.WriteLine("Init OK");

            result = controller.Initialize(1000f);
            if (result.Succeeded == true)
            {
                richTextBox1.Text += "连接成功\r\n";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            result = controller.GoHome();
            if (result.Succeeded == true)
            {
                richTextBox1.Text += "回原成功\r\n";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            result = controller.Stop();
            if (result.Succeeded == true)
            {
                richTextBox1.Text += "急停成功\r\n";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int axisId = 1;
            int.TryParse(txtDist.Text, out var distance);
            int nPulsePos = distance * 100;
            int acc = 100;
            int dec = 100;
            int speed = 50 * 100;
            MotionType motionType = MotionType.Relative;
            result = controller.Move(axisId, nPulsePos, acc, dec, speed, motionType);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ts = DateTime.Now;
            controller.Read<short>("Axis");
            var end = DateTime.Now;
            richTextBox1.Text += $"{(DateTime.Now - ts).TotalMilliseconds}" + "\r\n";


        }

        private void button6_Click(object sender, EventArgs e)
        {
            return;
            ILogger logger = InitalLog();
            ResouceDbMigration resouceDb = new ResouceDbMigration(logger, "Data Source=../settings/resource.db");
            resouceDb.Migrate();

            EquipmentMotionMechanismDbRepository eqAdd = new EquipmentMotionMechanismDbRepository();
            IEquipmentMotionMechanismRepository equipmentMotionMechanismRepository = new EquipmentMotionMechanismMemoryRepository();
            var equipments = equipmentMotionMechanismRepository.List();
            foreach (var item in equipments)
            {
                eqAdd.Add(item);
            }

            AddressCatalogDbRepository dbAddressCatalog = new AddressCatalogDbRepository();
            IAddressRepository addressCatalog = new MemoryAddressCatalog();
            var address = addressCatalog.List();

            foreach (var item in address)
            {
                item.Id = 1;
                dbAddressCatalog.Add(item);
            }

        }

        private void btnShowAddrConfig_Click(object sender, EventArgs e)
        {
            var form = new MotionConfigForm();
            form.ShowDialog();
        }
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
}
