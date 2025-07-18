using PalletScanner.Data;

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

        private readonly Dictionary<IStatus, StatusBlock> statusBlocks = [];

        private void Model_StatusUpdated(IEnumerable<IStatus> statuses)
        {
            var statusArray = statuses.ToArray();
            /* Remove deleted blocks */ {
                HashSet<IStatus> toRemove = [.. statusBlocks.Keys];
                foreach (var status in statusArray) toRemove.Remove(status);
                foreach (var status in toRemove)
                {
                    ValidationStatusPanel.Controls.Remove(statusBlocks[status]);
                    statusBlocks.Remove(status);
                }
            }

            const int Margin = 3;
            int y = Margin;
            foreach (IStatus status in statuses)
            {
                if (statusBlocks.TryGetValue(status, out StatusBlock? block))
                {
                    block.Reload();
                }
                else
                {
                    block = new(status) { Width = ValidationStatusPanel.Width - 2 * Margin };
                    statusBlocks.Add(status, block);
                    ValidationStatusPanel.Controls.Add(block);
                }
                block.Location = new(Margin, y);
                y += Margin + block.Height;
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
