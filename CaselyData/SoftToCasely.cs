using System;
using System.Collections.Generic;
using System.Linq;
using ExcelDataReader;
using System.IO;
using Superpower;
using Superpower.Parsers;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;

namespace CaselyData {
    class SoftSignoutData {
        public string caseNum { get; set; }
        public string registrationDateTime { get; set; }
        public DateTime enteredDateTime { get; set; }
        public string residentID { get; set; }
        public string residentReportSectionCode { get; set; }
        public string residentSectionText { get; set; }
        public string attendingInterpretationText { get; set; } 
        public string attendingResultText { get; set; } 
        public string attendingCommentText { get; set; } 
        public string attendingSynopticText { get; set; } 
        public string attendingID { get; set; }
    }

    class PathReportFields {
        public List<string> sectionCodes = new List<string>() { "RESLT", "INTER", "COMM", "XTUMO", };
        public string InterpretationText { get; set; } = "";
        public string ResultText { get; set; } = "";
        public string CommentText { get; set; } = "";
        public string TumorSynopticText { get; set; } = "";
        public PathReportFields() {

        }
        public PathReportFields(PathReportFields oldVersion) {
            foreach(var f in oldVersion.sectionCodes) {
                this.setFieldValue(oldVersion.getFieldValue(f),f);
            }
        }

        public void setFieldValue(string content, string sectionCode) {
            switch (sectionCode) {
                case "RESLT":
                    ResultText = content;
                    break;
                case "INTER":
                    InterpretationText = content;
                    break;
                case "COMM":
                    CommentText = content;
                    break;
                case "XTUMO":
                    TumorSynopticText = content;
                    break;
                default:
                    Debug.WriteLine($"Section code {sectionCode} is not handled. Ignoring.");
                    break;
            }
        }

        public string getFieldValue(string sectionCode) {
            switch (sectionCode) {
                case "RESLT":
                    return ResultText;
                case "INTER":
                    return InterpretationText;
                case "COMM":
                    return CommentText;
                case "XTUMO":
                    return TumorSynopticText;
                default:
                    Debug.WriteLine($"Section code {sectionCode} is not handled. Ignoring.");
                    return "";
            }
        }
    }


    public class SoftToCaselyConverter {
        RichTextBox rtx = new RichTextBox(); // this is used for rich text conversion. Is a global variable so we only create one instance of it.

        public List<CaseEntry> importSoftPathCSVData(string pathToSoftData) {
            List<CaseEntry> caseEntries = new List<CaseEntry>();
            List<SoftSignoutData> listSoftData = new List<SoftSignoutData>();

            // Extract the CSV data that was export from SoftPath.
            try {
                using (var stream = File.Open(pathToSoftData, FileMode.Open, FileAccess.Read)) {

                    
                    using (var reader = ExcelReaderFactory.CreateReader(stream)) {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration() {
                            UseColumnDataType = true,
                            ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() {
                                UseHeaderRow = true


                            }
                        });
                        var table = result.Tables[0];
                        foreach (DataRow r in table.Rows) {
                            string frmCaseNum = formatCaseNumber(r["ORDERNUMBER"].ToString() ?? string.Empty);
                            SoftSignoutData sft = new SoftSignoutData() {
                                caseNum = frmCaseNum,
                                registrationDateTime = r["ORDERREGISTRATIONDATE"].ToString() ?? string.Empty,
                                enteredDateTime = DateTime.Parse(r["ENTEREDDATE"].ToString() ?? string.Empty),
                                residentID = r["USERID"].ToString() ?? string.Empty,
                                residentReportSectionCode = r["REPORTSECTIONCODE"].ToString() ?? string.Empty,
                                residentSectionText = RichTextToPlainText(r["USERREPORT"].ToString()),
                                attendingInterpretationText = RichTextToPlainText(r["FINALDIAGNOSISRICHTEXT"].ToString() ?? string.Empty),
                                attendingResultText = RichTextToPlainText(r["FINALGROSSANDMICRORICHTEXT"].ToString() ?? string.Empty),
                                attendingCommentText = RichTextToPlainText(r["FINALCOMMENTRICHTEXT"].ToString() ?? string.Empty),
                                attendingSynopticText = RichTextToPlainText(r["FINALSYNOPTICRICHTEXT"].ToString() ?? string.Empty),
                                attendingID = r["SIGNOUTPATHOLOGIST"].ToString() ?? string.Empty
                            };
                            listSoftData.Add(sft);
                        }
                        

                    }
                }

            } catch (Exception ex) {
                throw new Exception("Cannot open file" + ex.Message);
            }

