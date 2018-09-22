PRAGMA foreign_keys = ON;

CREATE TABLE `path_case` (
	`case_number`	TEXT NOT NULL UNIQUE PRIMARY KEY ON CONFLICT IGNORE,
	`service`	TEXT
);

CREATE TABLE IF NOT EXISTS `case_entry` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`tumor_synoptic`	TEXT,
	`comment`	TEXT,
	`date`	TEXT,
	`time`	TEXT,
	`material` TEXT,
	`interpretation` TEXT,
	`gross` TEXT,
	`microscopic`	TEXT,
	`author_full_name`	TEXT,
	`case_number`	TEXT NOT NULL,
	FOREIGN KEY(case_number) REFERENCES path_case(case_number)

);

CREATE TABLE IF NOT EXISTS `part_entry` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`author_full_name`	TEXT,
	`part`	TEXT,
	`procedure`	TEXT,
	`specimen`	TEXT,
	`diagnosis`	TEXT,
	`date`	TEXT,
	`time`	TEXT,
	`case_number` TEXT NOT NULL,
	FOREIGN KEY(case_number) REFERENCES path_case(case_number)
);

CREATE TABLE IF NOT EXISTS `staff` (
	`full_name`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE,
	`role`	TEXT
);

CREATE TABLE IF NOT EXISTS `diagnosis` (
	`diagnosis`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TABLE IF NOT EXISTS `specimen` (
	`specimen`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TABLE IF NOT EXISTS `procedure` (
	`procedure`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TRIGGER insert_part_entry_author AFTER INSERT  ON part_entry
BEGIN
INSERT INTO staff (full_name) VALUES (new.author_full_name);
END;

CREATE TRIGGER insert_part_entry_diagnosis AFTER INSERT  ON part_entry
BEGIN
INSERT INTO diagnosis (diagnosis) VALUES (new.diagnosis);
END;

CREATE TRIGGER insrt_part_entry_specimen AFTER  INSERT ON part_entry
BEGIN
INSERT INTO specimen (specimen) VALUES (new.specimen);
END;


CREATE TRIGGER insert_part_entry_procedure AFTER INSERT  ON part_entry
BEGIN
INSERT INTO procedure(procedure) VALUES (new.procedure);
END;

