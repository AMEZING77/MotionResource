using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    /// <summary>
    /// 实现DB中地址配置的CRUD
    /// </summary>
    public class AddressCatalogDbRepository : IAddressRepository
    {
        private readonly string _dbConnString;

        public AddressCatalogDbRepository()
            : this(Constants.dbConnString)
        {
        }

        public AddressCatalogDbRepository(string dbConnString)
        {
            if (dbConnString == null) { throw new ArgumentNullException(nameof(dbConnString)); }
            _dbConnString = dbConnString;
        }

        public int Add(AddressRecord record)
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText =
$@"INSERT INTO  address_catalog (axis_id,mechanism_id,address,io_type,func_code,is_enable,creation_time)
VALUES (@axisId,@mechanismId,@address,@ioType,@funcCode,@isEnable,date('now'));
SELECT last_insert_rowid();";
                cmd.Parameters.AddRange(new[]
                {
                    new SQLiteParameter("@axisId", record.AxisId),
                    new SQLiteParameter("@mechanismId", record.MechanismId),
                    new SQLiteParameter("@address", record.Address),
                    new SQLiteParameter("@ioType", record.IOType),
                    new SQLiteParameter("@funcCode", record.FuncCode),
                    new SQLiteParameter("@isEnable", record.IsEnable),
                });
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            finally
            {
                conn.Dispose();
            }
        }

        public AddressRecord Get(int id)
        {
            var conn = new SQLiteConnection(_dbConnString);
            SQLiteDataReader reader = null;
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id, axis_id, mechanism_id, address, io_type, func_code, is_enable from address_catalog WHERE id=@id";
                cmd.Parameters.Add(new SQLiteParameter("@id", id));
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return new AddressRecord
                    {
                        Id = reader.GetInt32(0),
                        AxisId = reader.GetInt32(1),
                        MechanismId = reader.GetInt32(2),
                        Address = reader.GetString(3),
                        IOType = (IOType)reader.GetInt32(4),
                        FuncCode = reader.GetString(5),
                        IsEnable = Convert.ToBoolean(reader.GetInt32(6)),
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

        public void Delete(int id)
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE from address_catalog WHERE id=@id";
                cmd.Parameters.Add(new SQLiteParameter("@id", id));
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Dispose();
            }
        }

        public List<AddressRecord> List()
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id, axis_id, mechanism_Id, address, io_type, func_code, is_enable from address_catalog";
                var reader = cmd.ExecuteReader();
                var res = new List<AddressRecord>();
                while (reader.Read())
                {
                    res.Add(new AddressRecord
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
                conn.Dispose();
            }
        }


        public void Update(AddressRecord record)
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText =
$@"UPDATE address_catalog SET axis_id=@axisId, mechanism_id=@mechanismId, address=@address,
io_type=@ioType, func_code=@funcCode, is_enable=@isEnable, update_time=date('now')
WHERE id=@id";
                cmd.Parameters.AddRange(new[]
                {
                    new SQLiteParameter("@id",record.Id),
                    new SQLiteParameter("@axisId", record.AxisId),
                    new SQLiteParameter("@mechanismId", record.MechanismId),
                    new SQLiteParameter("@address", record.Address),
                    new SQLiteParameter("@ioType", record.IOType),
                    new SQLiteParameter("@funcCode", record.FuncCode),
                    new SQLiteParameter("@isEnable", record.IsEnable),
                });
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Dispose();
            }
        }

        public void Clear()
        {
            var conn = new SQLiteConnection(_dbConnString);
            try
            {
                conn.Open();
                var cmd = new SQLiteCommand(conn);
                cmd.CommandText = "DELETE FROM address_catalog";
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Dispose();
            }
        }
    }
}
