using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DC.Resource2.IntegrationTest
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Migrate();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PLCForm());
        }

        static void Migrate()
        {
            var migration = new ResouceDbMigration(null);
            migration.Migrate();


            var mmRepo = new EquipmentMotionMechanismDbRepository();
            mmRepo.Clear();
            var id = mmRepo.Add(new MotionMechanism
            {
                Code = "plc01",
                IpAddress = "127.0.0.1",
                Oem = OEM.PlcInovance,
                MechanismType = MechanismType.Plc,
                Port = 402,
                Protocol = Protocol.ModbusTcp,
                Series = PlcControllerFactory.SupportedSeries[OEM.PlcInovance].First()
            });


            var repository = new AddressCatalogDbRepository();
            repository.Clear();
            repository.Add(new AddressRecord
            {
                Address = "d301",
                AxisId = 10,
                FuncCode = "xxxx",
                IOType = IOType.Axis,
                IsEnable = true,
                MechanismId = id
            });
            repository.Add(new AddressRecord
            {
                Address = "d302",
                AxisId = 1,
                FuncCode = "yyyy",
                IOType = IOType.Rsts,
                IsEnable = true,
                MechanismId = id
            });


        }
    }
}
