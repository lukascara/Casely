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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using CaselyData;

namespace Casely {
    /// <summary>
    /// Interaction logic for UCdiagnosis.xaml
    /// </summary>
    public partial class UCdiagnosis : UserControl {

        public PartDiagnosis partDiagnosis;
        public UCdiagnosis(PartDiagnosis partDiagnosis) {
            InitializeComponent();
            this.DataContext = partDiagnosis;
            this.partDiagnosis = partDiagnosis;
        }

        public UCdiagnosis(PartDiagnosis partDiagnosis, List<String> suggestOrgan, List<String> suggestOrganSystem, List<String> suggestCategory, List<String> suggestDiagnosis) {
            InitializeComponent();
            this.DataContext = partDiagnosis;
            this.partDiagnosis = partDiagnosis;
            cmbCategory.ItemsSource = suggestCategory;
            cmbOrgan.ItemsSource = suggestOrgan;
            cmbOrganSystem.ItemsSource = suggestOrganSystem;
            cmbDiagnosis.ItemsSource = suggestDiagnosis;
        }
    }
}
