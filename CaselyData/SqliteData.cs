﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using System.IO;

namespace CaselyData {
    public class PathCase {
        public string CaseNumber { get; set; }
        public string Service { get; set; }
    }
    public class CaseEntry {
        string result = "";
        string microscopic = "";
        string history = "";
        string material = "";
        string gross = "";
        string tumorSynoptic = "";
        string comment = "";
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string Interpretation { get; set; }
        public string Result { get { return this.result; }
            set {
                this.result = value;
                // parse the result and fill in the micro,gross,material, and history fields.
                PopulateMicrGrossMatHist();
            }
        }
        public string Material { get { return material ; } set { material = value; } }
        public string History { get { return history; } set { history = value; } }
        public string Microscopic { get { return microscopic; } set { microscopic = value; } }
        public string Gross { get { return gross; }  set { gross = value; } }
        public string TumorSynoptic { get { return tumorSynoptic; } set { tumorSynoptic = value; } }
        public string Comment { get { return comment; } set { comment = value; } }
        public string TimeModifiedString { get; set; }
        public string DateModifiedString { get; set; }
        
        public DateTime DateTimeModifiedObject {
            get {
                return DateTime.Parse(DateModifiedString + " " + TimeModifiedString);
            }
            set {
                DateModifiedString = value.ToString("yyyy-MM-dd");

                TimeModifiedString = value.TimeOfDay.ToString();
            }
        }
        public string SoftID { get; set; }
        public List<PartEntry> ListPartEntry { get; set; }

        public string PrettyVersion() {
            return $"{SoftID} ({DateModifiedString})";
        }

        private void PopulateMicrGrossMatHist() {
            List<string> sectionWords = new List<string> { "MATERIAL:", "HISTORY:", "GROSS:", "MICROSCOPIC:" };
            bool endOfResult = false;
            var linesPathResult = Result.Trim().Replace("\r", "").Split('\n');
            int i = 0;
            int lineCount = linesPathResult.Length;
            while (i < lineCount) {
                if (endOfResult) { break; } // we have reached the end of the result. this skips the microscopic dictation portion of report.
                var curLine = linesPathResult[i].Trim();
                var l = "";
                // Reads through result and adds the text from each section to the appropriate CaseEntry property.
                switch (curLine) {
                    case "MATERIAL:":
                        // continue adding text until next section is reached
                        do {
                            l = linesPathResult[++i];
                            Material += l + "\n";
                            if (i+1 >= lineCount) break; // handles the case if no other sections are found after material
                        } while (sectionWords.IndexOf(linesPathResult[i + 1].Trim()) == -1);
                        Material = material.Trim();
                        break;
                    case "HISTORY:":
                        do {
                            l = linesPathResult[++i];
                            History += l + "\n";
                            if (i + 1 >= lineCount) break;
                        } while (sectionWords.IndexOf(linesPathResult[i + 1].Trim()) == -1);
                        History = history.Trim();
                        break;
                    case "GROSS:":
                        do {
                            l = linesPathResult[++i];
                            Gross += l + "\n";
                            if (i + 1 >= lineCount) break;
                        } while (sectionWords.IndexOf(linesPathResult[i + 1].Trim()) == -1);
                        Gross = gross.Trim();
                        break;
                    case "MICROSCOPIC:":
                        // will continue parsing until the last line of the report is reached
                        do {
                            l = linesPathResult[++i];
                            Microscopic += l + "\n";
                            if (i + 1 >= lineCount) break;
                        } while (!(l.StartsWith("Microscopic Dictator ID")));
                        endOfResult = true;
                        Microscopic = microscopic.Trim();
                        break;
                    default:
                        // just in case nothing matches. Safety measure to at least continue to increment.
                        i++;
                        break;
                }
                i++;
            }
        }
    }

    public class PartEntry {
        public string Id { get; set; }
        public string CaseNumber { get; set; }
        public string Part { get; set; }
        public string Procedure { get; set; }
        public string Specimen { get; set; }
        public string TimeModifiedString { get; set; }
        public string DateModifiedString { get; set; }
        public string SoftID { get; set; }
       
        public DateTime DateTimeModifiedObject {
            get {
                return DateTime.Parse(DateModifiedString + " " + TimeModifiedString);
            }
            set {
                DateModifiedString = value.ToString("yyyy-MM-dd");

                TimeModifiedString = value.ToString("HH:mm:ss");
            }
        }
    }

