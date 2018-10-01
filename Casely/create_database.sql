CREATE TABLE `path_case` (
	`case_number`	TEXT NOT NULL UNIQUE PRIMARY KEY ON CONFLICT IGNORE,
	`service`	TEXT,
	`is_signed_out` INTEGER
);

CREATE TABLE IF NOT EXISTS `case_entry` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,	
	`author_full_name`	TEXT,
	`case_number`	TEXT NOT NULL,
	`date_created`	TEXT,
	`time_created`	TEXT,
	`date_modified`	TEXT,
	`time_modified`	TEXT,
	`tumor_synoptic`	TEXT,
	`comment`	TEXT,
	`result` TEXT,
	`material` TEXT,
	`history` TEXT,
	`interpretation` TEXT,
	`gross` TEXT,
	`microscopic`	TEXT,
	FOREIGN KEY(case_number) REFERENCES path_case(case_number)

);

CREATE TABLE IF NOT EXISTS `part_diagnosis` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,	
	`case_number`	TEXT NOT NULL,
	`part` TEXT NOT NULL,
	`date_modified` TEXT NOT NULL,
	`time_modified` TEXT NOT NULL,
	`organ_system` TEXT,
	`organ` TEXT,
	`category` TEXT,
	`diagnosis`	TEXT,
	`diagnosis_detailed` TEXT
);

CREATE TABLE IF NOT EXISTS `part_entry` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`author_full_name`	TEXT,
	`part`	TEXT,
	`procedure`	TEXT,
	`specimen`	TEXT,
	`organ_system` TEXT,
	`date_created`	TEXT,
	`time_created`	TEXT,
	`date_modified`	TEXT,
	`time_modified`	TEXT,
	`case_number` TEXT NOT NULL,
	`grossed_by_full_name` TEXT,
	FOREIGN KEY(case_number) REFERENCES path_case(case_number)
);

CREATE TABLE IF NOT EXISTS `staff` (
	`full_name`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE,
	`role`	TEXT
);

CREATE TRIGGER insert_part_entry_author AFTER INSERT  ON part_entry
BEGIN
INSERT INTO staff (full_name) VALUES (new.author_full_name);
END;

CREATE TRIGGER insert_case_entry_author AFTER INSERT  ON case_entry
BEGIN
INSERT INTO staff (full_name) VALUES (new.author_full_name);
END;

CREATE TABLE IF NOT EXISTS `specimen` (
	`specimen`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TRIGGER insrt_part_entry_specimen AFTER  INSERT ON part_entry
BEGIN
INSERT INTO specimen (specimen) VALUES (new.specimen);
END;

CREATE TABLE IF NOT EXISTS `procedure` (
	`procedure`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);


CREATE TRIGGER insert_part_entry_procedure AFTER INSERT  ON part_entry
BEGIN
INSERT INTO procedure(procedure) VALUES (new.procedure);
END;


CREATE TABLE IF NOT EXISTS `diagnosis` (
	`diagnosis`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TRIGGER insert_part_diagnosis_diagnosis AFTER INSERT  ON part_diagnosis
BEGIN
INSERT INTO diagnosis (diagnosis) VALUES (new.diagnosis);
END;


CREATE TABLE IF NOT EXISTS `organ` (
	`organ`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TRIGGER insert_part_diganosis_organ AFTER INSERT  ON part_diagnosis
BEGIN
INSERT INTO organ (organ) VALUES (new.organ);
END;


CREATE TABLE IF NOT EXISTS `organ_system` (
	`organ_system`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TRIGGER insert_part_diganosis_organ_system AFTER INSERT  ON part_diagnosis
BEGIN
INSERT INTO organ_system (organ_system) VALUES (new.organ_system);
END;


CREATE TABLE IF NOT EXISTS `diagnosis_category` (
	`category`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TRIGGER insert_part_diganosis_category AFTER INSERT  ON part_diagnosis
BEGIN
INSERT INTO diagnosis_category (category) VALUES (new.category);
END;


