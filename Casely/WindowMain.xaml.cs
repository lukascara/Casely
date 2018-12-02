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
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

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
            connectToDB();
            WindowSelfEvaluation wn = new WindowSelfEvaluation();
            wn.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            connectToDB();
        }

        private void connectToDB() {
            if (!(File.Exists(SqliteDataAcces.DBPath))) {
                MessageBoxResult dialogResult = System.Windows.MessageBox.Show($"Casely database does not exist at {SqliteDataAcces.DBPath}. Should it be created?", "Create Database", MessageBoxButton.YesNo, MessageBoxImage.None);
                if (dialogResult == MessageBoxResult.Yes) {
                    SqliteDataAcces.CreateDatabase();
                    tbDBPath.Text = SqliteDataAcces.DBPath;
                } 
            }
            CheckDatabaseExists();
        }

        private void CheckDatabaseExists() {
            if (!File.Exists(SqliteDataAcces.DBPath)){
                tbDBPath.Text = "Casely database cannot be found! Create or open it in File->Create Database/Open Database.";
                LockButtons(false);
            } else {
                LockButtons(true);
                tbDBPath.Text = SqliteDataAcces.DBPath;
            }
        }

        private void LockButtons(bool ButtonsShouldBeActive) {
            btnSelfEvaluate.IsEnabled = ButtonsShouldBeActive;
            btnSearch.IsEnabled = ButtonsShouldBeActive;
            btnSignout.IsEnabled = ButtonsShouldBeActive;
            btnImportSoftPathData.IsEnabled = ButtonsShouldBeActive;
            MenuImportSoftPath.IsEnabled = ButtonsShouldBeActive;
        }

        private void MenuShowLicense_Click(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show(@"MIT License

Copyright (c) 2018 Lukas Cara

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the 'Software'), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

            The above copyright notice and this permission notice shall be included in all
            copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.");
        }

        private void btnImportData_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Excel File" ;
            theDialog.Filter = "Excel files (.xls)|*.xls";
            if (theDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    
                    var softText = File.ReadAllText(theDialog.FileName);
                    CaselyData.SoftToCaselyConverter sc = new SoftToCaselyConverter();
                    var importedData = sc.importSoftPathCSVData(theDialog.FileName);
                    
                    var pathCaseEntriesToAdd = new List<CaseEntry>();
                    int casesImportedCount = 0;
                    int casesAlreadImported = 0;
                    foreach(var d in importedData) {
                        // Import the cases only if there is not already a report version by both the attending and the resident.
                        if (!(SqliteDataAcces.HasMultipleAuthorEntries(d.CaseNumber))) {
                            pathCaseEntriesToAdd.Add(d);
                            casesImportedCount++;
                        } else {
                            casesAlreadImported++;
                        }
                    }
                    SqliteDataAcces.BatchInsertNewCaseEntry(pathCaseEntriesToAdd);
                   
                    
                    System.Windows.Forms.MessageBox.Show($"{casesImportedCount/2} cases imported. {casesAlreadImported} already existed in Casely.");
                    
                       
                } catch (Exception ex) {
                    System.Windows.MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void btnOpenDatabase_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = ".db";
            fileDialog.Filter = "Casely database files (.db)|*.db";
            Nullable<bool> result = fileDialog.ShowDialog();
            if (result == true) {
                //var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                //var path = Properties.Settings.Default.DatabasePath;
                //var dbPath = Path.Combine(path, "Casely.db");
                tbDBPath.Text = fileDialog.FileName;
                CaselyData.SqliteDataAcces.DBPath = fileDialog.FileName;
                connectToDB();
            }
        }

        private void btnCreateDatabase_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.DefaultExt = ".db";
            fileDialog.Filter = "Casely database files (.db)|*.db";
            Nullable<bool> result = fileDialog.ShowDialog();
            if (result == true) {
                //var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                //var path = Properties.Settings.Default.DatabasePath;
                //var dbPath = Path.Combine(path, "Casely.db");
                if (File.Exists(fileDialog.FileName)) {
                    File.Delete(fileDialog.FileName);
                }
                tbDBPath.Text = fileDialog.FileName;
                CaselyData.SqliteDataAcces.DBPath = fileDialog.FileName;
                connectToDB();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            WindowCaseSearch wn = new WindowCaseSearch();
            wn.ShowDialog();
        }
    }
}
