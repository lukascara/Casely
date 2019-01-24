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

        private bool hasCaseNumberChanged = false;

        public WindowReportEditor() {
            InitializeComponent();
            txtCaseNumber.Text = SqliteDataAcces.CaseNumberPrefix;
            foreach (var a in SqliteDataAcces.GetListAuthor()) {
                cmbAuthor.Items.Add(a.AuthorID);
            }
            foreach (var a in SqliteDataAcces.GetUniqueService()) {
                cmbService.Items.Add(a);
            }

        }

        private void btnAddCase_Click(object sender, RoutedEventArgs e) {
            if (txtCaseNumber.Text.Trim() == "" || txtCaseNumber.Text == SqliteDataAcces.CaseNumberPrefix){
                MessageBox.Show("Please enter a case number");
            } else if (cmbService.Text == "") {
                MessageBox.Show("Please choose a service");
            } else if (cmbAuthor.Text == "") {
                MessageBox.Show("Please choose an author");
            } else {
                CaselyUserData pc = new CaselyUserData();
                pc.CaseNumber = txtCaseNumber.Text;
                pc.Service = cmbService.Text;
                CaseEntry ce = new CaseEntry();
                DateTime currentTime = DateTime.Now;
                ce.AuthorID = cmbAuthor.Text;
                ce.CaseNumber = txtCaseNumber.Text;
                ce.Interpretation = txtInterpretation.Text;
                ce.Result = txtResultEntry.Text;
                ce.Comment = txtComment.Text;
                ce.TumorSynoptic = txtTumorSynoptic.Text;
                ce.DateModifiedString = dtCreated.Value.GetValueOrDefault().ToString("yyyy-MM-dd");
                ce.TimeModifiedString = dtCreated.Value.GetValueOrDefault().ToString("HH:mm:ss");
                SqliteDataAcces.InsertNewCaseEntry(ce, pc);
                txtInterpretation.Text = "";
                txtComment.Text = "";
                txtResultEntry.Text = "";
                txtTumorSynoptic.Text = "";
                txtCaseNumber.Text = SqliteDataAcces.CaseNumberPrefix;
                MessageBox.Show("Case Added");
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
            dtCreated.Value = DateTime.Now;
        }

        private void RefreshCaseEntry() {
            if (txtCaseNumber.Text != SqliteDataAcces.CaseNumberPrefix && txtCaseNumber.Text != "") {
                CaseEntry ce = SqliteDataAcces.GetCaseEntryLatestVersion(txtCaseNumber.Text);
                if (ce != null) {
                    txtInterpretation.Text = ce.Interpretation;
                    txtResultEntry.Text = ce.Result;
                    txtComment.Text = ce.Comment;
                    txtTumorSynoptic.Text = ce.TumorSynoptic;
                    message.Text = $"Case entry found by: {ce.AuthorID} ({ce.DateModifiedString} {ce.TimeModifiedString})";
                }
                hasCaseNumberChanged = false;
            } else {
                message.Text = $"No case entry found";
            }     
        }

        private void txtCaseNumber_LostFocus(object sender, RoutedEventArgs e) {
            if (hasCaseNumberChanged == true) {
                RefreshCaseEntry();
            }
        }

        private void txtCaseNumber_TextChanged(object sender, TextChangedEventArgs e) {
            if (txtCaseNumber.Text != SqliteDataAcces.CaseNumberPrefix && txtCaseNumber.Text != "") {
                hasCaseNumberChanged = true;
            }
        }
    }
   
}
