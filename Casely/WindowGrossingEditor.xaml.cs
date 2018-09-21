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
        public GrossingEditor(CaseEntry caseEntry) {
            listCaseParts = new ObservableCollection<PartEntry>(SqliteDataAcces.getParts("1"));
            this.caseEntry = caseEntry;
            
            InitializeComponent();
            loadParts();
        }

        public GrossingEditor() {
            listCaseParts = new ObservableCollection<PartEntry>(SqliteDataAcces.getParts("1"));
            
            this.caseEntry = new CaseEntry() {
                Author = new Staff() { FullName = "Default" },
                DateTimeObject = DateTime.Now,
                Id = 1

            };

            InitializeComponent();
            loadParts();
        }


        public void loadParts() {
            foreach(var p in listCaseParts) {
                wpParts.Children.Add(new UCPartEntry(p));
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
                    Author = lastPart.partEntry.Author,
                    DateTimeObject = lastPart.dtTime.Value.Value,
                    Specimen = lastPart.tbSpecimen.Text,
                    Procedure = lastPart.tbProcedure.Text
                });
                wpParts.Children.Add(newPart);
            } else {
                UCPartEntry newPart = new UCPartEntry(new PartEntry {
                    Part = "A",
                    Author = caseEntry.Author,
                    DateTimeObject = caseEntry.DateTimeObject,
                    Specimen = "",
                    Procedure = ""
                });
                wpParts.Children.Add(newPart);
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            addPart();

        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e) {

        }
    }
}
