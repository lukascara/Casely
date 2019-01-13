using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CaselyData;
using DiffMatchPatch;
using System.Diagnostics;

namespace Casely {
    /// <summary>
    /// Interaction logic for WindowDiagnosis.xaml
    /// </summary>
    public partial class WindowSelfEvaluation : Window {

        private List<string> suggestionOrgan = new List<string>();
        private List<string> suggestionOrganSystem = new List<string>();
        private List<string> suggestionDiagnosis = new List<string>();
        private List<string> suggestionCategory = new List<string>();
        private List<string> suggestionService = new List<string>();
        private List<string> suggestionEvaluation = new List<string>();
        private List<PathCase> listAllPathCase = new List<PathCase>();
        private List<CaseEntry> listAllCaseEntry = new List<CaseEntry>();
        public ObservableCollection<Staff> listStaff = new ObservableCollection<Staff>();
        private ObservableCollection<CaseEntry> listFilteredCaseEntry = new ObservableCollection<CaseEntry>();

        public WindowSelfEvaluation() {
            InitializeComponent();
            refreshSuggestions();
            this.DataContext = this;
        }
        
        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            // load the names of authors
            listStaff = new ObservableCollection<Staff>(SqliteDataAcces.GetListAuthor());
            cmbAuthor.ItemsSource = listStaff;
            cmbAuthor.SelectedValuePath = "AuthorID";
            cmbCaseNumber.ItemsSource = listFilteredCaseEntry;
            cmbCaseNumber.SelectedValuePath = "CaseNumber";
            dtFilterDate.Text = "";
            // load all the cases from the database
            listAllCaseEntry = await GetAllCaseEntryAsync();
            listAllPathCase = await GetAllPathCaseAsync();
            RefreshCaseListUI(listAllCaseEntry);
            cmbService.ItemsSource = suggestionService;
            cmbSelfEvaluation.ItemsSource = suggestionEvaluation;
            ApplyFiltersToCaseListAndRefresh();

        }


        private async Task<List<CaseEntry>> GetAllCaseEntryAsync() {
            var result = await Task.Run(() => SqliteDataAcces.GetListAllCaseEntries());
            return result; 
        }

        private async Task<List<PathCase>> GetAllPathCaseAsync() {
            var result = await Task.Run(() => SqliteDataAcces.GetAllPathCase());
            return result;
        }

