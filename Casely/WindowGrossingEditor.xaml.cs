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
using System.Collections.ObjectModel;

namespace Casely {
    /// <summary>
    /// Interaction logic for GrossingEditor.xaml
    /// </summary>
    public partial class WindowGrossingEditor : Window {

     
        List<string> suggestSpecimen;
        List<string> suggestedProcedure;
        private bool hasCaseNumberChanged = false;

        public WindowGrossingEditor(CaseEntry caseEntry) {
            loadSuggestions();
            InitializeComponent();
            loadParts();
        }

        public WindowGrossingEditor() {
           
            
            loadSuggestions();
            InitializeComponent();
            loadParts();
        }

        public void loadSuggestions() {
            suggestSpecimen = new List<string>(SqliteDataAcces.GetListSpecimen());
            suggestedProcedure = new List<string>(SqliteDataAcces.GetListProcedure());
        }


        public void loadParts() {
            
            List<PartEntry> listPartEntry = SqliteDataAcces.GetListPartEntryLatestVersion(txtCaseNumber.Text);
            // clear old UC controls
            foreach (var p in wpParts.Children) {
               if (p is UCPartEntry) {
                    wpParts.Children.Remove((UCPartEntry)p);
                }
            }
            // create new ones from the partEntry loaded from the database
            foreach (var p in listPartEntry) {
                wpParts.Children.Add(new UCPartEntry(p, suggestSpecimen, suggestedProcedure));
            }
        }


        public void addPart() {
            if (wpParts.Children.Count > 1) {
                UCPartEntry lastPart = wpParts.Children[wpParts.Children.Count-1] as UCPartEntry;
                char lstParLetter = lastPart.tbPart.Text.Length != 0 ? lastPart.tbPart.Text[0] : 'A';
                lstParLetter++;
                UCPartEntry newPart = new UCPartEntry(new PartEntry {
                    Part = (lstParLetter++).ToString(),
                    DateTimeCreatedObject = DateTime.Now,
                    Specimen = lastPart.tbSpecimen.Text,
                    Procedure = lastPart.tbProcedure.Text
                }, suggestSpecimen, suggestedProcedure);
                wpParts.Children.Add(newPart);
            } else {
                UCPartEntry newPart = new UCPartEntry(new PartEntry {
                    Part = "A",
                    DateTimeCreatedObject = DateTime.Now,
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
            List<PartEntry> partsToAdd = new List<PartEntry>();
            // get the current date and time to save the same modified time for all parts being added to the database
            DateTime currentTime = DateTime.Now;
            foreach (var p in wpParts.Children) {
                if(p is UCPartEntry) {
                    var pt = (UCPartEntry)p;
                    PartEntry newPart = new PartEntry() {
                        Part = pt.partEntry.Part,
                        Procedure = pt.partEntry.Procedure,
                        Specimen = pt.partEntry.Specimen,
                        AuthorFullName = cmbStaff.Text,
                        DateCreatedString = pt.dtTime.Value.GetValueOrDefault().ToShortDateString(),
                        TimeCreatedString = pt.dtTime.Value.GetValueOrDefault().ToShortTimeString(),
                        DateModifiedString = currentTime.ToShortDateString(),
                        TimeModifiedString = currentTime.ToShortTimeString(),
                        CaseNumber = txtCaseNumber.Text
                    };
                    partsToAdd.Add(newPart);
                }
            }
            SqliteDataAcces.InsertNewParts(partsToAdd, new PathCase() { CaseNumber = txtCaseNumber.Text, Service = cbService.Text });
            this.Close();
        }

        private void wpParts_Loaded(object sender, RoutedEventArgs e) {
            cmbStaff.ItemsSource = SqliteDataAcces.GetListStaffFullNames();
        }


        private void txtCaseNumber_LostFocus(object sender, RoutedEventArgs e) {
            if (hasCaseNumberChanged == true) {
                loadParts();
            }
        }

        private void txtCaseNumber_TextChanged(object sender, TextChangedEventArgs e) {
            hasCaseNumberChanged = true;
        }
    }
}
