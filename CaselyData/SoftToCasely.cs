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
        public string CaseNum { get; set; }
        public string RegistrationDateTime { get; set; }
        public DateTime EnteredDateTime { get; set; }
        public string ResidentID { get; set; }
        public string ResidentInterpretationText { get; set; }
        public string ResidentResultText { get; set; }
        public string ResidentCommentText { get; set; }
        public string ResidentSynopticText { get; set; }

        public string AttendingInterpretationText { get; set; } 
        public string AttendingResultText { get; set; } 
        public string AttendingCommentText { get; set; } 
        public string AttendingSynopticText { get; set; } 
        public string AttendingID { get; set; }
    }
    
    /// <summary>
    /// This class is used by Casely to import SoftPath data from an excel file
    /// </summary>
    public class SoftPathExcelConvert {
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
                            string frmCaseNum = r["FORMATTEDORDERNUMBER"].ToString() ?? string.Empty;
                            SoftSignoutData sft = new SoftSignoutData() {
                                CaseNum = frmCaseNum,
                                RegistrationDateTime = r["ORDERREGISTRATIONDATE"].ToString() ?? string.Empty,
                                EnteredDateTime = DateTime.Parse(r["ENTEREDDATE"].ToString() ?? string.Empty),
                                ResidentID = r["USERID"].ToString() ?? string.Empty,

                                ResidentInterpretationText = RichTextToPlainText(r["USERINTERPRETATION"].ToString() ?? string.Empty),
                                ResidentResultText = RichTextToPlainText(r["USERGROSSANDMICRO"].ToString() ?? string.Empty),
                                ResidentCommentText = RichTextToPlainText(r["USERCOMMENT"].ToString() ?? string.Empty),
                                ResidentSynopticText = RichTextToPlainText(r["USERSYNOPTIC"].ToString() ?? string.Empty),

                                AttendingInterpretationText = RichTextToPlainText(r["FINALDIAGNOSISRICHTEXT"].ToString() ?? string.Empty),
                                AttendingResultText = RichTextToPlainText(r["FINALGROSSANDMICRORICHTEXT"].ToString() ?? string.Empty),
                                AttendingCommentText = RichTextToPlainText(r["FINALCOMMENTRICHTEXT"].ToString() ?? string.Empty),
                                AttendingSynopticText = RichTextToPlainText(r["FINALSYNOPTICRICHTEXT"].ToString() ?? string.Empty),
                                AttendingID = r["SIGNOUTPATHOLOGIST"].ToString() ?? string.Empty
                            };
                            listSoftData.Add(sft);
                        }                 

                    }
                }

            } catch (Exception ex) {
                throw new Exception("Cannot open file" + ex.Message);
            }

            // get all the distinct case numbers that were in the softpath export data
            var distCaseNums = listSoftData.GroupBy(x => x.CaseNum).Select(y => y.First()).Distinct().Select(x => x.CaseNum);
            foreach (var c in distCaseNums) {
                // get the entries for the current case
                var entriesCurrentCase = listSoftData.Where(x => x.CaseNum == c).OrderBy(f => f.EnteredDateTime);

                // Each row of the softpath data contains the attending final report, therefore we only need one version
                // We have to use the last version so that when we adjust the time, the date is correct (i.e a report that was modified on multiple days by the resident)
                var lastReportVersion = entriesCurrentCase.Last();
                // Get the attending version of the report
                CaseEntry attendCE = new CaseEntry() {
                    DateTimeModifiedObject = lastReportVersion.EnteredDateTime.AddHours(1), // make the attending report added later than the resident report
                    CaseNumber = lastReportVersion.CaseNum,
                    Interpretation = lastReportVersion.AttendingInterpretationText,
                    Comment = lastReportVersion.AttendingCommentText,
                    TumorSynoptic = lastReportVersion.AttendingSynopticText,
                    Result = lastReportVersion.AttendingResultText,    
                    AuthorID = lastReportVersion.AttendingID
                };

                // Iterate through each version of the resident report
                List<CaseEntry> residentVersions = new List<CaseEntry>(); // This variable temporarily stores list of resident report versions
                foreach (var curReportVersion in entriesCurrentCase) {
                    CaseEntry resCE = new CaseEntry() {
                        DateTimeModifiedObject = curReportVersion.EnteredDateTime,
                        CaseNumber = curReportVersion.CaseNum,
                        Interpretation = curReportVersion.ResidentInterpretationText,
                        Result = curReportVersion.ResidentResultText,
                        Comment = curReportVersion.ResidentCommentText,
                        TumorSynoptic = curReportVersion.ResidentSynopticText,
                        AuthorID = curReportVersion.ResidentID
                    };
                    residentVersions.Add(resCE);
                }
                // Each version of the resident report only stores fields that were changed.
                // For example, if only the interpretation was changed and the report save, than result, comment, tumor synoptic will be blank
                // This for loop fixes that problem by 'updating' even non-saved fields
                for (int i =1; i < residentVersions.Count; i++) {
                    if (residentVersions[i-1].Interpretation != "" && residentVersions[i].Interpretation == "") {
                        residentVersions[i].Interpretation = residentVersions[i-1].Interpretation;
                    }
                    if (residentVersions[i - 1].Result != "" && residentVersions[i].Result == "") {
                        residentVersions[i].Result = residentVersions[i - 1].Result;
                    }
                    if (residentVersions[i - 1].Comment != "" && residentVersions[i].Comment == "") {
                        residentVersions[i].Comment = residentVersions[i - 1].Comment;
                    }
                    if (residentVersions[i - 1].TumorSynoptic != "" && residentVersions[i].TumorSynoptic == "") {
                        residentVersions[i].TumorSynoptic = residentVersions[i - 1].TumorSynoptic;
                    }
                    
                }
                caseEntries.AddRange(residentVersions);
                caseEntries.Add(attendCE); // Add attending report after resident version to keep it sorted for debugging                                
            }
            return caseEntries;            
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
