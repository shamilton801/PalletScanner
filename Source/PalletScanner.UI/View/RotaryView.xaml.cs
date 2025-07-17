using PalletScanner.UI.ViewModel;
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

namespace PalletScanner.UI.View
{
    /// <summary>
    /// Interaction logic for RotaryView.xaml
    /// </summary>
    public partial class RotaryView : Window
    {
        public RotaryView()
        {
            InitializeComponent();
            DataContext = new RotaryViewModel();
        }

        private void Window_Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
