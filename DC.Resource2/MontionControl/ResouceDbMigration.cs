using DC.Common2;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Resource2
{
    public class ResouceDbMigration : SqliteDBMigrateBase
    {
		static ResouceDbMigration()
		{
			Directory.CreateDirectory(Constants.dbDir);
		}

        public override int VersionNow => 1;

        public ResouceDbMigration(ILogger logger)
            : this(logger, Constants.dbConnString) { }

        public ResouceDbMigration(ILogger logger, string dbConnString)
            : base(logger, dbConnString)
        {
            AddMigration(new Common2.Migrate(1, $@"
CREATE table address_catalog(
	id integer primary key autoincrement,
	axis_id int not null default(0),
	mechanism_id int not null,
	address varchar(20) not null,
	func_code varchar(50) not null,
	io_type int default(0) not null,
	is_enable int default(1),
	creation_time datetime,
	update_time datetime
);

CREATE  table equipment_motion_mechanism(
	id integer primary key autoincrement,
	mechanism_type varchar(20) not null,
	oem varchar(50) not null,
	protocol varchar(50) not null,
	series varchar(50) not null,
	code varchar(20) not null,
	ip_address varchar(20) not null,
	port int default(502),
	creation_time datetime,
	update_time datetime
);

CREATE  table preset_equipment_catalog(
	code varchar(30) not null unique,
	name varchar(50) not null,
	description text null,
	creation_time datetime null
);

CREATE table preset_motion_mechanism(
	id integer primary key autoincrement,
	equipment_code varchar(30),
	mechanism_type varchar(20) not null,
	oem varchar(50) not null,
	protocol varchar(50) not null,
	series varchar(50) not null,
	code varchar(20) not null,
	ip_address varchar(20) not null,
	port int default(502),
	creation_time datetime
);

CREATE  table preset_address_catalog(
	id integer primary key autoincrement,
	mechanism_code varchar(20) not null,
	axis_id int not null default(0),
	address varchar(20) not null,
	func_code varchar(50) not null,
	is_enable int default(1)
);

INSERT INTO preset_equipment_catalog
(code,name,description) VALUES
('0101','单架一体机','');


INSERT INTO preset_motion_mechanism
(equipment_code,mechanism_type,oem,protocol,series,code,ip_address,port) VALUES
('','','Inovance','ModbusTcp','','01','192.168.1.88',502),
('','','Inovance','ModbusTcp','','01','192.168.1.88',502);

INSERT INTO preset_address_catalog
(mechanism_code, axis_id, address, func_code) VALUES
('',0,'',''),
('',0,'','');
", "initialization"));
        }
    }
}
