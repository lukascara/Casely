using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CaselyData;
using DiffMatchPatch;

namespace Casely {
    /// <summary>
    /// Interaction logic for WindowDiagnosis.xaml
    /// </summary>
    public partial class WindowDiagnosis : Window {

        private List<string> suggestionOrgan = new List<string>();
        private List<string> suggestionOrganSystem = new List<string>();
        private List<string> suggestionDiagnosis = new List<string>();
        private List<string> suggestionCategory = new List<string>();

        public WindowDiagnosis() {
            InitializeComponent();
            RefreshPartDiagnosis();
            refreshSuggestions();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            RefreshCasesDiagnosis();
        }

        private void cmbCaseNumber_LostFocus(object sender, RoutedEventArgs e) {
            RefreshPartDiagnosis();
        }

        private void RefreshCasesDiagnosis() {
            DateTime startDate = DateTime.Now.AddDays(-Double.Parse(txtDaysToLoad.Text));
            DateTime endDate = DateTime.Now;
            cmbCaseNumber.Items.Clear();
            foreach (var s in SqliteDataAcces.GetListCaseNumbersPastDays(startDate)) {
                if (chkFilterCompleted.IsChecked == false || !(SqliteDataAcces.EntryExistsPartDiagnosis(s))) {
                    cmbCaseNumber.Items.Add(s);
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
            suggestionCategory = new List<string>(SqliteDataAcces.GetListCategory());
            foreach (string st in standardSuggestCategory) {
                if (suggestionCategory.IndexOf(st) == -1) {
                    suggestionCategory.Add(st);
                }
            }

            suggestionDiagnosis = new List<string>(SqliteDataAcces.GetListDiagnosis());
            suggestionOrgan = new List<string>(SqliteDataAcces.GetListOrgan());
        }


        /// <summary>
        /// Loads the diagnosis for the selected case in to the UI.
        /// </summary>
        private void RefreshPartDiagnosis() {
            List<PartDiagnosis> listPartDiagnosis = SqliteDataAcces.GetListPartDiagnosisLatestVersion(cmbCaseNumber.Text);
            // clear old UC controls
            for (int i = 0; i < spParts.Children.Count; i++) {
                if (spParts.Children[i] is UCdiagnosis) {
                    spParts.Children.RemoveAt(i);
                    i--; // go back one since we removed a control
                }
            }
            // Check if a valid case number is selected
            string cmbCaseText = cmbCaseNumber.Text;
            if (cmbCaseText != "" && cmbCaseText != SqliteDataAcces.DbConnectionString) {
                btnAddDiagnosis.IsEnabled = true;
                // create new ones from the partDiagnosis loaded from the database
                foreach (var p in listPartDiagnosis) {
                    spParts.Children.Add(new UCdiagnosis(p, suggestionOrgan, suggestionOrganSystem, suggestionCategory, suggestionDiagnosis));
                }

                refreshComparison();
            } else {
                // blank case number entered, prevent addition of diagnosis
                btnAddDiagnosis.IsEnabled = false;
            }
        }

        private void AddPartDiagnosis() {
            if (spParts.Children.Count > 1) {
                UCdiagnosis lastPart = spParts.Children[spParts.Children.Count - 1] as UCdiagnosis;
                char lstParLetter = lastPart.tbPart.Text.Length != 0 ? lastPart.tbPart.Text[0] : 'A';
                UCdiagnosis newPart = new UCdiagnosis(new PartDiagnosis {
                    // keep the same part letter as preveious entry to allow multiple diagnosis for the same part
                    Part = (lstParLetter).ToString(),
                    Organ = lastPart.cmbOrgan.Text,
                    OrganSystem = lastPart.cmbOrgan.Text
                }, suggestionOrgan,suggestionOrganSystem,suggestionCategory,suggestionDiagnosis);
                spParts.Children.Add(newPart);
            } else {
                UCdiagnosis newPart = new UCdiagnosis(new PartDiagnosis {
                    Part = "A",
                    Organ = "",
                    OrganSystem = "",
                    Diagnosis = "",
                    DiagnosisDetailed = "",
                }, suggestionOrgan, suggestionOrganSystem, suggestionCategory, suggestionDiagnosis);
                spParts.Children.Add(newPart);
            }

        }

        private void btnAddDiagnosis_Click(object sender, RoutedEventArgs e) {
            AddPartDiagnosis();
        }

        private void btnSubmitDiagnosis_Click(object sender, RoutedEventArgs e) {
            List<PartDiagnosis> partsToAdd = new List<PartDiagnosis>();
            // get the current date and time to save the same modified time for all parts being added to the database
            DateTime currentTime = DateTime.Now;
            foreach (var p in spParts.Children) {
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
            SqliteDataAcces.InsertNewPartDiagnosisEntry(partsToAdd, new PathCase() { CaseNumber = cmbCaseNumber.Text });
            cmbCaseNumber.Text = SqliteDataAcces.CaseNumberPrefix;
        }

        private void cmbCaseNumber_DropDownClosed(object sender, EventArgs e) {
            RefreshPartDiagnosis();
        }

        private void chkFilterCompleted_Click(object sender, RoutedEventArgs e) {
            RefreshCasesDiagnosis();
        }

        private void refreshComparison() {
            if (cmbCaseNumber.Text != "" && cmbCaseNumber.Text != SqliteDataAcces.CaseNumberPrefix) {
                var listCase = SqliteDataAcces.getListCaseEntry(cmbCaseNumber.Text);
                /*foreach (var lc in listCase) {
                    ComboBoxItem cbitem = new ComboBoxItem();
                    cbitem.Content = lc.PrettyVersion();
                    cmbVersion.Items.Add(cbitem);
                }*/
                // gets the case entrys, groups them by author and then selects the last two author entries to compare.
                var listCaseToCompare = listCase.OrderByDescending(x => x.DateTimeModifiedObject).GroupBy(t => t.AuthorFullName).Select(x => x.FirstOrDefault()).ToList();


                string version0 = "";
                string version1 = "";
                wbDiffText.Text = "";
                if (cbInterpretation.IsChecked == true) {
                    version1 += $"-------Interpretation------------------------------------\n{listCaseToCompare[0].Interpretation}\n";
                    version0 += listCaseToCompare.Count == 2 ? $"-------Interpretation------------------------------------\n{listCaseToCompare[1].Interpretation}\n" : "";
                }
                if (cbResult.IsChecked == true) {
                    version1 += $"-------Result-------------------------------------------\n{listCaseToCompare[0].Result}\n";
                    version0 += listCaseToCompare.Count == 2 ? $"-------Result-------------------------------------------\n{listCaseToCompare[1].Result}\n" : "";
                }
                if (cbTumorSynoptic.IsChecked == true) {
                    version1 += $"-------Tumor Synoptic------------------------------------\n{listCaseToCompare[0].TumorSynoptic}\n";
                    version0 += listCaseToCompare.Count == 2 ? $"-------Tumor Synoptic------------------------------------\n{listCaseToCompare[1].TumorSynoptic}\n" : "";
                }
                if (cbComment.IsChecked == true) {
                    version1 += $"-------Comment-------------------------------------------\n{listCaseToCompare[0].Comment}\n";
                    version0 += listCaseToCompare.Count == 2 ? $"-------Comment-------------------------------------------\n{listCaseToCompare[1].Comment}\n" : "";
                }
                if (listCaseToCompare.Count < 2) {
                    wbDiffText.Text += "<h3>Need at least a report from two different authors to compare</h3>";
                    wbDiffText.Text += "\n" + version1.Replace("\n","<BR>");
                } else {
                    var dmp = DiffMatchPatchModule.Default;
                    var diffs = dmp.DiffMain(version0, version1);
                    dmp.DiffCleanupSemantic(diffs);
                    var html = dmp.DiffPrettyHtml(diffs).Replace("&para;", "");
                    wbDiffText.Text += html;
                }

            }
        }

        private void cbInterpretation_Click(object sender, RoutedEventArgs e) {
            refreshComparison();
        }

        private void cbResult_Click(object sender, RoutedEventArgs e) {
            refreshComparison();
        }

        private void cbTumorSynoptic_Checked(object sender, RoutedEventArgs e) {
            refreshComparison();
        }

        private void cbComment_Checked(object sender, RoutedEventArgs e) {
            refreshComparison();
        }
    }
}