            // get all the distinct case numbers that were in the softpath export data
            var distCaseNums = listSoftData.GroupBy(x => x.caseNum).Select(y => y.First()).Distinct().Select(x => x.caseNum);
            foreach (var c in distCaseNums) {
                // get the entries for the current case
                var entriesCurrentCase = listSoftData.Where(x => x.caseNum == c).OrderBy(f => f.enteredDateTime);
                var entrieVersionByDateTime = entriesCurrentCase.Select(x => x.enteredDateTime).Distinct().OrderBy(x => x).ToList();
                var listResidentEntry = new List<PathReportFields>();

                // Each row of the softpath data contains the entire final report, therefore we just need to read last row.
                var lastSoftRowEntry = entriesCurrentCase.Last();
                var attendingEntry = new PathReportFields() {
                    InterpretationText = lastSoftRowEntry.attendingInterpretationText,
                    ResultText = lastSoftRowEntry.attendingResultText,
                    TumorSynopticText = lastSoftRowEntry.attendingSynopticText,
                    CommentText = lastSoftRowEntry.attendingCommentText
                };
                // Get the attending version of the report
                CaseEntry attendCE = new CaseEntry() {
                    DateTimeModifiedObject = lastSoftRowEntry.enteredDateTime.AddHours(1), // make the attending report added later than the resident report
                    CaseNumber = lastSoftRowEntry.caseNum,
                    Interpretation = attendingEntry.InterpretationText,
                    Result = attendingEntry.ResultText,
                    Comment = attendingEntry.CommentText,
                    TumorSynoptic = attendingEntry.TumorSynopticText,
                    AuthorID = lastSoftRowEntry.attendingID
                };
                

                // Iterate through each version of the resident report grouped by date.
                foreach (var entryDateTime in entrieVersionByDateTime) {
                    var newRE = listResidentEntry.Count == 0 ? new PathReportFields() : new PathReportFields(listResidentEntry.Last());
                    var residentVersions = entriesCurrentCase.Where(x => x.enteredDateTime == entryDateTime);
                    foreach (var sectCode in newRE.sectionCodes) {
                        var sectText = residentVersions.Where(x => x.residentReportSectionCode == sectCode)
                            .Select(x => x.residentSectionText).DefaultIfEmpty(null).First();
                        if (sectText != null) {
                            newRE.setFieldValue(sectText, sectCode);
                        }
                    }

                    CaseEntry resCE = new CaseEntry() {
                        DateTimeModifiedObject = entryDateTime,
                        CaseNumber = lastSoftRowEntry.caseNum,
                        Interpretation = newRE.InterpretationText,
                        Result = newRE.ResultText,
                        Comment = newRE.CommentText,
                        TumorSynoptic = newRE.TumorSynopticText,
                        AuthorID = lastSoftRowEntry.residentID
                    };
                    listResidentEntry.Add(newRE);
                    caseEntries.Add(resCE);
                }

                caseEntries.Add(attendCE); // Add attending report after resident version to keep it sorted for debuggin



            }
            return caseEntries;

            
        }

        public string formatCaseNumber(string caseNumber) {
            Superpower.TextParser<string> formatCaseNum =
                from caseType in Character.Letter.Many()
                from year in Character.Digit.Repeat(2)
                from caseNum in Character.Digit.Many()
                select $"{new String(caseType)}-{new String(year)}-{new String(caseNum).TrimStart('0')}";
            var formCaseNum = formatCaseNum.Parse(caseNumber);
            return formCaseNum;
        }
            
        /// <summary>
        /// Converts the rich text into a string. A slow function since it must use the rich textbox control.
        /// </summary>
        /// <param name="richTxt"></param>
        /// <returns></returns>
        public string RichTextToPlainText(string richTxt) {
            rtx.Rtf = richTxt;
            return rtx.Text;
        }        
         
    }
}
