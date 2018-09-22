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
    public partial class GrossingEditor : Window {

        ObservableCollection<PartEntry> listCaseParts;
        CaseEntry caseEntry;
        List<string> suggestSpecimen;
        List<string> suggestedProcedure;

        public GrossingEditor(CaseEntry caseEntry) {
            listCaseParts = new ObservableCollection<PartEntry>(SqliteDataAcces.getParts("1"));
            this.caseEntry = caseEntry;
            loadSuggestions();
            InitializeComponent();
            loadParts();
        }

        public GrossingEditor() {
            listCaseParts = new ObservableCollection<PartEntry>(SqliteDataAcces.getParts("1"));
           
            this.caseEntry = new CaseEntry() {
                AuthorFullName = "Default" ,
                DateTimeObject = DateTime.Now,
                Id = 1

            };
            loadSuggestions();
            InitializeComponent();
            loadParts();
        }

        public void loadSuggestions() {
            suggestSpecimen = new List<string>(SqliteDataAcces.GetListSpecimen());
            suggestedProcedure = new List<string>(SqliteDataAcces.GetListProcedure());
        }


        public void loadParts() {
            foreach(var p in listCaseParts) {
                wpParts.Children.Add(new UCPartEntry(p, suggestSpecimen, suggestedProcedure));
            }
        }

       /* /// <summary>
        /// Gets the index after the last UCPart control.
        /// This allows us to place UCcontrols above the buttons for add, etc. visually.
        /// </summary>
        private int wpPartLastIndex() {
            int lastUCPartIndex = 0;
            for (int i = 0; i< wpParts.Children.Count; i++) {
                if (!(wpParts.Children[i] is UCPartEntry)) {
                    lastUCPartIndex = i - 1;
                    break;
                }
            }
            return lastUCPartIndex;
        }*/

        public void addPart() {
            if (wpParts.Children.Count > 1) {
                UCPartEntry lastPart = wpParts.Children[wpParts.Children.Count-1] as UCPartEntry;
                char lstParLetter = lastPart.tbPart.Text.Length != 0 ? lastPart.tbPart.Text[0] : 'A';
                lstParLetter++;
                UCPartEntry newPart = new UCPartEntry(new PartEntry {
                    Part = (lstParLetter++).ToString(),
                    AuthorFullName = lastPart.partEntry.AuthorFullName,
                    DateTimeObject = lastPart.dtTime.Value.Value,
                    Specimen = lastPart.tbSpecimen.Text,
                    Procedure = lastPart.tbProcedure.Text
                }, suggestSpecimen, suggestedProcedure);
                wpParts.Children.Add(newPart);
            } else {
                UCPartEntry newPart = new UCPartEntry(new PartEntry {
                    Part = "A",
                    AuthorFullName = caseEntry.AuthorFullName,
                    DateTimeObject = caseEntry.DateTimeObject,
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
            foreach(var p in wpParts.Children) {
                if(p is UCPartEntry) {
                    var pt = (UCPartEntry)p;
                    PartEntry newPart = new PartEntry() {
                        Part = pt.partEntry.Part,
                        Procedure = pt.partEntry.Procedure,
                        Specimen = pt.partEntry.Specimen,
                        AuthorFullName = cmbStaff.Text,
                        DateString = pt.partEntry.DateString,
                        TimeString = pt.partEntry.TimeString,
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
    }
}
