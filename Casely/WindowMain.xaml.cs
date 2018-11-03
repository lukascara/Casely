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
using System.Data.SQLite;
using CaselyData;
using System.IO;
using System.Windows.Forms;

namespace Casely {
    /// <summary>
    /// Interaction logic for WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window {
        public WindowMain() {
            InitializeComponent();
        }

        private void btnGrossing_Click(object sender, RoutedEventArgs e) {
            WindowGrossingEditor ge = new WindowGrossingEditor();
            ge.ShowDialog();

        }

        private void btnSignout_Click(object sender, RoutedEventArgs e) {
            WindowReportEditor wn = new WindowReportEditor();
            wn.ShowInTaskbar = false;
            wn.ShowDialog();
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e) {
            WindowCompareReports wn = new WindowCompareReports();
            wn.ShowDialog();
        }

        private void btnDiagnosis_Click(object sender, RoutedEventArgs e) {
            WindowDiagnosis wn = new WindowDiagnosis();
            wn.ShowDialog();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (!(File.Exists(SqliteDataAcces.DBPath))) {
                MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Casely database does not exist and will now be created.", "Create Database");
                if (dialogResult == MessageBoxResult.OK) {
                    SqliteDataAcces.CreateDatabase();
                }
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Tab Separated File";
            if (theDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    var softText = File.ReadAllText(theDialog.FileName);
                    CaselyData.SoftToCaselyConverter sc = new SoftToCaselyConverter();
                    var importedData = sc.importSoftPathCSVData(theDialog.FileName);
                    foreach(var d in importedData) {
                        var pc = new PathCase() { CaseNumber = d.CaseNumber };
                        // Import the cases only if there is not already a report version by both the attending and the resident.
                        if (!(SqliteDataAcces.HasMultipleAuthorEntries(d.CaseNumber))) {
                            SqliteDataAcces.InsertNewPathCase(pc);
                            SqliteDataAcces.InsertNewCaseEntry(d, pc);
                        }
                    }
                    System.Windows.Forms.MessageBox.Show("Data imported!");

                       
                } catch (Exception ex) {
                    System.Windows.MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
    }
}
