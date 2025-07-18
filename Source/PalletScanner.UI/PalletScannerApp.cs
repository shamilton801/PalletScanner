using PalletScanner.Data;
using PalletScanner.Hardware.Cameras;
using PalletScanner.Hardware.StartStop;

namespace PalletScanner.UI
{
    public partial class PalletScannerApp : Form
    {
        private readonly RotaryModel _model;

        private bool ButtonModeRunning
        {
            set
            {
                StartButton.Enabled = !value;
                StopButton.Enabled = value;
                TimedScanButton.Enabled = !value;
            }
        }

        public PalletScannerApp(RotaryModel model)
        {
            _model = model;
            InitializeComponent();
            model.StatusUpdated += statuses => BeginInvoke(() => Model_StatusUpdated(statuses));
        }

        private void Model_StatusUpdated(IEnumerable<IStatus> statuses)
        {
            ValidationStatusPanel.Controls.Clear();
            const int Margin = 3;
            const int Height = 40;
            int y = Margin;
            foreach (IStatus status in statuses)
            {
                Label ctrl = new()
                {
                    Text = status.Message,
                    Location = new(Margin, y),
                    Size = new(ValidationStatusPanel.Width - 2 * Margin, Height),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left,
                };
                ctrl.Font = new Font(ctrl.Font.FontFamily, 14);
                y += Height + Margin;
                ValidationStatusPanel.Controls.Add(ctrl);
            }
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            _model.StartScan();
            ButtonModeRunning = true;
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            _model.StopScan();
            ButtonModeRunning = false;
        }
        private void TimedScanButton_Click(object sender, EventArgs e)
        {
            _model.StartTimedScan(() =>
            {
                // Invoke forces argument to run on GUI thread
                // WinForms will not allow cross-thread interaction
                Invoke(() => ButtonModeRunning = false);
            });
            ButtonModeRunning = true;
        }

        private void PalletScannerApp_FormClosed(object sender, FormClosedEventArgs e) => _model.StopScan();
    }
}
