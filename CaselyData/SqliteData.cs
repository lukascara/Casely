using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using System.Diagnostics;
using System.ComponentModel;

namespace CaselyData {
    public class PathCase {
        public string CaseNumber { get; set; }
        public string Service { get; set; }
    }
    public class CaseEntry {
        public int Id { get; set; }
        public string Interpretation { get; set; }
        public string Material { get; set; }
        public string Microscopic { get; set; }
        public string Gross { get; set; }
        public string TumorSynoptic { get; set; }
        public string Comment { get; set; }
        public string DateString { get; set; }
        public string TimeString { get; set; }
        public DateTime DateTimeObject {
            get {
                return DateTime.Parse(DateString + " " + TimeString);
            }
            set {
                DateString = value.ToShortDateString();

                TimeString = value.TimeOfDay.ToString();
            }
        }
        public string AuthorFullName { get; set; }
        public List<PartEntry> ListPartEntry { get; set; }
    }

    public class PartEntry {
        public string Id { get; set; }
        public string CaseNumber { get; set; }
        public string AuthorFullName { get; set; }
        public string Part { get; set; }
        public string Procedure { get; set; }
        public string Specimen { get; set; }
        public string Diagnosis { get; set; }
        public string DateString { get; set; }
        public string TimeString { get; set; }
        public DateTime DateTimeObject {
            get {
                return DateTime.Parse(DateString + " " + TimeString);
            }
            set {
                DateString = value.ToShortDateString();

                TimeString = value.TimeOfDay.ToString();
            }
        }
    }

    public class Staff {
        public string FullName;
        public string Role;
    }

    public class SqliteDataAcces {

        public static void InsertNewParts(List<PartEntry> parts, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO part_entry (author_full_name, part, procedure,
                            specimen, diagnosis, date, time, case_number)
                            VALUES (@AuthorFullName, @Part, @Procedure, @Specimen, @Diagnosis, @DateString, 
                                    @TimeString, @CaseNumber);" ;
                cn.Execute(sql, parts);
            }
        }

        public static List<PartEntry> getListPartEntry(string case_number) {
            var sql = @"SELECT author_full_name AS AuthorFullName, 
                            part, procedure,
                            specimen, diagnosis, 
                            date AS DateString, time AS TimeString,
                            case_number AS CaseNumber 
                            FROM part_entry WHERE CaseNumber = @case_number;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@case_number", case_number, System.Data.DbType.String);
                var output = cn.Query<PartEntry>(sql, dp).ToList();
                return output;
            }
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


        public static string DbConnectionString {
            get { return @"Data Source = C:\Users\Lukas_and_Carlie\source\repos\Casely\Casely.db"; }
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

       

        public static List<PartEntry> getParts(string Id) {
            List<PartEntry> parts = new List<PartEntry>();
            PartEntry p1 = new PartEntry();
            p1.AuthorFullName = "Lukas Cara";
            p1.DateString = "11/09/2018";
            p1.TimeString = "2:00PM";
            p1.Specimen = "Ovary";
            p1.Procedure = "Salpingectomy";
            p1.Part = "A";
            parts.Add(p1);
            PartEntry p2 = new PartEntry();
            p2.AuthorFullName = "Lukas Cara";
            p2.DateString = "11/09/2018";
            p2.TimeString = "2:00PM";
            p2.Specimen = "Ovary222";
            p2.Procedure = "Salpingectomy";
            p2.Part = "B";
            parts.Add(p2);
            return parts;
        }
    }

    

}
