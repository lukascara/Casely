using System;
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
        List<CaselyUserData> listAllCaselyUserDatas = new List<CaselyUserData>();


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
            listAllCaselyUserDatas = SqliteDataAcces.GetAllCaselyUserData();
            SearchDatabase();

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            SearchDatabase();
        }

        /// <summary>
        /// Searches the database using the search terms and restrictions inputed by the user.
        /// </summary>
        private void SearchDatabase() {
            SanitizeSearchBoxes();
            try {
                // create a copy of all the case entries which will be filtered in the following steps
                List<CaseEntry> listFilteredCE = new List<CaseEntry>(listAllCaseEntries);
                // Apply the filters based on the values in the gui
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
                    var listFilter = SearchFilterByResult(txtFilterResult.Text);  // Search for cases that match the filter query supplied
                                                                                  // Filter out all the cases by case numbers that are not returned in the listFilter (which is returned from the database)
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
                if (txtFilterEvaluationComment.Text != "") {
                    List<CaselyUserData> listFilter = SearchFilterByEvaluationComment(txtFilterEvaluationComment.Text);
                    listFilteredCE = listFilteredCE.Where(x => listFilter.ToList()
                                            .FindIndex(c => c.CaseNumber == x.CaseNumber) != -1).ToList();
                }
                listFilteredCaseEntry.Clear();
                foreach (var c in listFilteredCE) {
                    listFilteredCaseEntry.Add(c);
                }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("Error searching the database. Please check your search terms and syntax. Certain case-sensitive words are reserved (NOT, OR, AND, i.e.) and can cause and error if used incorrectly. " +
                    " Original error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Removes certain characters from the search terms, i.e. ', in order to prevent a SQL syntax error for search terms.
        /// </summary>
        private void SanitizeSearchBoxes() {
            txtFilterInterpretation.Text = txtFilterInterpretation.Text.Replace("\"", "").Replace("'", "");
            txtFilterResult.Text = txtFilterResult.Text.Replace("\"", "").Replace("'", "");
            txtFilterComment.Text = txtFilterComment.Text.Replace("\"", "").Replace("'", "");
            txtFilterTumorSynoptic.Text = txtFilterTumorSynoptic.Text.Replace("\"", "").Replace("'", "");
        }

        private List<CaseEntry> SearchByAuthor(string authorID) {
            return SqliteDataAcces.GetCaseEntryFilterAuthorID(authorID);
        }

        private List<CaseEntry> SearchFilterByInterpretation(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryInterpretation(searchTerms, Properties.Settings.Default.UserID);
        }

        private List<CaseEntry> SearchFilterByResult(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryResult(searchTerms, Properties.Settings.Default.UserID);
        }

        private List<CaseEntry> SearchFilterByComment(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryComment(searchTerms, Properties.Settings.Default.UserID);
        }

        private List<CaseEntry> SearchFilterByTumorSynoptic(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryTumorSynoptic(searchTerms, Properties.Settings.Default.UserID);
        }

        private List<CaselyUserData> SearchFilterByEvaluationComment(string searchTerms) {
            return SqliteDataAcces.FilterCaseEntryEvaluationComment(searchTerms);
        }

        private void lbFilteredCaseEntry_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (lbFilteredCaseEntry.SelectedIndex != -1) {
                var ce = (CaseEntry)lbFilteredCaseEntry.SelectedItem;
                var pc = listAllCaselyUserDatas.Where(x => x.CaseNumber == ce.CaseNumber).First();
                wbReportViewer.Text = $"<b>Self-Evluation: </b>{pc.Evaluation}<b><br>Self-Evaluation Comments: </b>{pc.EvaluationComment}";
                wbReportViewer.Text +=  ce.toHtml();
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
