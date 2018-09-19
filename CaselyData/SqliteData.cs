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
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string Service { get; set; }
        public List<CaseEntry> ListCaseEntry { get; set; }
    }
    public class CaseEntry {
        public int Id { get; set; }
        public string TumorSynoptic { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public Staff Author { get; set; }
        public List<PartEntry> ListPartEntry { get; set; }
    }

    public class PartEntry {
        public string Id { get; set; }
        public string Interpretation { get; set; }
        public string Material { get; set; }
        public Staff Author { get; set; }
        public string Microscopic { get; set; }
        public string Gross { get; set; }
        public string Part { get; set; }
        public string Procedure { get; set; }
        public string Specimen { get; set; }
        public string DateString { get; set; }
        public string TimeString { get; set; }
        public DateTime DateTimeObject {
            get {
                return DateTime.Parse(DateString + " " + TimeString);
            }
            set {
                DateString = value.Date.ToString();
                TimeString = value.TimeOfDay.ToString();
            }
        }
    }

    public class Staff {
        public string FullName;
        public string Role;
    }

    public class SqliteDataAcces {
        public static List<PartEntry> getParts(string Id) {
            List<PartEntry> parts = new List<PartEntry>();
            PartEntry p1 = new PartEntry();
            var a1 = new Staff();
            a1.FullName = "Lukas Cara";
            p1.Author = a1;
            p1.DateString = "11/09/2018";
            p1.TimeString = "2:00PM";
            p1.Specimen = "Ovary";
            p1.Interpretation = "";
            p1.Material = "blahbla";
            p1.Microscopic = "";
            p1.Procedure = "Salpingectomy";
            p1.Part = "A";
            parts.Add(p1)
;
            return parts;
        }
    }
    

}
