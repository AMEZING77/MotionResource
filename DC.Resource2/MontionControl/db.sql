
CREATE table address_catalog(
	id integer primary key autoincrement,
	axis_id int not null default(0),
	mechanism_code varchar(20) not null,
	address varchar(20) not null,
	io_type int default(0) not null,
	func_code varchar(50) not null,
	is_enable int default(1),
	creation_time datetime,
	update_time datetime
)

CREATE  table equipment_motion_mechanism(
	mechanism_type varchar(20) not null,
	oem varchar(50) not null,
	protocol varchar(50) not null,
	series varchar(50) not null,
	code varchar(20) not null,
	ip_address varchar(20) not null,
	port int default(502),
	creation_time datetime,
	update_time datetime
)

CREATE  table preset_equipment_catalog(
	code varchar(30) not null unique,
	name varchar(50) not null,
	description text null,
	creation_time datetime null
)

CREATE table preset_motion_mechanism(
	equipment_code varchar(30),
	mechanism_type varchar(20) not null,
	oem varchar(50) not null,
	protocol varchar(50) not null,
	series varchar(50) not null,
	code varchar(20) not null,
	ip_address varchar(20) not null,
	port int default(502),
	creation_time datetime
)

CREATE  table preset_address_catalog(
	id integer primary key autoincrement,
	mechanism_code varchar(20) not null,
	axis_id int not null default(0),
	address varchar(20) not null,
	io_type int default(0) not null,
	func_code varchar(50) not null,
	is_enable int default(1)
)