        private void ApplyFiltersToCaseListAndRefresh() {
            var listFilteredCases = listAllCaseEntry.ToList();
            if (chkOnlyShowUncompleted.IsChecked == true) {
                listFilteredCases = listAllCaseEntry.Where(x => !(IsCaseEvaluated(x))).ToList();
            }
           
            // Filter by date
            listFilteredCases = dtFilterDate.Text != "" ? listFilteredCases.Where(x => x.DateTimeModifiedObject.Date >= dtFilterDate.Value).ToList() : listFilteredCases.ToList();
            var listResults = new List<CaseEntry>();
            // Filter by Author ID
            if (cmbAuthor.SelectedIndex != -1) {
                var listFilter = SqliteDataAcces.GetCaseEntryFilterAuthorID(cmbAuthor.SelectedValue.ToString());
                listFilteredCases = listFilteredCases.Where(x => listFilter.ToList()
                                        .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();
            }

            // Filter by case number, this trumps all filters and will return even if the case does not satisfy the other filters
            var stCaseNum = txtFilterCaseNumber.Text;
            if (stCaseNum != "") {
                listFilteredCases = listAllCaseEntry.Where(x => x.CaseNumber == stCaseNum).ToList();
            }

            RefreshCaseListUI(listFilteredCases);
           
        }

        private bool IsCaseEvaluated(CaseEntry CE) {
            var xy = listAllPathCase.Where(x => x.CaseNumber == CE.CaseNumber).Where(y => y.Evaluation != null);
            return xy.Count() != 0;
        }

        /// <summary>
        /// This method loads all the cases into the dropdownlist
        /// </summary>
        /// <param name="listCaseEntries"></param>
        private void RefreshCaseListUI(List<CaseEntry> listCaseEntries) {
            listFilteredCaseEntry.Clear();
            foreach (var s in listCaseEntries) {               
                    listFilteredCaseEntry.Add(s);
            }
            if (cmbCaseNumber.Items.Count > 0) cmbCaseNumber.SelectedIndex = 0;
        }

        public void refreshSuggestions() {
            List<string> standardSuggestOS = new List<string>();
            standardSuggestOS.Add("Gastrointestinal");
            standardSuggestOS.Add("Gynecological");
            standardSuggestOS.Add("Cardiovascular");
            standardSuggestOS.Add("Dermatological");
            standardSuggestOS.Add("Genitourinary");
            standardSuggestOS.Add("Hematological");
            standardSuggestOS.Add("Bone and soft tissue");
            standardSuggestOS.Add("Hepatobiliary");
            standardSuggestOS.Add("Nervous");
            standardSuggestOS.Add("Pulmonary");
            standardSuggestOS.Add("Endocrine");
            suggestionOrganSystem = new List<string>(SqliteDataAcces.GetListOrganSystem());
            foreach (string st in standardSuggestOS) {
                if (suggestionOrganSystem.IndexOf(st) == -1) {
                    suggestionOrganSystem.Add(st);
                }
            }
            // adds standard suggestions for diagnostic category
            List<string> standardSuggestCategory = new List<string>();
            standardSuggestCategory.Add("Benign");
            standardSuggestCategory.Add("Malignant");
            standardSuggestCategory.Add("Infectious");
            standardSuggestCategory.Add("Normal");
            suggestionCategory = new List<string>(SqliteDataAcces.GetUniqueDiagnosticCategory());
            foreach (string st in standardSuggestCategory) {
                if (suggestionCategory.IndexOf(st) == -1) {
                    suggestionCategory.Add(st);
                }
            }
            

            suggestionDiagnosis = new List<string>(SqliteDataAcces.GetUniqueDiagnosis());
            suggestionOrgan = new List<string>(SqliteDataAcces.GetListOrgan());
            suggestionEvaluation = new List<string>(SqliteDataAcces.GetUniqueEvaluations());
            suggestionService = new List<string>(SqliteDataAcces.GetUniqueService());
            var listeval = new List<string>() { "NA", "1 - Style changes" ,
                "2 - Grammar and spelling", "3 - Interpretation - Minor diagnostic alteration","3 - Microscopic - Minor diagnostic alteration",
             "4 - Interpretation - Major diagnostic alteration", "4 - Interpretation - Major diagnostic alteration", "1 - Perfect"};
            foreach (var lv in listeval) {
                if (!(suggestionEvaluation.Contains(lv))) {
                    suggestionEvaluation.Add(lv);
                }
            }
           suggestionEvaluation.Sort();

            var listService = new List<string>() { "Routine", "Frozen", "Biopsy"};
            foreach (var lv in listService) {
                if (!(suggestionService.Contains(lv))) {
                    suggestionService.Add(lv);
                }
            }

        }

        private void cmbAuthor_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ApplyFiltersToCaseListAndRefresh();
        }

      

        private void btnSubmitDiagnosis_Click(object sender, RoutedEventArgs e) {
            submitEvaluation();
            txtStatus.Text = "Evaluation saved";
        }

        private void submitEvaluation() {
            PathCase pathCase = new PathCase() { CaseNumber = cmbCaseNumber.SelectedValue.ToString(), Service = cmbService.Text, Evaluation = cmbSelfEvaluation.Text, EvaluationComment = txtSelfEvalComments.Text };
            // Save the evaluation and other data for the case, essentially completing it.
            SqliteDataAcces.UpdateCompletedCase(pathCase);
            // Update in-memory list of pathCases, that way do not have to go back to the database again to load.
            foreach (var pc in listAllPathCase) {
                if (pc.CaseNumber == pathCase.CaseNumber) {
                    pc.Evaluation = pathCase.Evaluation;
                    pc.EvaluationComment = pathCase.EvaluationComment;
                    pc.Service = pathCase.Service;
                }
            }
            listAllPathCase = listAllPathCase.Where(x => x.CaseNumber == pathCase.CaseNumber).Select(c => { c.EvaluationComment = cmbSelfEvaluation.Text; return c; }).ToList();
            cmbSelfEvaluation.Text = "";
            var indx = cmbCaseNumber.SelectedIndex;
            cmbService.Focus();
            ApplyFiltersToCaseListAndRefresh();
        }

        private void chkFilterCompleted_Click(object sender, RoutedEventArgs e) {
            ApplyFiltersToCaseListAndRefresh();
        }

        private void refreshCaseData() {
            RefreshComparison();
            var cn = cmbCaseNumber.SelectedValue;
            if (cn != null) {
                var pathCase = SqliteDataAcces.GetPathCase(cn.ToString());
                cmbSelfEvaluation.Text = pathCase.Evaluation != null ? pathCase.Evaluation : "";
                txtSelfEvalComments.Text = pathCase.EvaluationComment != null ? pathCase.EvaluationComment : "";
                cmbService.Text = pathCase.Service != null ? pathCase.Service : "";
            }
        }



