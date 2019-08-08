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
using System.Windows.Input;

namespace Casely {
    /// <summary>
    /// Interaction logic for WindowDiagnosis.xaml
    /// </summary>
    public partial class WindowSelfEvaluation : Window {

        private List<string> suggestionOrgan = new List<string>();
        private List<string> suggestionOrganSystem = new List<string>();
        private List<string> suggestionDiagnosis = new List<string>();
        private List<string> suggestionCategory = new List<string>();
        private ObservableCollection<string> suggestionService = new ObservableCollection<string>();
        private ObservableCollection<string> suggestionEvaluation = new ObservableCollection<string>();
        private List<CaselyUserData> listAllCaselyUserData = new List<CaselyUserData>();
        private List<CaseEntry> listAllCaseEntry = new List<CaseEntry>();
        public ObservableCollection<Staff> listStaff = new ObservableCollection<Staff>();
        private ObservableCollection<CaseEntry> listFilteredCaseEntry = new ObservableCollection<CaseEntry>();
        private bool allCasesLoaded = false; // changes to true after all cases are loaded so that the user interface can be correctly updated
        private bool disableVersionChangeevent = true;

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
            wbDiffText.Text = "<h3><b>Casely is loading! Please be patient :)</b></h3>";
            // load all the cases from the database
            listAllCaseEntry = await GetAllCaseEntryAsync();
            listAllCaselyUserData = await GetAllCaselyUserDataAsync();
            RefreshCaseListUI(listAllCaseEntry);
            cmbService.ItemsSource = suggestionService;
            cmbSelfEvaluation.ItemsSource = suggestionEvaluation;
            allCasesLoaded = true;
            ApplyFiltersToCaseListAndRefresh();

            // instantiate keyboard shortcuts
            RoutedCommand cmndSettingnext = new RoutedCommand();
            cmndSettingnext.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(cmndSettingnext, btnNextCase_Click));

