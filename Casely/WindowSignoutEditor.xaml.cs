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
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void btnAddCase_Click(object sender, RoutedEventArgs e) {
            PathCase pc = new PathCase();
            pc.CaseNumber = txtCaseNumber.Text;
            pc.Service = cmbService.Text;
            CaseEntry ce = new CaseEntry();
            ce.AuthorFullName = cmbAuthor.Text;
            ce.Comment = txtComment.Text;
            ce.TumorSynoptic = txtComment.Text;
            PartEntry p = new PartEntry();
            p.TimeString = DateTime.Now.ToShortTimeString();
            p.DateString = DateTime.Now.ToShortDateString();
            p.Part = "A";
            

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

       
    }
   
}