    public class PartDiagnosis {
        public string Id { get; set; }
        public string CaseNumber { get; set; }
        public string Part { get; set; }
        public string DateModifiedString { get; set; }
        public string TimeModifiedString { get; set; }
        public string OrganSystem { get; set; }
        public string Organ { get; set; }
        public string Category { get; set; }
        public string Diagnosis { get; set; }
        public string DiagnosisDetailed { get; set; }
        public DateTime DateTimeModifiedObject {
            get {
                return DateTime.Parse(DateModifiedString + " " + TimeModifiedString);
            }
            set {
                DateModifiedString = value.ToString("yyyy-MM-dd");

                TimeModifiedString = value.ToString("HH:mm:ss");
            }
        }
    }

    public class Staff {
        public string SoftID;
        public string firstLastName;
        public string Role;
    }

    public class SqliteDataAcces {

        public static string CaseNumberPrefix = "SMP-18-";

        

        public static void InsertNewCaseEntry(CaseEntry ce, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO case_entry (soft_id, case_number, date_modified,
                                                time_modified, tumor_synoptic, comment, result, material, history, interpretation, gross, microscopic)
                            VALUES (@SoftID, @CaseNumber,@DateModifiedString,@TimeModifiedString,@TumorSynoptic, @Comment, @Result, @Material, @History, 
                                    @Interpretation, @Gross, @Microscopic);";
                cn.Execute(sql, ce);
            }
        }

