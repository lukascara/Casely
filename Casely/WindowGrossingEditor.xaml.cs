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
        public GrossingEditor() {
            listCaseParts = new ObservableCollection<PartEntry>(SqliteDataAcces.getParts("1"));

            InitializeComponent();
            loadParts();
        }


        public void loadParts() {
            foreach(var p in listCaseParts) {
                int insertIndex = wpPartLastIndex();
                wpParts.Children.Insert(insertIndex,new UCPartEntry(p));
            }
        }

        /// <summary>
        /// Gets the index after the last UCPart control.
        /// This allows us to place UCcontrols above the buttons for add, etc. visually.
        /// </summary>
        private int wpPartLastIndex() {
            int lastUCPartIndex = 0;
            for (int i = 0; i< wpParts.Children.Count; i++) {
                if (!(wpParts.Children[i] is UCPartEntry)) {
                    lastUCPartIndex = i > 1 ? wpParts.Children.Count - 2 : 0;
                }
            }
            return lastUCPartIndex;
        }

        public void addPart() {
            UCPartEntry lastPart = wpParts.Children[-1] as UCPartEntry;
            char lstParLetter = lastPart.tbPart.Text.Length != 0 ? lastPart.tbPart.Text[0] : 'A';
            lstParLetter++;
            UCPartEntry newPart = new UCPartEntry(new PartEntry {
                Part = (lstParLetter++).ToString(),
                Author = lastPart.partEntry.Author,
                DateTimeObject = lastPart.dtTime.Value.Value,
                Specimen = lastPart.tbSpecimen.Text,
                Procedure = lastPart.tbProcedure.Text
            });
            wpParts.Children.Insert(wpPartLastIndex(), newPart);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            PartEntry p2 = new PartEntry();
            var a2 = new Staff();
            a2.FullName = "Lukas Cara";
            p2.Author = a2;
            p2.DateString = "11/09/2018";
            p2.TimeString = "2:00PM";
            p2.Specimen = "Ovary222";
            p2.Procedure = "Salpingectomy";
            p2.Part = "C";
            listCaseParts.Add(p2);
            wpParts.Children.Insert(3,new UCPartEntry(p2));

        }
    }
}
