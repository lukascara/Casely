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
            ge.Show();

        }
    }
}
