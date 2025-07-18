using PalletScanner.Data;

namespace PalletScanner.UI
{
    public partial class StatusBlock : UserControl
    {
        private bool _isOpen = false;
        private bool _canOpen = false;
        private readonly IStatus _status;
        public StatusBlock(IStatus status)
        {
            _status = status;
            InitializeComponent();
            Reload();
        }

        public void Reload()
        {
            var children = _status.ChildStatus.ToArray();
            _canOpen = children.Length > 0;
            MessageLabel.Text = _status.Message;
            TypeLabel.Image = _status.Type switch
            {
                StatusType.Info => Properties.Resources.InfoIcon,
                StatusType.Warning => Properties.Resources.WarningIcon,
                StatusType.Error => Properties.Resources.ErrorIcon,
                _ => null
            };
            BackColor = _status.Type switch
            {
                StatusType.Info => Color.FromArgb(128, 128, 240),
                StatusType.Warning => Color.FromArgb(240, 16, 240),
                StatusType.Error => Color.FromArgb(240, 128, 128),
                _ => BackColor
            };
            DropDownLabel.Image = _canOpen ? _isOpen
                ? Properties.Resources.DropdownOpenIcon
                : Properties.Resources.DropdownClosedIcon
                : null;
        }

        private void DropDownLabel_Click(object sender, EventArgs e)
        {
            _isOpen = _canOpen && !_isOpen;
            Reload();
        }
    }
}
