using System;
using System.Collections.Generic;
using System.Windows;
using CaselyData;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Reflection;

namespace Casely {
    /// <summary>
    /// Interaction logic for WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window {
        public WindowMain() {
            InitializeComponent();
        }

     

        private void btnSignout_Click(object sender, RoutedEventArgs e) {
            WindowReportEditor wn = new WindowReportEditor();
            wn.ShowInTaskbar = false;
            wn.ShowDialog();
        }



        private void btnDiagnosis_Click(object sender, RoutedEventArgs e) {
            ConnectOrUpdateDB();
            WindowSelfEvaluation wn = new WindowSelfEvaluation();
            wn.ShowDialog();
        }

        private void ConnectOrUpdateDB() {
            var isValid = SqliteDataAcces.CreateOrUpdateDatabase();
            tbDBPath.Text = SqliteDataAcces.DBPath;
            if (!(isValid)) {
                ActivateButtons(false);
            } else {
                ActivateButtons(true);
                tbDBPath.Text = SqliteDataAcces.DBPath;
            }
        } 

        private void ActivateButtons(bool ButtonsShouldBeActive) {
            btnSelfEvaluate.IsEnabled = ButtonsShouldBeActive;
            btnSearch.IsEnabled = ButtonsShouldBeActive;
            btnAddSignoutCase.IsEnabled = ButtonsShouldBeActive;
            btnImportSoftPathData.IsEnabled = ButtonsShouldBeActive;
            MenuImportSoftPath.IsEnabled = ButtonsShouldBeActive;
        }

        private void MenuShowLicense_Click(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show(@"
Source code available at: https://github.com/lukascara/Casely
MIT License

Copyright (c) 2018-2019 Lukas Cara

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
            ChangeStatusText("Importing data. Please wait...");
            if (theDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    var softText = File.ReadAllText(theDialog.FileName);
                    CaselyData.SoftPathExcelConvert sc = new SoftPathExcelConvert();
                    var importedData = sc.importSoftPathCSVData(theDialog.FileName);
                    
                    var caselyUserDataEntriesToAdd = new List<CaseEntry>();
                    int casesImportedCount = 0;
                    int casesAlreadImported = 0;
                    foreach(var d in importedData) {
                        // Import the cases only if there is not already a report version by both the attending and the resident.
                        if (!(SqliteDataAcces.HasMultipleAuthorEntries(d.CaseNumber))) {
                            caselyUserDataEntriesToAdd.Add(d);
                            casesImportedCount++;
                        } else {
                            casesAlreadImported++;
                        }
                    }
                    SqliteDataAcces.BatchInsertNewCaseEntry(caselyUserDataEntriesToAdd);
                   
                    
                    System.Windows.Forms.MessageBox.Show($"{casesImportedCount/2} cases imported. {casesAlreadImported} already existed in Casely.");
                    

                } catch (Exception ex) {
                    System.Windows.MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
                ChangeStatusText("");
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
                ConnectOrUpdateDB();
            }
        }

        private void btnCreateDatabase_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.DefaultExt = ".db";
            fileDialog.Filter = "Casely database files (.db)|*.db";
            Nullable<bool> result = fileDialog.ShowDialog();
            if (result == true) {
                ChangeStatusText("Creating database");
                //var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                //var path = Properties.Settings.Default.DatabasePath;
                //var dbPath = Path.Combine(path, "Casely.db");
                try {
                    if (File.Exists(fileDialog.FileName)) {
                        File.Delete(fileDialog.FileName);
                    }
                    tbDBPath.Text = fileDialog.FileName;
                    CaselyData.SqliteDataAcces.DBPath = fileDialog.FileName;
                    ConnectOrUpdateDB();
                    ChangeStatusText("Finished");

                } catch (Exception err) {
                    System.Windows.MessageBox.Show($"Error creating database. Is it open in another program?\nOrignal Error: {err.Message}","",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
        }

        private void ChangeStatusText(string status) {
            txtStatus.Text = status;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            WindowCaseSearch wn = new WindowCaseSearch();
            wn.ShowDialog();
        }

        private void ChangeUserID() {
            var oldUID = Properties.Settings.Default.UserID;
            Properties.Settings.Default.UserID = txtUserID.Text;
            Properties.Settings.Default.Save();
            txtStatus.Text = $"User ID changed from '{oldUID}' to '{txtUserID.Text}'";
        }

        private void txtUserID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                ChangeUserID();
            }
        }

        private void txtUserID_LostFocus(object sender, RoutedEventArgs e) {
            ChangeUserID();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            txtUserID.Text = Properties.Settings.Default.UserID;
            
            ConnectOrUpdateDB();
        }

        private void ImportCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
           
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show($"Casely version is: {Assembly.GetEntryAssembly().GetName().Version}");
        }
    }
}
