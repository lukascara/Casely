using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CaselyData;


namespace Casely {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowReportEditor : Window {
        public WindowReportEditor() {
            InitializeComponent();
            txtCaseNumber.Text = SqliteDataAcces.CaseNumberPrefix;

        }

        private void btnAddCase_Click(object sender, RoutedEventArgs e) {
            if (txtCaseNumber.Text.Trim() == "" || txtCaseNumber.Text == SqliteDataAcces.CaseNumberPrefix){
                MessageBox.Show("Please enter a case number");
            } else if (cmbService.Text == "") {
                MessageBox.Show("Please choose a service");
            } else if (cmbAuthor.Text == "") {
                MessageBox.Show("Please choose an author");
            } else {
                PathCase pc = new PathCase();
                pc.CaseNumber = txtCaseNumber.Text;
                pc.Service = cmbService.Text;
                CaseEntry ce = new CaseEntry();
                DateTime currentTime = DateTime.Now;
                ce.AuthorFullName = cmbAuthor.Text;
                ce.CaseNumber = txtCaseNumber.Text;
                ce.Interpretation = txtInterpretation.Text;
                ce.Result = txtResultEntry.Text;
                ce.Comment = txtComment.Text;
                ce.TumorSynoptic = txtTumorSynoptic.Text;
                ce.DateCreatedString = dtCreated.Value.GetValueOrDefault().ToString("yyyy-MM-dd");
                ce.TimeCreatedString = dtCreated.Value.GetValueOrDefault().ToString("HH:mm:ss");
                ce.DateModifiedString = currentTime.ToString("yyyy-MM-dd");
                ce.TimeModifiedString = currentTime.ToString("HH:mm:ss");
                SqliteDataAcces.ParseInsertCaseEntry(ce, pc);
                txtInterpretation.Text = "";
                txtComment.Text = "";
                txtResultEntry.Text = "";
                txtTumorSynoptic.Text = "";
                txtCaseNumber.Text = SqliteDataAcces.CaseNumberPrefix;
                MessageBox.Show("Case Added");
                cmbAuthor.ItemsSource = SqliteDataAcces.GetListStaffFullNames();
                dtCreated.Value = DateTime.Now;

            }
        }

        /// <summary>
        /// Parses the SoftPath result textfield and outputs the parts
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<PartEntry> parseResult(string result) {
            List<PartEntry> parts = new List<PartEntry>();
            return parts;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            cmbAuthor.ItemsSource = SqliteDataAcces.GetListStaffFullNames();
            dtCreated.Value = DateTime.Now;
        }
    }
   
}
