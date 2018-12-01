using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Casely {

    /// <summary>
    /// Interaction logic for WindowSearch.xaml
    /// </summary>
    public partial class WindowCaseSearch : Window {
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

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            SearchDatabase();
        }

        private void SearchDatabase() {
            List<CaseEntry> listCE = new List<CaseEntry>();
            if (cmbAuthor.SelectedIndex != -1) {
                listCE = SearchByAuthor(cmbAuthor.SelectedValue.ToString());
            }
            listFilteredCaseEntry.Clear();
            foreach(var c in listCE) {
                listFilteredCaseEntry.Add(c);
            }
        }

        private List<CaseEntry> SearchByAuthor(string authorID) {
           return  SqliteDataAcces.GetCaseEntryFilterAuthorID(authorID);
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
        }
    }
}
