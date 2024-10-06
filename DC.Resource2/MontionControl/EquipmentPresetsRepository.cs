using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class EquipmentPresetsRepository
    {
        public EquipmentPresetsRepository() { }

        public List<Equipment> ListEuqipments()
        {
            var conn = new SQLiteConnection(Constants.dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT code, name, description from preset_equipment_catalog";
                var reader = cmd.ExecuteReader();
                var result = new List<Equipment>();
                while (reader.Read())
                {
                    result.Add(new Equipment()
                    {
                        Code = reader.GetString(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                    });
                }

                return result;
            }
            finally
            {
                conn.Close();
            }
        }

        public EquipomentMotionPreset Get(string equipCode)
        {
            var conn = new SQLiteConnection(Constants.dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"SELECT mechanism_type,oem,protocol,series,code,ip_address,port,id 
FROM equipment_motion_mechanism WHERE equipment_code=@equipCode";
                cmd.Parameters.Add(new SQLiteParameter("@equipCode", equipCode));
                var reader = cmd.ExecuteReader();

                var res = new EquipomentMotionPreset();
                while (reader.Read())
                {
                    res.Mechanisms.Add(new MotionMechanism
                    {
                        MechanismType = (MechanismType)Enum.Parse(typeof(MechanismType), reader.GetString(0)),
                        Oem = (OEM)Enum.Parse(typeof(OEM), reader.GetString(1)),
                        Protocol = (Protocol)Enum.Parse(typeof(Protocol), reader.GetString(2)),
                        Series = reader.GetString(3),
                        Code = reader.GetString(4),
                        IpAddress = reader.GetString(5),
                        Port = (ushort)reader.GetInt32(6),
                        Id = reader.GetInt32(7),
                    });
                }

                cmd.CommandText = $@"SELECT id, axis_id, mechanism_id, address, io_type, func_code, is_enable 
from preset_address_catalog WHERE mechanism_code in ({string.Join(",", res.Mechanisms.Select(m => $"'{m.Code}'"))})";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    res.AddressRecords.Add(new AddressRecord
                    {
                        Id = reader.GetInt32(0),
                        AxisId = reader.GetInt32(1),
                        MechanismId = reader.GetInt32(2),
                        Address = reader.GetString(3),
                        IOType = (IOType)reader.GetInt32(4),
                        FuncCode = reader.GetString(5),
                        IsEnable = Convert.ToBoolean(reader.GetInt32(6)),
                    });
                }

                return res;
            }
            finally
            {
                conn.Close();
            }
        }
    }

    public class Equipment
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EquipomentMotionPreset
    {
        public List<MotionMechanism> Mechanisms { get; set; } = new List<MotionMechanism>();
        public List<AddressRecord> AddressRecords { get; set; } = new List<AddressRecord>();
    }
}
