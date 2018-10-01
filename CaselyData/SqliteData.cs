using System;
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
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string Interpretation { get; set; }
        public string Result { get; set; }
        public string Material { get; set; }
        public string History { get; set; }
        public string Microscopic { get; set; }
        public string Gross { get; set; }
        public string TumorSynoptic { get; set; }
        public string Comment { get; set; }
        public string DateCreatedString { get; set; }
        public string TimeCreatedString { get; set; }
        public string TimeModifiedString { get; set; }
        public string DateModifiedString { get; set; }
        public DateTime DateTimeCreatedObject {
            get {
                return DateTime.Parse(DateCreatedString + " " + TimeCreatedString);
            }
            set {
                DateCreatedString = value.ToShortDateString();

                TimeCreatedString = value.TimeOfDay.ToString();
            }
        }
        public DateTime DateTimeModifiedObject {
            get {
                return DateTime.Parse(DateCreatedString + " " + TimeCreatedString);
            }
            set {
                DateModifiedString = value.ToShortDateString();

                TimeModifiedString = value.TimeOfDay.ToString();
            }
        }
        public string AuthorFullName { get; set; }
        public List<PartEntry> ListPartEntry { get; set; }

        public string PrettyVersion() {
            return $"{AuthorFullName} ({DateModifiedString})";
        }
    }

    public class PartEntry {
        public string Id { get; set; }
        public string CaseNumber { get; set; }
        public string AuthorFullName { get; set; }
        public string Part { get; set; }
        public string Procedure { get; set; }
        public string Specimen { get; set; }
        public string DateCreatedString { get; set; }
        public string TimeCreatedString { get; set; }
        public string TimeModifiedString { get; set; }
        public string DateModifiedString { get; set; }
        public string GrossByFullName { get; set; }
        public DateTime DateTimeCreatedObject {
            get {
                return DateTime.Parse(DateCreatedString + " " +  TimeCreatedString);
            }
            set {
                DateCreatedString = value.ToString("yyyy-MM-dd");

                TimeCreatedString = value.ToString("HH:mm:ss");
            }
        }
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
        public string FullName;
        public string Role;
    }

    public class SqliteDataAcces {

        public static string CaseNumberPrefix = "SMP-18-";



        public static void InsertNewCaseEntry(CaseEntry ce, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO case_entry (author_full_name, case_number, date_created, time_created, date_modified,
                                                time_modified, tumor_synoptic, comment, result, material, history, interpretation, gross, microscopic)
                            VALUES (@AuthorFullName, @CaseNumber,  @DateCreatedString, 
                                    @TimeCreatedString,@DateModifiedString,@TimeModifiedString,@TumorSynoptic, @Comment, @Result, @Material, @History, 
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
                sql = @"INSERT INTO part_entry (author_full_name, part, procedure,
                            specimen,  date_created, time_created,date_modified, time_modified, case_number, grossed_by_full_name)
                            VALUES (@AuthorFullName, @Part, @Procedure, @Specimen, @DateCreatedString, 
                                    @TimeCreatedString,@DateModifiedString,@TimeModifiedString, @CaseNumber, @GrossByFullName);";
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
            var sql = @"SELECT DISTINCT case_number AS CaseNumber, specimen, procedure, date_created AS DateCreatedString
                            FROM part_entry WHERE date_created >= @startDate";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@startDate", startDate.ToString("yyyy-MM-dd"), System.Data.DbType.String);
                var output = cn.Query<string>(sql, dp).ToList();
                return output;
            }
        }

        public static List<string> GetListCaseNumbersPastDays(DateTime startDate) {
            var sql = @"SELECT DISTINCT case_number
                            FROM case_entry WHERE date_created >= @startDate";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@startDate", startDate.ToString("yyyy-MM-dd"), System.Data.DbType.String);
                var output = cn.Query<string>(sql, dp).ToList();
                return output;
            }
        }


        public static void ParseInsertCaseEntry(CaseEntry caseEntry, PathCase pathCase) {
            List<string> sectionWords = new List<string> { "MATERIAL:", "HISTORY:","GROSS:", "MICROSCOPIC:" };
            bool endOfResult = false;
            var linesPathResult = caseEntry.Result.Trim().Replace("\r","").Split('\n');
            int i = 0;
            while ( i < linesPathResult.Length) {
                if (endOfResult) { break; } // we have reached the end of the result. this skips the microscopic dictation portion of report.
                var curLine = linesPathResult[i].Trim();
                var l = "";
                // Reads through result and adds the text from each section to the appropriate CaseEntry property.
                switch (curLine) {
                    case "MATERIAL:":
                        // continue adding text until next section is reached
                        do {
                            l = linesPathResult[++i];
                            caseEntry.Material += l + "\n";
                        } while (sectionWords.IndexOf(linesPathResult[i + 1]) == -1);
                        break;
                    case "HISTORY:":
                        do {
                            l = linesPathResult[++i];
                            caseEntry.History += l + "\n";
                        } while (sectionWords.IndexOf(linesPathResult[i+1]) == -1);
                        break;
                    case "GROSS:":
                        do {
                            l = linesPathResult[++i];
                            caseEntry.Gross += l + "\n";
                        } while (sectionWords.IndexOf(linesPathResult[i + 1]) == -1);
                        break;
                    case "MICROSCOPIC:":
                        // will continue parssing until the last line of the report is reached
                        do {
                            l = linesPathResult[++i];
                            caseEntry.Microscopic += l + "\n";
                        } while (!(l.StartsWith("Microscopic Dictator ID")));
                        endOfResult = true;
                        break;                   
                    default:
                        // just in case nothing matches. Safety measure to at least continue to increment.
                        i++;
                        break;
                }
                i++;
            }
            InsertNewCaseEntry(caseEntry, pathCase);
        }

        public static List<PartEntry> getListPartEntry(string caseNumber) {
            var sql = @"SELECT author_full_name AS AuthorFullName, 
                            part, procedure,
                            specimen, 
                            date_created AS DateCreatedString, time_created AS TimeCreatedString,
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
	author_full_name AS AuthorFullName,
	case_number AS CaseNumber,
	date_created AS DateCreatedString,
	time_created AS TimeCreatedString,
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
            var sql = @"SELECT full_name FROM staff;";
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
                var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                var dbPath = Path.Combine(path, "Casely.db");
                return @"Data Source = "+dbPath; }
        }

        /// <summary>
        /// Inserts a new pathology case into the database.
        /// Case number must be unique. If not unique, insertion will be ignored by sqlite database.
        /// </summary>
        /// <param name="pathCase"></param>
        public static void InsertNewPathCase(PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service) VALUES (@CaseNumber, Service);";
                var result = cn.Execute(sql, new { pathCase });
            }
        }

    }

}
