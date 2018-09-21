CREATE TABLE `path_case` (
	`case_number`	TEXT NOT NULL PRIMARY KEY UNIQUE ON CONFLICT INGORE,
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
	`author_id`	INTEGER,
	`path_case_id`	INTEGER NOT NULL,
	FOREIGN KEY(author_id) REFERENCES staff(id),
	FOREIGN KEY(path_case_id) REFERENCES path_case(id)

);

CREATE TABLE IF NOT EXISTS `part_entry` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`author_id`	INTEGER,
	`part`	TEXT,
	`procedure`	TEXT,
	`specimen`	TEXT,
	`diagnosis`	TEXT,
	`date`	TEXT,
	`time`	TEXT,
	`path_case` INTEGER,
	FOREIGN KEY(author_id) REFERENCES staff(id),
	FOREIGN KEY(path_case_id) REFERENCES path_case(id)
);

CREATE TABLE IF NOT EXISTS `staff` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`name`	TEXT,
	`role`	TEXT
);