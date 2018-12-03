using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CaselyData;
using DiffMatchPatch;

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
        public ObservableCollection<Staff> listStaff = new ObservableCollection<Staff>();
        private ObservableCollection<CaseEntry> listFilteredCaseEntry = new ObservableCollection<CaseEntry>();

        public WindowSelfEvaluation() {
            InitializeComponent();
            RefreshPartDiagnosis();
            refreshSuggestions();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // load the names of authors
            listStaff = new ObservableCollection<Staff>(SqliteDataAcces.GetListAuthor());
            cmbAuthor.ItemsSource = listStaff;
            cmbAuthor.SelectedValuePath = "AuthorID";
            cmbCaseNumber.ItemsSource = listFilteredCaseEntry;
            cmbCaseNumber.SelectedValuePath = "CaseNumber";
            dtFilterDate.Text = "";
            RefreshCaseList();
            if (cmbCaseNumber.Items.Count > 0) cmbCaseNumber.SelectedIndex = 0;
            cmbService.ItemsSource = suggestionService;
            cmbSelfEvaluation.ItemsSource = suggestionEvaluation;
        }


        private void RefreshCaseList() {
            // Filter by date
            var listFilteredCE = dtFilterDate.Text != "" ?  SqliteDataAcces.FilterCaseEntryDateModified(DateTime.Parse(dtFilterDate.Text)) : SqliteDataAcces.GetListAllCaseEntries();
            // Filter by Author ID
            if (cmbAuthor.SelectedIndex != -1) {
                var listFilter = SqliteDataAcces.GetCaseEntryFilterAuthorID(cmbAuthor.SelectedValue.ToString());
                listFilteredCE = listFilteredCE.Where(x => listFilter.ToList()
                                        .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();
            }
            listFilteredCaseEntry.Clear();
            cmbCaseNumber.SelectedValuePath = "CaseNumber";
            foreach (var s in listFilteredCE) {
                if (chkFilterCompleted.IsChecked == false || !(SqliteDataAcces.CaseEntryEvaluated(s.CaseNumber))) {
                    listFilteredCaseEntry.Add(s);
                }
            }

            if (listFilteredCaseEntry.Count != 0) {
                cmbCaseNumber.SelectedIndex = 0;
            }
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
            RefreshCaseList();
        }

        /// <summary>
        /// Loads the diagnosis for the selected case in to the UI.
        /// </summary>
        private void RefreshPartDiagnosis() {
            List<PartDiagnosis> listPartDiagnosis = SqliteDataAcces.GetListPartDiagnosisLatestVersion(cmbCaseNumber.Text);
            clearDiagnosisControls();
            // Check if a valid case number is selected
            string cmbCaseText = cmbCaseNumber.Text;
            if (cmbCaseText != "" && cmbCaseText != SqliteDataAcces.DbConnectionString) {
                btnAddDiagnosis.IsEnabled = true;
                // create new ones from the partDiagnosis loaded from the database
                foreach (var p in listPartDiagnosis) {
                    spPartDiagnosis.Children.Add(new UCdiagnosis(p, suggestionOrgan, suggestionOrganSystem, suggestionCategory, suggestionDiagnosis));
                }
            } else {
                // blank case number entered, prevent addition of diagnosis
                btnAddDiagnosis.IsEnabled = false;
            }
        }
        /// <summary>
        /// clear old UC controls
        /// </summary>
        private void clearDiagnosisControls() {            
            for (int i = 0; i < spPartDiagnosis.Children.Count; i++) {
                if (spPartDiagnosis.Children[i] is UCdiagnosis) {
                    spPartDiagnosis.Children.RemoveAt(i);
                    i--; // go back one since we removed a control
                }
            }
        }

        private void AddPartDiagnosis() {
            if (spPartDiagnosis.Children.Count > 1) {
                UCdiagnosis lastPart = spPartDiagnosis.Children[spPartDiagnosis.Children.Count - 1] as UCdiagnosis;
                char lstParLetter = lastPart.tbPart.Text.Length != 0 ? lastPart.tbPart.Text[0] : 'A';
                UCdiagnosis newPart = new UCdiagnosis(new PartDiagnosis {
                    // keep the same part letter as preveious entry to allow multiple diagnosis for the same part
                    Part = (lstParLetter).ToString(),
                    Organ = lastPart.cmbOrgan.Text,
                    OrganSystem = lastPart.cmbOrganSystem.Text
                }, suggestionOrgan, suggestionOrganSystem, suggestionCategory, suggestionDiagnosis);
                spPartDiagnosis.Children.Add(newPart);
            } else {
                UCdiagnosis newPart = new UCdiagnosis(new PartDiagnosis {
                    Part = "A",
                    Organ = "",
                    OrganSystem = "",
                    Diagnosis = "",
                    DiagnosisDetailed = "",
                }, suggestionOrgan, suggestionOrganSystem, suggestionCategory, suggestionDiagnosis);
                spPartDiagnosis.Children.Add(newPart);
            }

        }

        private void btnAddDiagnosis_Click(object sender, RoutedEventArgs e) {
            AddPartDiagnosis();
        }

        private void btnSubmitDiagnosis_Click(object sender, RoutedEventArgs e) {
            submitDiagnosis();
        }

        private void submitDiagnosis() {
            List<PartDiagnosis> partsToAdd = new List<PartDiagnosis>();
            // get the current date and time to save the same modified time for all parts being added to the database
            DateTime currentTime = DateTime.Now;
            foreach (var p in spPartDiagnosis.Children) {
                if (p is UCdiagnosis) {
                    var pt = (UCdiagnosis)p;
                    PartDiagnosis newPartDiagnosis = new PartDiagnosis() {
                        Part = pt.partDiagnosis.Part,
                        OrganSystem = pt.partDiagnosis.OrganSystem,
                        Organ = pt.partDiagnosis.Organ,
                        Diagnosis = pt.partDiagnosis.Diagnosis,
                        DiagnosisDetailed = pt.partDiagnosis.DiagnosisDetailed,
                        Category = pt.partDiagnosis.Category,
                        DateModifiedString = currentTime.ToString("yyyy-MM-dd"),
                        TimeModifiedString = currentTime.ToString("HH:mm:ss"),
                        CaseNumber = cmbCaseNumber.Text
                    };
                    partsToAdd.Add(newPartDiagnosis);
                }
            }
            PathCase pathCase = new PathCase() { CaseNumber = cmbCaseNumber.SelectedValue.ToString(), Service = cmbService.Text, Evaluation = cmbSelfEvaluation.Text };
            // save the assigned diagnosis to the case
            SqliteDataAcces.InsertNewPartDiagnosisEntry(partsToAdd, pathCase);

            // Save the evaluation and other data for the case, essentially completing it.
            SqliteDataAcces.UpdateCompletedCase(pathCase);
            cmbSelfEvaluation.Text = "";
            var indx = cmbCaseNumber.SelectedIndex;
            listFilteredCaseEntry.RemoveAt(indx);
            cmbCaseNumber.SelectedIndex = cmbCaseNumber.Items.Count == 0 ? 0 : cmbCaseNumber.Items.Count - 1;
            cmbService.Focus();
        }

        private void chkFilterCompleted_Click(object sender, RoutedEventArgs e) {
            RefreshCaseList();
        }

        private void refreshCaseData() {
            RefreshComparison();
            RefreshPartDiagnosis();
            var cn = cmbCaseNumber.SelectedValue;
            if (cn != null) {
                var pathCase = SqliteDataAcces.GetPathCase(cn.ToString());
                cmbSelfEvaluation.Text = pathCase.Evaluation;
                cmbService.Text = pathCase.Service;

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
                clearDiagnosisControls();
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
            RefreshCaseList();
        }

        private void cmbVersion_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            refreshCaseData();
        }
    }

}
