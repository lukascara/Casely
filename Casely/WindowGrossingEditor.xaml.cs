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
using System.Collections.ObjectModel;

namespace Casely {
    /// <summary>
    /// Interaction logic for GrossingEditor.xaml
    /// </summary>
    public partial class GrossingEditor : Window {

        ObservableCollection<PartEntry> listCaseParts;
        public GrossingEditor() {
            InitializeComponent();
            listCaseParts = new ObservableCollection<PartEntry>(SqliteDataAcces.getParts("1"));

            CollectionViewSource itemCollectionViewSource;
            itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
            itemCollectionViewSource.Source = listCaseParts;
        }
    }
}
