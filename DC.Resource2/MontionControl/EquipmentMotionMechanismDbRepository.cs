using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class EquipmentMotionMechanismDbRepository : IEquipmentMotionMechanismRepository
    {
        private readonly string _dbConnString;
        public EquipmentMotionMechanismDbRepository()
            : this(Constants.dbConnString)
        {
        }

        public EquipmentMotionMechanismDbRepository(string dbConnString)
        {
            _dbConnString = dbConnString ?? throw new ArgumentNullException(nameof(dbConnString));
        }

        public int Add(MotionMechanism mechanism)
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"INSERT INTO  equipment_motion_mechanism 
(mechanism_type,oem,protocol,series,code,ip_address,port,creation_time)
values(@mechanismType,@oem,@protocol,@series,@code,@ipAddress,@port,date('now'));
SELECT last_insert_rowid();";
                cmd.Parameters.AddRange(new[]
                {
                    new SQLiteParameter("@mechanismType", mechanism.MechanismType.ToString()),
                    new SQLiteParameter("@oem", mechanism.Oem.ToString()),
                    new SQLiteParameter("@protocol", mechanism.Protocol.ToString()),
                    new SQLiteParameter("@series", mechanism.Series),
                    new SQLiteParameter("@code", mechanism.Code),
                    new SQLiteParameter("@ipAddress", mechanism.IpAddress),
                    new SQLiteParameter("@port", mechanism.Port),
                });
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            finally
            {
                conn.Close();
            }
        }

        public void Delete(int id)
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE from equipment_motion_mechanism WHERE id=@id;";
                cmd.Parameters.Add(new SQLiteParameter("@id", id));
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM address_catalog WHERE mechanism_id=@id;";
                cmd.Parameters.Add(new SQLiteParameter("@id", id));
                cmd.ExecuteNonQuery();
                tran.Commit();
                cmd.Dispose();
            }
            finally
            {
                conn.Close();
            }
        }

        public List<MotionMechanism> List()
        {
            var conn = new SQLiteConnection(_dbConnString);
            SQLiteDataReader reader = null;
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"SELECT mechanism_type,oem,protocol,series,code,ip_address,port,id FROM  equipment_motion_mechanism";
                reader = cmd.ExecuteReader();
                var res = new List<MotionMechanism>();
                while (reader.Read())
                {
                    res.Add(new MotionMechanism
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
                return res;
            }
            finally
            {
                reader?.Close();
                conn.Close();
            }
        }

        public MotionMechanism Get(int id)
        {
            var conn = new SQLiteConnection(_dbConnString);
            SQLiteDataReader reader = null;
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"SELECT mechanism_type,oem,protocol,series,code,ip_address,port,id FROM  equipment_motion_mechanism where id=@id";
                cmd.Parameters.Add(new SQLiteParameter("@id", id));
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return new MotionMechanism
                    {
                        MechanismType = (MechanismType)Enum.Parse(typeof(MechanismType), reader.GetString(0)),
                        Oem = (OEM)Enum.Parse(typeof(OEM), reader.GetString(1)),
                        Protocol = (Protocol)Enum.Parse(typeof(Protocol), reader.GetString(2)),
                        Series = reader.GetString(3),
                        Code = reader.GetString(4),
                        IpAddress = reader.GetString(5),
                        Port = (ushort)reader.GetInt32(6),
                        Id = reader.GetInt32(7),
                    };
                }
                return null;
            }
            finally
            {
                reader?.Close();
                conn.Dispose();
            }
        }

        public void Update(MotionMechanism mechanism)
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"UPDATE  equipment_motion_mechanism SET
mechanism_type=@mechanismType,oem=@oem,protocol=@protocol,series=@series,code=@code,ip_address=@ipAddress
,port=@port,update_time=date('now')
WHERE id=@id";
                cmd.Parameters.AddRange(new[]
                {
                    new SQLiteParameter("@id", mechanism.Id),
                    new SQLiteParameter("@mechanismType", mechanism.MechanismType.ToString()),
                    new SQLiteParameter("@oem", mechanism.Oem.ToString()),
                    new SQLiteParameter("@protocol", mechanism.Protocol.ToString()),
                    new SQLiteParameter("@series", mechanism.Series),
                    new SQLiteParameter("@code", mechanism.Code),
                    new SQLiteParameter("@ipAddress", mechanism.IpAddress),
                    new SQLiteParameter("@port", mechanism.Port),
                });
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public void Clear()
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = new SQLiteCommand(conn);
                cmd.CommandText = "DELETE FROM equipment_motion_mechanism";
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Dispose();
            }
        }
    }
}
