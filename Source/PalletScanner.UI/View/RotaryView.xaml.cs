using PalletScanner.Customers.Tyson;
using PalletScanner.Data;
using PalletScanner.Hardware.Cameras;
using PalletScanner.Hardware.StartStop;
using PalletScanner.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private const bool IsTest = false;

        public RotaryView()
        {
            InitializeComponent();
            DataContext = new RotaryViewModel(IsTest ? CreateTestModel() : CreateRotaryModel());
        }

        private void Window_Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private static RotaryModel CreateRotaryModel() => new(
            [
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.103"), "7-1-DM3812-371BE6"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.104"), "6-1-DM3812-371068"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.105"), "5-1-DM3812-371D96"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.106"), "4-1-DM3812-36A236"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.107"), "3-1-DM3812-371E12"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.108"), "2-1-DM3812-371DA6"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.109"), "1-1-DM3812-371E32"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.113"), "7-2-DM3812-371D9A"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.114"), "6-2-DM3812-371CAA"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.115"), "5-2-DM3812-370EE4"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.116"), "4-2-DM3812-371DAE"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.117"), "3-2-DM3812-371DB6"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.118"), "2-2-DM3812-371CA6"),
                new DatamanNetworkCamera(IPAddress.Parse("10.191.0.119"), "1-2-DM3812-36E25C")
            ],
            new ArduinoIf("COM3"),
            new Tyson()
        );
        private static RotaryModel CreateTestModel() => new(
            [
                new TestCamera("Test"),
                new DatamanNetworkCamera(IPAddress.Parse("192.168.1.42"), "DM262-852514") {
                    CameraPose = Pose.Identity,
                    CameraParams = new() {
                        CameraSize = new(1280, 960),
                        FocalLengthPixels = 1000
                    }
                }
            ],
            new TestStartStop(),
            new Tyson()
        );
    }
}
