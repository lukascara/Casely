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
using DiffMatchPatch;
using CaselyData;

namespace Casely
{
    /// <summary>
    /// Interaction logic for WindowCompareReports.xaml
    /// </summary>
    public partial class WindowCompareReports : Window
    {
        bool caseNumberChanged = false;
        public WindowCompareReports()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            
        }

        private void txtCaseNumber_TextChanged(object sender, TextChangedEventArgs e) {
            caseNumberChanged = true;
        }

        private void txtCaseNumber_LostFocus(object sender, RoutedEventArgs e) {
            if (caseNumberChanged && txtCaseNumber.Text != "") {
                updateComparison();
                caseNumberChanged = false;
            }
        }

        private void updateComparison() {
            if (txtCaseNumber.Text != "") {
                var listCase = SqliteDataAcces.getListCaseEntry(txtCaseNumber.Text);
                /*foreach (var lc in listCase) {
                    ComboBoxItem cbitem = new ComboBoxItem();
                    cbitem.Content = lc.PrettyVersion();
                    cmbVersion.Items.Add(cbitem);
                }*/
                // gets the case entrys, groups them by author and then selects the last two author entries to compare.
                var listCaseToCompare = listCase.OrderByDescending(x => x.DateTimeModifiedObject).GroupBy(t => t.AuthorID).Select(x => x.FirstOrDefault()).ToList();
                if (listCaseToCompare.Count < 2) {
                    wbDiffText.Text = "<h3>Need at least a report from two different authors to compare</h3>";
                } else {
                    
                    string version0 = "";
                    string version1 = "";
                    if (cbInterpretation.IsChecked == true) {
                        version1 += $"-------Interpretation------------------------------------\n{listCaseToCompare[0].Interpretation}\n";
                        version0 += $"-------Interpretation------------------------------------\n{listCaseToCompare[1].Interpretation}\n";
                    }
                    if (cbResult.IsChecked == true) {
                        version1 += $"-------Result-------------------------------------------\n{listCaseToCompare[0].Result}\n";
                        version0 += $"-------Result-------------------------------------------\n{listCaseToCompare[1].Result}\n";
                    }
                    if (cbTumorSynoptic.IsChecked == true) {
                        version1 += $"-------Tumor Synoptic------------------------------------\n{listCaseToCompare[0].TumorSynoptic}\n";
                        version0 += $"-------Tumor Synoptic--------\n{listCaseToCompare[1].TumorSynoptic}\n";
                    }
                    if (cbComment.IsChecked == true) {
                        version1 += $"-------Comment-------------------------------------------\n{listCaseToCompare[0].Comment}\n";
                        version0 += $"-------Comment-------------------------------------------\n{listCaseToCompare[1].Comment}\n";
                    }
                    var dmp = DiffMatchPatchModule.Default;
                    var diffs = dmp.DiffMain(version0, version1);
                    dmp.DiffCleanupSemantic(diffs);
                    var html = dmp.DiffPrettyHtml(diffs).Replace("&para;","");
                    wbDiffText.Text = html;
                }
            }
        }
        private void cbInterpretation_Click(object sender, RoutedEventArgs e) {
            updateComparison();
        }

        private void cbResult_Click(object sender, RoutedEventArgs e) {
            updateComparison();
        }

        private void cbTumorSynoptic_Checked(object sender, RoutedEventArgs e) {
            updateComparison();
        }

        private void cbComment_Checked(object sender, RoutedEventArgs e) {
            updateComparison();
        }
    }

    
}