        public static void InsertNewPartDiagnosisEntry(List<PartDiagnosis> ce, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO part_diagnosis ( case_number, part, date_modified, time_modified,
                                                organ_system, organ, category, diagnosis, diagnosis_detailed)
                            VALUES (@CaseNumber, @Part, @DateModifiedString, @TimeModifiedString,@OrganSystem, @Organ, 
                                    @Category, @Diagnosis, @DiagnosisDetailed);";
                cn.Execute(sql, ce);
            }
        }

        public static List<PartDiagnosis> getListPartDiagnosis(string caseNumber) {
            var sql = @"SELECT case_number AS CaseNumber,
                            part,
                            date_modified AS DateModifiedString, 
                            time_modified AS TimeModifiedString,
                            organ_system AS OrganSystem,
                            organ, category, diagnosis, 
                            diagnosis_detailed AS DiagnosisDetailed
                            FROM part_diagnosis WHERE CaseNumber = @caseNumber;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query<PartDiagnosis>(sql, dp).ToList();
                return output;
            }
        }

        /// <summary>
        /// Retrieves the part diagnosis with all of its versions, and then returns the latest version
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public static List<PartDiagnosis> GetListPartDiagnosisLatestVersion(string caseNumber) {
            List<PartDiagnosis> parts = getListPartDiagnosis(caseNumber);
            DateTime latestDateTimeModified = (from latestTimeModified in parts
                                               orderby latestTimeModified.DateTimeModifiedObject descending
                                               select latestTimeModified.DateTimeModifiedObject).FirstOrDefault();
            List<PartDiagnosis> latestPartEntry = new List<PartDiagnosis>();
            if (latestDateTimeModified != null) {
                latestPartEntry = (from p in parts.ToList<PartDiagnosis>()
                                   where p.DateTimeModifiedObject.Equals(latestDateTimeModified)
                                   select p).ToList();
            }
            return latestPartEntry;
        }

        public static bool EntryExistsPartDiagnosis(string caseNumber) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"select exists(select 1 from part_diagnosis where case_number = @caseNumber LIMIT 1);";
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                int v = cn.Query<int>(sql, dp).FirstOrDefault();
                bool entryExists = v == 1 ? true : false;
                return entryExists;
            }
               
        }

        public static void InsertNewParts(List<PartEntry> parts, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO part_entry (part, procedure,
                            specimen, date_modified, time_modified, case_number, soft_id)
                            VALUES (@Part, @Procedure, @Specimen, @DateModifiedString,@TimeModifiedString, @CaseNumber, @SoftID);";
                cn.Execute(sql, parts);
            }
        }

        public static List<PartEntry> GetListPartEntryLatestVersion(string caseNumber) {
            List<PartEntry> parts = getListPartEntry(caseNumber);
            DateTime latestDateTimeModified = (from latestTimeModified in parts
                                               orderby latestTimeModified.DateTimeModifiedObject descending
                                               select latestTimeModified.DateTimeModifiedObject).FirstOrDefault();
            List<PartEntry> latestPartEntry = new List<PartEntry>();
            if (latestDateTimeModified != null) {
                latestPartEntry = (from p in parts.ToList<PartEntry>()
                                   where p.DateTimeModifiedObject.Equals(latestDateTimeModified)
                                   select p).ToList();
            }
            return latestPartEntry;
        }


        public static List<string> GetListPartEntryPastDays(DateTime startDate) {
            var sql = @"SELECT DISTINCT case_number AS CaseNumber, specimen, procedure, date_modified AS DateModifiedString
                            FROM part_entry WHERE date_modified >= @startDate";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@startDate", startDate.ToString("yyyy-MM-dd"), System.Data.DbType.String);
                var output = cn.Query<string>(sql, dp).ToList();
                return output;
            }
        }

        public static List<CaseEntry> GetListCaseEntryPastDays(DateTime startDate) {
            var sql = @"SELECT DISTINCT case_number AS CaseNumber, material, date_modified AS DateModifiedString
                            FROM case_entry WHERE date_modified >= @startDate";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@startDate", startDate.ToString("yyyy-MM-dd"), System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                return output;
            }
        }

        public static List<CaseEntry> GetListCaseEntriesPastDays(DateTime startDate) {
            var sql = @"SELECT case_number AS CaseNumber,
	                    date_modified AS DateModifiedString,
	                    time_modified AS TimeModifiedString,
	                    tumor_synoptic AS TumorSynoptic,
	                    comment,
	                    result,
	                    material,
	                    history,
	                    interpretation,
	                    gross,
	                    microscopic FROM case_entry 
                        GROUP BY case_number";
            var strStartDate = startDate.ToString("yyyy-MM-dd");
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@startDate", strStartDate, System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                return output;
            }
        }


      

        public static List<PartEntry> getListPartEntry(string caseNumber) {
            var sql = @"SELECT soft_id AS SoftID, 
                            part, procedure,
                            specimen, 
                            date_modified AS DateModifiedString, time_modified AS TimeModifiedString,
                            case_number AS CaseNumber 
                            FROM part_entry WHERE CaseNumber = @caseNumber;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query<PartEntry>(sql, dp).ToList();
                return output;
            }
        }



        public static List<CaseEntry> getListCaseEntry(string caseNumber) {
            var sql = @"SELECT id,
	                    soft_id AS SoftID,
	                    case_number AS CaseNumber,
	                    date_modified AS DateModifiedString,
	                    time_modified AS TimeModifiedString,
	                    tumor_synoptic AS TumorSynoptic,
	                    comment,
	                    result,
	                    material,
	                    history,
	                    interpretation,
	                    gross,
	                    microscopic FROM case_entry WHERE case_number = @caseNumber;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                return output;
            }
        }

        public static CaseEntry GetCaseEntryLatestVersion(string caseNumber) {
            List<CaseEntry> cases = getListCaseEntry(caseNumber);
            DateTime latestDateTimeModified = (from latestTimeModified in cases
                                               orderby latestTimeModified.DateTimeModifiedObject descending
                                               select latestTimeModified.DateTimeModifiedObject).FirstOrDefault();
            CaseEntry latestCaseEntry = new CaseEntry();
            if (latestDateTimeModified != null) {
                latestCaseEntry = (from p in cases.ToList<CaseEntry>()
                                   where p.DateTimeModifiedObject.Equals(latestDateTimeModified)
                                   select p).FirstOrDefault();
            }
            return latestCaseEntry;
        }

        public static List<Staff> GetListStaff() {
            var sql = @"SELECT full_name, role FROM staff;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<Staff>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetListStaffFullNames() {
            var sql = @"SELECT last_first_name FROM staff;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }


        public static List<string> GetListProcedure() {
            var sql = @"SELECT procedure FROM procedure;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetListSpecimen() {
            var sql = @"SELECT specimen FROM specimen;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetListCategory() {
            var sql = @"SELECT category FROM diagnosis_category;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetListDiagnosis() {
            var sql = @"SELECT diagnosis FROM diagnosis;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetListOrgan() {
            var sql = @"SELECT organ FROM organ;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetListOrganSystem() {
            var sql = @"SELECT organ_system FROM organ_system;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }


        public static string DbConnectionString {
            get {
               
                return @"Data Source = "+ DBPath; 
            } 
        }

        public static string DBPath {
            get {
                //var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                var path = @"L:\data\";
                var dbPath = Path.Combine(path, "Casely.db");
                return dbPath;
            }
        }

        /// <summary>
        /// Creates the Casely database
        /// </summary>
        public static void CreateDatabase() {
            if (!(File.Exists(DBPath))) {
               var f = File.Create(DBPath);
                f.Close();
            }
            
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                cn.Execute(DBCreationString.sqlCreateDBString);
            }
        }

        /// <summary>
        /// Inserts a new pathology case into the database.
        /// Case number must be unique. If not unique, insertion will be ignored by sqlite database.
        /// </summary>
        /// <param name="pathCase"></param>
        public static void InsertNewPathCase(PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service) VALUES (@CaseNumber, @Service);";
                var result = cn.Execute(sql, pathCase );
            }
        }

    }

}
