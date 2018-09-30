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

namespace Casely
{
    /// <summary>
    /// Interaction logic for WindowDiagnosis.xaml
    /// </summary>
    public partial class WindowDiagnosis : Window
    {
        public WindowDiagnosis()
        {
            InitializeComponent();
            RefreshPartDiagnosis();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            RefreshCasesWithoutDiagnosis();
        }

        private void cmbCaseNumber_LostFocus(object sender, RoutedEventArgs e) {
            RefreshPartDiagnosis();
        }

        private void RefreshCasesWithoutDiagnosis() {
            DateTime startDate = DateTime.Now.AddDays(-Double.Parse(txtDaysToLoad.Text));
            DateTime endDate = DateTime.Now;
            foreach (var s in SqliteDataAcces.GetListCaseNumbersPastDays(startDate)) {
                if (!(SqliteDataAcces.EntryExistsPartDiagnosis(s))) {
                    cmbCaseNumber.Items.Add(s);
                }
            }
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
                // create new ones from the partEntry loaded from the database
                foreach (var p in listPartDiagnosis) {
                    spParts.Children.Add(new UCdiagnosis(p));
                }

                CaseEntry ce = SqliteDataAcces.GetCaseEntryLatestVersion(cmbCaseText);
                // Display the report data in the rich textbox
                rtxCaseReport.Document.Blocks.Clear();
                rtxCaseReport.Document.Blocks.Add(new Paragraph(new Run($"Author: {ce.AuthorFullName}\nDate Modified:${ce.DateModifiedString}")));
                rtxCaseReport.Document.Blocks.Add(new Paragraph(new Run($"INTERPRETATION:\n {ce.Interpretation}")));
                rtxCaseReport.Document.Blocks.Add(new Paragraph(new Run($"MICROSCOPIC:\n {ce.Microscopic}")));
                rtxCaseReport.Document.Blocks.Add(new Paragraph(new Run($"TUMOR SYNOPTIC:\n {ce.TumorSynoptic}")));
                rtxCaseReport.Document.Blocks.Add(new Paragraph(new Run($"COMMENT:\n {ce.Comment}")));
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
                });
                spParts.Children.Add(newPart);
            } else {
                UCdiagnosis newPart = new UCdiagnosis(new PartDiagnosis {
                    Part = "A",
                    Organ = "",
                    OrganSystem = "",
                    Diagnosis = "",
                    DiagnosisDetailed = "",
                });
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
    }
}