            RoutedCommand cmndSettingPrev = new RoutedCommand();
            cmndSettingPrev.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(cmndSettingPrev, btnPreviousCase_Click));

            RoutedCommand cmndSettingService = new RoutedCommand();
            cmndSettingService.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(cmndSettingService, focusService));

            RoutedCommand cmndSettingEval = new RoutedCommand();
            cmndSettingEval.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(cmndSettingEval, focusEvaluation));

            RoutedCommand cmndSettingEvalComments = new RoutedCommand();
            cmndSettingEvalComments.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(cmndSettingEvalComments, focusSelfEvalComments));

        }


        private async Task<List<CaseEntry>> GetAllCaseEntryAsync() {
            var result = await Task.Run(() => SqliteDataAcces.GetListAllCaseEntries());
            return result; 
        }

        private async Task<List<CaselyUserData>> GetAllCaselyUserDataAsync() {
            var result = await Task.Run(() => SqliteDataAcces.GetAllCaselyUserData());
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
            var xy = listAllCaselyUserData.Where(x => x.CaseNumber == CE.CaseNumber).Where(y => y.Evaluation != null && y.Evaluation.TrimStart(' ') != "");
            return xy.Count() != 0;
        }

        /// <summary>
        /// This method loads all the cases into the dropdownlist
        /// </summary>
        /// <param name="listCaseEntries"></param>
        private void RefreshCaseListUI(List<CaseEntry> listCaseEntries) {
            if (allCasesLoaded) {
                var oldSelectedIndex = cmbCaseNumber.SelectedIndex; // store the old selected index before we reload the combobox with the new list
                var oldCountFilteredCount = listFilteredCaseEntry.Count;
                listFilteredCaseEntry.Clear();
                foreach (var s in listCaseEntries) {               
                        listFilteredCaseEntry.Add(s);
                }
                // select the first case if a case exists, and none is currently selected
                if (oldSelectedIndex == -1 && cmbCaseNumber.Items.Count > 0) {
                    cmbCaseNumber.SelectedIndex = 0;
                } else if (oldSelectedIndex < cmbCaseNumber.Items.Count) {
                    cmbCaseNumber.SelectedIndex = oldSelectedIndex;
                    // A case was completed, change the index so that the next case is correctly selected
                    if (1 == (oldCountFilteredCount - listFilteredCaseEntry.Count)) {
                        cmbCaseNumber.SelectedIndex -= 1;
                    }
                }
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
            

            suggestionDiagnosis = new List<string>(SqliteDataAcces.GetUniqueDiagnosis().Where(x => x.ToString().Trim() != ""));
            suggestionOrgan = new List<string>(SqliteDataAcces.GetListOrgan().Where(x => x.ToString().Trim() != ""));
            suggestionEvaluation = new ObservableCollection<string>(SqliteDataAcces.GetUniqueEvaluations().Where(x => x.ToString().Trim() != ""));
            suggestionService = new ObservableCollection<string>(SqliteDataAcces.GetUniqueService().Where(x => x.ToString().Trim() != ""));
            var listeval = new List<string>() { "NA", "1 - Style changes" ,
                "2 - Grammar and spelling", "3 - Interpretation - Minor diagnostic alteration","3 - Microscopic - Minor diagnostic alteration",
             "4 - Interpretation - Major diagnostic alteration", "4 - Interpretation - Major diagnostic alteration", "1 - Perfect"};
            foreach (var lv in listeval) {
                if (!(suggestionEvaluation.Contains(lv))) {
                    suggestionEvaluation.Add(lv);
                }
            }
           suggestionEvaluation = new ObservableCollection<string>(suggestionEvaluation.OrderBy(x => x.ToString()));

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
            CaselyUserData caselyUserData = new CaselyUserData() { CaseNumber = cmbCaseNumber.SelectedValue.ToString(), Service = cmbService.Text, Evaluation = cmbSelfEvaluation.Text, EvaluationComment = txtSelfEvalComments.Text };
            // Save the evaluation and other data for the case, essentially completing it.
            SqliteDataAcces.UpdateCompletedCase(caselyUserData);
            // Update in-memory list of caselyUserDatas, that way do not have to go back to the database again to load.
            foreach (var pc in listAllCaselyUserData) {
                if (pc.CaseNumber == caselyUserData.CaseNumber) {
                    pc.Evaluation = caselyUserData.Evaluation;
                    pc.EvaluationComment = caselyUserData.EvaluationComment;
                    pc.Service = caselyUserData.Service;
                }
            }
            cmbSelfEvaluation.Text = "";
            var indx = cmbCaseNumber.SelectedIndex;
           // cmbService.Focus();
            ApplyFiltersToCaseListAndRefresh();
        }

        private void chkFilterCompleted_Click(object sender, RoutedEventArgs e) {
            ApplyFiltersToCaseListAndRefresh();
        }


        private void refreshCaseData() {
            RefreshComparison();
            var cn = cmbCaseNumber.SelectedValue;
            var lastService = cmbService.Text; // save the last service so that we do not have to keep entering it for each eval
            if (cn != null) {
                var caselyUserData = SqliteDataAcces.GetCaselyUserData(cn.ToString());
                cmbSelfEvaluation.Text = caselyUserData.Evaluation != null ? caselyUserData.Evaluation : "";
                txtSelfEvalComments.Text = caselyUserData.EvaluationComment != null ? caselyUserData.EvaluationComment : "";
                cmbService.Text = caselyUserData.Service != null ? caselyUserData.Service : lastService;
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
            disableVersionChangeevent = true;
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
            disableVersionChangeevent = false;
        }

        private void dtFilterDate_LostFocus(object sender, RoutedEventArgs e) {
            ApplyFiltersToCaseListAndRefresh();
        }


        private void cmbVersion_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (disableVersionChangeevent == false){
                refreshCaseData();
            }
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
        
        private void focusService(object sender, RoutedEventArgs e) {
            cmbService.Focus();
        }

        private void focusEvaluation(object sender, RoutedEventArgs e) {
            cmbSelfEvaluation.Focus();
        }

        private void focusSelfEvalComments(object sender, RoutedEventArgs e) {
            txtSelfEvalComments.Focus();
        }

        private void cmbService_LostFocus(object sender, RoutedEventArgs e) {
            // Adds item in the service combobox if it does not already exists
            // this allows user to reuse the value in the next evaluation without Casely having to recheck the database
            if (cmbService.Text.Trim() != "" && !cmbService.Items.Contains(cmbService.Text)) {
                suggestionService.Add(cmbService.Text);
            }
        }

        private void cmbSelfEvaluation_LostFocus(object sender, RoutedEventArgs e) {
            // Adds item in the service combobox if it does not already exists
            // this allows user to reuse the value in the next evaluation without Casely having to recheck the database
            if (cmbSelfEvaluation.Text.Trim() != "" && !cmbSelfEvaluation.Items.Contains(cmbSelfEvaluation.Text)) {
                suggestionEvaluation.Add(cmbSelfEvaluation.Text);
            }
            suggestionEvaluation = new ObservableCollection<string>(suggestionEvaluation.OrderBy(x => x.ToString()));
            cmbSelfEvaluation.ItemsSource = suggestionEvaluation;
        }
    }

}
