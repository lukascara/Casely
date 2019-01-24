using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CaselyData;

namespace Casely {
    /// <summary>
    /// Interaction logic for GrossingEditor.xaml
    /// </summary>
    public partial class WindowGrossingEditor : Window {

     
        List<string> suggestSpecimen;
        List<string> suggestedProcedure;
        private bool hasCaseNumberChanged = false;

        public WindowGrossingEditor(CaseEntry caseEntry) {
            refreshSuggestions();
            InitializeComponent();
            refreshParts();
        }

        public WindowGrossingEditor() {
            refreshSuggestions();
            InitializeComponent();
            refreshParts();
            txtCaseNumber.Text = SqliteDataAcces.CaseNumberPrefix;
        }

        public void refreshSuggestions() {
            suggestSpecimen = new List<string>(SqliteDataAcces.GetUniqueSpecimen());
            suggestedProcedure = new List<string>(SqliteDataAcces.GetUniqueProcedure());
        }


        private void refreshParts() {
            
            List<PartEntry> listPartEntry = SqliteDataAcces.GetListPartEntryLatestVersion(txtCaseNumber.Text);
            // clear old UC controls
            for (int i = 0; i < wpParts.Children.Count; i++) {
                if (wpParts.Children[i] is UCPartEntry) {
                    wpParts.Children.RemoveAt(i);
                    i--; // go back one since we removed a control
                }
            }
            // create new ones from the partEntry loaded from the database
            foreach (var p in listPartEntry) {
                wpParts.Children.Add(new UCPartEntry(p, suggestSpecimen, suggestedProcedure));
            }
        }


        private void addPart() {
            if (wpParts.Children.Count > 1) {
                UCPartEntry lastPart = wpParts.Children[wpParts.Children.Count-1] as UCPartEntry;
                char lstParLetter = lastPart.tbPart.Text.Length != 0 ? lastPart.tbPart.Text[0] : 'A';
                lstParLetter++;
                UCPartEntry newPart = new UCPartEntry(new PartEntry {
                    Part = (lstParLetter++).ToString(),
                    DateTimeModifiedObject = DateTime.Now,
                    Specimen = lastPart.tbSpecimen.Text,
                    Procedure = lastPart.tbProcedure.Text
                }, suggestSpecimen, suggestedProcedure);
                wpParts.Children.Add(newPart);
            } else {
                UCPartEntry newPart = new UCPartEntry(new PartEntry {
                    Part = "A",
                    DateTimeModifiedObject = DateTime.Now,
                    Specimen = "",
                    Procedure = ""
                }, suggestSpecimen,suggestedProcedure);
                wpParts.Children.Add(newPart);
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            addPart();

        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e) {
            if (txtCaseNumber.Text == "" || txtCaseNumber.Text == SqliteDataAcces.CaseNumberPrefix) {
                MessageBox.Show("Please enter a case number.");
            } else if (cmbStaff.Text == "") {
                MessageBox.Show("Please enter the name of the person grossing the specimen");
            } else if (cbService.Text == "") {
                MessageBox.Show("Please enter a service");
            } else {
                List<PartEntry> partsToAdd = new List<PartEntry>();
                // get the current date and time to save the same modified time for all parts being added to the database
                DateTime currentTime = DateTime.Now;
                foreach (var p in wpParts.Children) {
                    if (p is UCPartEntry) {
                        var pt = (UCPartEntry)p;
                        PartEntry newPart = new PartEntry() {
                            Part = pt.partEntry.Part,
                            Procedure = pt.partEntry.Procedure,
                            Specimen = pt.partEntry.Specimen,
                            AuthorID = cmbStaff.Text,
                            DateModifiedString = pt.dtTime.Value.GetValueOrDefault().ToString("yyyy-MM-dd"),
                            TimeModifiedString = pt.dtTime.Value.GetValueOrDefault().ToString("HH:mm:ss"),
                            CaseNumber = txtCaseNumber.Text
                        };
                        partsToAdd.Add(newPart);
                    }
                }
                SqliteDataAcces.InsertNewParts(partsToAdd, new CaselyUserData() { CaseNumber = txtCaseNumber.Text, Service = cbService.Text });
                txtCaseNumber.Text = SqliteDataAcces.CaseNumberPrefix;
                refreshParts();
                refreshSuggestions();
                MessageBox.Show("Case entry added to database");
            }           
        }

        private void wpParts_Loaded(object sender, RoutedEventArgs e) {
           
        }


        private void txtCaseNumber_LostFocus(object sender, RoutedEventArgs e) {
            if (hasCaseNumberChanged == true) {
                refreshParts();
            }
        }

        private void txtCaseNumber_TextChanged(object sender, TextChangedEventArgs e) {
            if (txtCaseNumber.Text != SqliteDataAcces.CaseNumberPrefix && txtCaseNumber.Text != "") {
                hasCaseNumberChanged = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}