        /// <summary>
        /// Loads the last two versions of the report. Use LoadCaseData to refresh UI as it loads the diagnosis for the case as well.
        /// </summary>
        private void RefreshComparison() {
              if (cmbCaseNumber.SelectedValue != null && cmbVersion.SelectedIndex != -1) {
                wbDiffText.Text = "";
                string html = "";
                var listCase = SqliteDataAcces.GetListCaseEntry(cmbCaseNumber.SelectedValue.ToString());

                // gets the case entrys, groups them by author and then selects the last two author entries to compare.
                var dt =  DateTime.Parse(cmbVersion.SelectedItem.ToString());
                CaseEntry residentEntry = listCase.Where(x => x.DateTimeModifiedObject == dt).First();
                CaseEntry attendingEntry = listCase.Last();
                

                html += DiffToHTML(residentEntry.Interpretation, attendingEntry.Interpretation, "Interpretation");
                html += DiffToHTML(residentEntry.Material, attendingEntry.Material, "Material");
                html += DiffToHTML(residentEntry.History, attendingEntry.History, "History");
                html += DiffToHTML(residentEntry.Gross, attendingEntry.Gross, "Gross");
                html += DiffToHTML(residentEntry.Microscopic, attendingEntry.Microscopic, "Microscopic");
                html += DiffToHTML(residentEntry.TumorSynoptic, attendingEntry.TumorSynoptic, "Tumor Synoptic");
                html += DiffToHTML(residentEntry.Comment, attendingEntry.Comment, "Comment");   
                html = "<head><style>INS {background-color: powderblue;}DEL  {color: #ff5151;}</style></head>" + html;
               
                wbDiffText.Text = html;
            } else {
                wbDiffText.Text = "";
            }
        }

        private string DiffToHTML(string version0, string version1, string header) {
            var dmp = DiffMatchPatchModule.Default;
            var diffs = dmp.DiffMain(version0, version1);
            dmp.DiffCleanupSemantic(diffs);
            var html = dmp.DiffPrettyHtml(diffs).Replace("&para;", "");
            return  $"<h3><u>{header}</u></h3>"+ html;
        }
       
       
        private void cmbCaseNumber_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            btnPreviousCase.IsEnabled = cmbCaseNumber.SelectedIndex > 0;
            btnNextCase.IsEnabled = cmbCaseNumber.SelectedIndex < cmbCaseNumber.Items.Count - 1;
            if (cmbCaseNumber.SelectedIndex == -1) {
                return;
            }
            var listDateModified = SqliteDataAcces.GetCaseEntryListDateTimeModified(cmbCaseNumber.SelectedValue.ToString());
            cmbVersion.Items.Clear();
            foreach (var l in listDateModified) {
                cmbVersion.Items.Add(l.ToString("MM-dd-yyyy HH:mm:ss"));
            }
            cmbVersion.SelectedIndex = cmbVersion.Items.Count - 1;
            refreshCaseData();
        }

        private void dtFilterDate_LostFocus(object sender, RoutedEventArgs e) {
            ApplyFiltersToCaseListAndRefresh();
        }


        private void cmbVersion_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            refreshCaseData();
            lbReportVersion.Content = SelectedVersionFormated;
        }

        private string SelectedVersionFormated {
            get {
                var selIndexPlusOne = cmbVersion.SelectedIndex != -1 ? cmbVersion.SelectedIndex + 1 : 0;
                return $"Version ({selIndexPlusOne}/{cmbVersion.Items.Count})";
            }
        }

        private void btnNextCase_Click(object sender, RoutedEventArgs e) {
            submitEvaluation();
            if (cmbCaseNumber.SelectedIndex < cmbCaseNumber.Items.Count - 1) {
                cmbCaseNumber.SelectedIndex = cmbCaseNumber.SelectedIndex + 1;
            }
            txtStatus.Text = "";
        }

        private void btnPreviousCase_Click(object sender, RoutedEventArgs e) {
            submitEvaluation();
            if (cmbCaseNumber.SelectedIndex > 0) {
                cmbCaseNumber.SelectedIndex = cmbCaseNumber.SelectedIndex - 1;
            }
            txtStatus.Text = "";

        }

        private void txtFilterCaseNumber_TextChanged(object sender, TextChangedEventArgs e) {
            var locCaret = txtFilterCaseNumber.CaretIndex;
            txtFilterCaseNumber.Text = txtFilterCaseNumber.Text.ToUpper();
            txtFilterCaseNumber.CaretIndex = locCaret;
            txtStatus.Text = "";
        }

        private void txtFilterCaseNumber_LostFocus(object sender, RoutedEventArgs e) {
            ApplyFiltersToCaseListAndRefresh();
            txtStatus.Text = "";
        }
    }

}
