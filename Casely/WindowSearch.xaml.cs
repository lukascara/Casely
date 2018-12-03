using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CaselyData;

namespace Casely {

    /// <summary>
    /// Interaction logic for WindowSearch.xaml
    /// </summary>
    public partial class WindowCaseSearch : Window {
        List<CaseEntry> listAllCaseEntries = new List<CaseEntry>();
        ObservableCollection<Staff> listStaff = new ObservableCollection<Staff>();
        ObservableCollection<CaseEntry> listFilteredCaseEntry = new ObservableCollection<CaseEntry>();


        public WindowCaseSearch() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            listStaff = new ObservableCollection<Staff>(SqliteDataAcces.GetListAuthor());
            cmbAuthor.ItemsSource = listStaff;
            cmbAuthor.SelectedValuePath = "AuthorID";

            lbFilteredCaseEntry.ItemsSource = listFilteredCaseEntry;
            lbFilteredCaseEntry.SelectedValuePath = "CaseNumber";
            listAllCaseEntries = SqliteDataAcces.GetListAllCaseEntries();
            SearchDatabase();

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            SearchDatabase();
        }

        /// <summary>
        /// Searches the database using the search terms and restrictions inputed by the user.
        /// </summary>
        private void SearchDatabase() {
            // create a copy of all the case entries which will be filtered in the following steps
            List<CaseEntry> listFilteredCE = new List<CaseEntry>(listAllCaseEntries);
            // Find cases that match the authorID
            if (cmbAuthor.SelectedIndex != -1) {
                var listFilter = SearchByAuthor(cmbAuthor.SelectedValue.ToString());
                listFilteredCE = listFilteredCE.Where(x => listFilter.ToList()
                                        .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();
            }
            if (txtFilterInterpretation.Text != "") {
                var listFilter = SearchFilterByInterpretation(txtFilterInterpretation.Text);
                listFilteredCE = listFilteredCE.Where(x => listFilter.ToList()
                                        .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();              
            }
            if (txtFilterResult.Text != "") {
                var listFilter = SearchFilterByResult(txtFilterResult.Text);
                listFilteredCE = listFilteredCE.Where(x => listFilter.ToList()
                                        .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();
            }
            if (txtFilterComment.Text != "") {
                var listFilter = SearchFilterByComment(txtFilterComment.Text);
                listFilteredCE = listFilteredCE.Where(x => listFilter.ToList()
                                        .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();
            }
            if (txtFilterTumorSynoptic.Text != "") {
                var listFilter = SearchFilterByTumorSynoptic(txtFilterTumorSynoptic.Text);
                listFilteredCE = listFilteredCE.Where(x => listFilter.ToList()
                                        .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();
            }
            listFilteredCaseEntry.Clear();
            foreach(var c in listFilteredCE) {
                listFilteredCaseEntry.Add(c);
            }
            if (listFilteredCaseEntry.Count != 0) {
                lbFilteredCaseEntry.SelectedIndex = 0;
            }
        }

        private List<CaseEntry> SearchByAuthor(string authorID) {
           return  SqliteDataAcces.GetCaseEntryFilterAuthorID(authorID);
        }

        private List<CaseEntry> SearchFilterByInterpretation(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryInterpretation(searchTerms);                           
        }

        private List<CaseEntry> SearchFilterByResult(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryResult(searchTerms);
        }

        private List<CaseEntry> SearchFilterByComment(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryComment(searchTerms);
        }

        private List<CaseEntry> SearchFilterByTumorSynoptic(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryTumorSynoptic(searchTerms);
        }

        private void lbFilteredCaseEntry_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (lbFilteredCaseEntry.SelectedIndex != -1) {
                var ce = (CaseEntry)lbFilteredCaseEntry.SelectedItem;
                wbReportViewer.Text = ce.toHtml();
            }
        }

        private void cmbAuthor_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            txtStatus.Text = "Searching...";
            SearchDatabase();
            txtStatus.Text = "Done";
        }


        private void txtFilter_LostFocus(object sender, RoutedEventArgs e) {
            SearchDatabase();
        }

        private void txtFilter_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SearchDatabase();
            }
        }

    }
}
