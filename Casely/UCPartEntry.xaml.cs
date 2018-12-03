using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CaselyData;

namespace Casely {


    /// <summary>
    /// Interaction logic for UCPartEntry.xaml
    /// </summary>
    public partial class UCPartEntry : UserControl {
        /// <summary>
        /// partEntry is used to give access to the author and other fields
        /// that are associated with this part entry control.
        /// </summary>
        public PartEntry partEntry;
        public UCPartEntry(PartEntry part) {
            InitializeComponent();
            this.DataContext = part;
            partEntry = part;
                        
        }

        public UCPartEntry(PartEntry part, List<string> suggestSpecimens, List<string> suggestProcedure) {
            InitializeComponent();
            this.DataContext = part;
            partEntry = part;
            tbProcedure.ItemsSource = suggestProcedure;
            tbSpecimen.ItemsSource = suggestSpecimens;

        }

        private void tbPart_GotFocus(object sender, RoutedEventArgs e) {
            tbPart.SelectAll();
        }

        private void tbSpecimen_GotFocus(object sender, RoutedEventArgs e) {
          //  tbSpecimen.();
        }

        private void tbProcedure_GotFocus(object sender, RoutedEventArgs e) {
          //  tbProcedure.SelectAll();
        }
    }
}
