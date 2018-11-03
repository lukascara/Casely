using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using System.IO;
using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;
using Superpower.Model;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;

namespace CaselyData {
    class SoftSignoutData {
        public string caseNum { get; set; }
        public string registrationDateTime { get; set; }
        public string enteredDateTime { get; set; }
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
    }


    public class SoftToCaselyConverter {
        
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
                            SoftSignoutData sft = new SoftSignoutData() {
                                caseNum = formatCaseNumber((string)r["ORDERNUMBER"]),
                                registrationDateTime = (string)r["ORDERREGISTRATIONDATE"],
                                enteredDateTime = (string)r["ENTEREDDATE"],
                                residentID = (string)r["USERID"],
                                residentReportSectionCode = (string)r["REPORTSECTIONCODE"],
                                residentSectionText = RichTextToPlainText((string)r["USERREPORT"]),
                                attendingInterpretationText = RichTextToPlainText(r["FINALDIAGNOSISRICHTEXT"].ToString() ?? string.Empty),
                                attendingResultText = RichTextToPlainText(r["FINALGROSSANDMICRORICHTEXT"].ToString() ?? string.Empty),
                                attendingCommentText = RichTextToPlainText(r["FINALCOMMENTRICHTEXT"].ToString() ?? string.Empty),
                                attendingSynopticText = RichTextToPlainText(r["FINALSYNOPTICRICHTEXT"].ToString() ?? string.Empty),
                                attendingID = (string)r["SIGNOUTPATHOLOGIST"].ToString() ?? string.Empty
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
                var entriesCurrentCase = listSoftData.Where(x => x.caseNum == c).OrderByDescending(f => f.enteredDateTime);
                var residentEntry = new PathReportFields();
               
                // Go through each section of the report and find the corresponding row in the exported softpath data
                // for the residents report.
                // Since the data is sorted by descending date, we can return the first matching value for that section
                foreach (var sectCode in residentEntry.sectionCodes) {
                    var sectText = entriesCurrentCase.Where(x => x.residentReportSectionCode == sectCode)
                        .Select(x=> x.residentSectionText).DefaultIfEmpty("").First();
                    residentEntry.setFieldValue(sectText, sectCode);
                }
                // Each row of the softpath data contains the entire final report, therefore we just need to read 
                // the first copy.
                var firstSoftRow = entriesCurrentCase.First();
                var attendingEntry = new PathReportFields() {
                    InterpretationText = firstSoftRow.attendingInterpretationText,
                    ResultText = firstSoftRow.attendingResultText,
                    TumorSynopticText = firstSoftRow.attendingSynopticText,
                    CommentText = firstSoftRow.attendingCommentText
                };

                CaseEntry resCE = new CaseEntry() {
                    DateTimeModifiedObject = DateTime.Parse(firstSoftRow.enteredDateTime),
                    CaseNumber = firstSoftRow.caseNum,
                    Interpretation = residentEntry.InterpretationText,
                    Result = residentEntry.ResultText,
                    Comment = residentEntry.CommentText,
                    TumorSynoptic = residentEntry.TumorSynopticText,
                    SoftID = firstSoftRow.residentID
                };

                CaseEntry attendCE = new CaseEntry() {
                    DateTimeModifiedObject = DateTime.Parse(firstSoftRow.enteredDateTime),
                    CaseNumber = firstSoftRow.caseNum,
                    Interpretation = attendingEntry.InterpretationText,
                    Result = attendingEntry.ResultText,
                    Comment = attendingEntry.CommentText,
                    TumorSynoptic = attendingEntry.TumorSynopticText,
                    SoftID = firstSoftRow.attendingID
                };
                caseEntries.Add(attendCE);
                caseEntries.Add(resCE);
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
        /// 
        /// </summary>
        /// <param name="richTxt"></param>
        /// <returns></returns>
        public string RichTextToPlainText(string richTxt) {
            RichTextBox rtx = new RichTextBox();
            rtx.Rtf = richTxt;
            return rtx.Text;
        }        
         
    }
}
