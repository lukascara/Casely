using System.Collections.Generic;

namespace CaselyData {
	public class DBCreationString {
        /// <summary>
        /// Dictionary with a key of the PRAGMA user_version of the sqlite database and a value with the creation/update SQL query needed to creation that version of the db
        /// </summary>
        public static Dictionary<int, string> dictSQVersion = new Dictionary<int, string>() {
                                                {0, @"CREATE TABLE `path_case` (
	`case_number`	TEXT NOT NULL UNIQUE PRIMARY KEY ON CONFLICT IGNORE,
	`service`	TEXT,
	`evaluation` TEXT,
    `date_of_service` TEXT
);


CREATE TABLE IF NOT EXISTS `case_entry` (
	`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,	
	`author_id`	TEXT,
	`case_number`	TEXT NOT NULL,
	`date_modified`	TEXT,
	`time_modified`	TEXT,
	`tumor_synoptic`	TEXT,
	`comment`	TEXT,
	`result` TEXT,
	`material` TEXT,
	`history` TEXT,
	`interpretation` TEXT,
	`gross` TEXT,
	`microscopic`	TEXT

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
	`author_id`	TEXT,
	`part`	TEXT,
	`procedure`	TEXT,
	`specimen`	TEXT,
	`organ_system` TEXT,
	`date_modified`	TEXT,
	`time_modified`	TEXT,
	`case_number` TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS `staff` (
	`last_first_name`	TEXT,
	`author_id`TEXT UNIQUE NOT NULL PRIMARY KEY ON CONFLICT IGNORE,
	`role`	TEXT
);

CREATE TABLE IF NOT EXISTS `evaluation` (
	`evaluation`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TABLE IF NOT EXISTS `service` (
	`service`	TEXT UNIQUE PRIMARY KEY ON CONFLICT IGNORE
);

CREATE TRIGGER insert_part_entry_author AFTER INSERT  ON part_entry
BEGIN
INSERT INTO staff (author_id) VALUES (new.author_id);
END;

-- trigger to add case entry content to the full search table
CREATE TRIGGER insert_case_entry AFTER INSERT  ON case_entry
BEGIN
INSERT INTO staff (author_id) VALUES (new.author_id);
INSERT INTO fts5_case_entry_result (author_id, case_number, date_modified, time_modified, result) VALUES (new.author_id, new.case_number, new.date_modified, new.time_modified, new.result);
INSERT INTO fts5_case_entry_interpretation (author_id, case_number, date_modified, time_modified,interpretation) VALUES (new.author_id, new.case_number, new.date_modified, new.time_modified,new.interpretation);
INSERT INTO  fts5_case_entry_comment (author_id, case_number, date_modified, time_modified, comment)  VALUES (new.author_id, new.case_number,new.date_modified, new.time_modified, new.comment);
INSERT INTO  fts5_case_entry_tumor_synoptic (author_id, case_number, date_modified, time_modified, tumor_synoptic)  VALUES (new.author_id, new.case_number, new.date_modified, new.time_modified,new.tumor_synoptic);
END;

CREATE TABLE IF NOT EXISTS `specimen` (
	`specimen`	TEXT UNIQUE NOT NULL PRIMARY KEY ON CONFLICT IGNORE
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

CREATE TRIGGER insert_case_entry_case_number AFTER INSERT ON case_entry
BEGIN
INSERT INTO path_case (case_number) VALUES (new.case_number);
END;

CREATE TRIGGER insert_part_entry_case_number AFTER INSERT ON part_entry
BEGIN
INSERT INTO path_case (case_number) VALUES (new.case_number);
END;

CREATE TRIGGER insert_path_case_case_number AFTER UPDATE ON path_case
BEGIN
INSERT INTO evaluation (evaluation) VALUES (new.evaluation);
END;

CREATE TRIGGER insert_service_path_case AFTER UPDATE ON path_case
BEGIN
INSERT INTO service (service) VALUES (new.service);
END;

CREATE VIRTUAL TABLE fts5_case_entry_result USING fts5(author_id, case_number, date_modified, time_modified, result);
CREATE VIRTUAL TABLE fts5_case_entry_interpretation USING fts5(author_id, case_number, date_modified, time_modified, interpretation);
CREATE VIRTUAL TABLE fts5_case_entry_comment USING fts5(author_id, case_number, date_modified, time_modified, comment);
CREATE VIRTUAL TABLE fts5_case_entry_tumor_synoptic USING fts5(author_id, case_number, date_modified, time_modified, tumor_synoptic);

CREATE INDEX casenum_case_entry ON case_entry (case_number);
CREATE INDEX casenum_part_entry ON part_entry (case_number);
CREATE INDEX casenum_path_case ON path_case (case_number);
CREATE INDEX casenum_part_diagnosis ON part_diagnosis (case_number);
	"}, 
// VERSION 1
                                                {1, @"ALTER TABLE path_case ADD COLUMN `evaluation_comment` TEXT;
CREATE VIRTUAL TABLE fts5_path_case_evaluation_comment USING fts5(case_number, evaluation_comment);

-- trigger to add case entry content to the full search table
CREATE TRIGGER insert_path_case AFTER INSERT  ON path_case
BEGIN
INSERT INTO fts5_path_case_evaluation_comment(case_number, evaluation_comment)  VALUES(new.case_number, new.evaluation_comment);
END;

CREATE TRIGGER update_path_case UPDATE OF evaluation_comment ON path_case
BEGIN
  UPDATE fts5_path_case_evaluation_comment SET evaluation_comment = new.evaluation_comment WHERE case_number = old.case_number;
        END;"},
// VERSION 2: update table names and migrate data
            {2, @"CREATE TABLE `casely_user_data` (
	`case_number`	TEXT NOT NULL UNIQUE PRIMARY KEY ON CONFLICT IGNORE,
	`service`	TEXT,
	`evaluation` TEXT,
    `date_of_service` TEXT,
    `evaluation_comment` TEXT
);

CREATE INDEX casenum_casely_user_data ON casely_user_data (case_number);
DROP TABLE IF EXISTS fts5_path_case_evaluation_comment;
CREATE VIRTUAL TABLE fts5_casely_user_data_evaluation_comment USING fts5(case_number, evaluation_comment);

-- Delete old triggers
            DROP TRIGGER IF EXISTS insert_path_case;
            DROP TRIGGER IF EXISTS update_path_case;
            DROP TRIGGER IF EXISTS insert_case_entry_case_number;
            DROP TRIGGER IF EXISTS insert_part_entry_case_number;
            DROP TRIGGER IF EXISTS insert_path_case_case_number;
            DROP TRIGGER IF EXISTS insert_service_path_case;

-- Create triggers for the new casely_user_data table, these are created first
-- so that when we migrate the data, the search tables are updated
CREATE TRIGGER insert_casely_user_data AFTER INSERT  ON casely_user_data
BEGIN
INSERT INTO fts5_casely_user_data_evaluation_comment(case_number, evaluation_comment)  VALUES(new.case_number, new.evaluation_comment);
END;

CREATE TRIGGER update_casely_user_data UPDATE OF evaluation_comment ON casely_user_data
BEGIN
  UPDATE fts5_casely_user_data_evaluation_comment SET evaluation_comment = new.evaluation_comment WHERE case_number = old.case_number;
        END;

CREATE TRIGGER insert_case_entry_case_number AFTER INSERT ON case_entry
BEGIN
INSERT INTO casely_user_data (case_number) VALUES (new.case_number);
END;

CREATE TRIGGER insert_part_entry_case_number AFTER INSERT ON part_entry
BEGIN
INSERT INTO casely_user_data (case_number) VALUES (new.case_number);
END;

CREATE TRIGGER insert_casely_user_data_case_number AFTER UPDATE ON casely_user_data
BEGIN
INSERT INTO evaluation (evaluation) VALUES (new.evaluation);
END;

CREATE TRIGGER insert_service_casely_user_data AFTER UPDATE ON casely_user_data
BEGIN
INSERT INTO service (service) VALUES (new.service);
END;

-- Import the old path_case table into the new casely_user_data 
INSERT INTO casely_user_data (case_number, service, evaluation) SELECT case_number, service, evaluation
                    FROM path_case;
        
  -- Delete old triggers and tables   


            DROP TABLE IF EXISTS path_case;
" }
        };
      
    }
}

    