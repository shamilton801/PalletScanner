using PalletScanner.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

        private readonly Dictionary<IStatus, StatusBlock> childStatusBlocks = [];
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
            if (!_canOpen || !_isOpen)
            {
                Height = StatusBlockBasicHeight;
                return;
            }
            int y = StatusBlockBasicHeight + StatusBlockListMargin;
            UpdateStatusBlockList(this, childStatusBlocks, ref y, children);
            Height = y;
        }

        public const int StatusBlockBasicHeight = 40;
        public const int StatusBlockListMargin = 3;
        public static void UpdateStatusBlockList(
            Control control,
            Dictionary<IStatus, StatusBlock> statusBlocks,
            ref int y,
            IStatus[] statuses)
        {
            /* Remove deleted blocks */
            {
                HashSet<IStatus> toRemove = [.. statusBlocks.Keys];
                foreach (var status in statuses) toRemove.Remove(status);
                foreach (var status in toRemove)
                {
                    control.Controls.Remove(statusBlocks[status]);
                    statusBlocks.Remove(status);
                }
            }

            foreach (IStatus status in statuses)
            {
                if (statusBlocks.TryGetValue(status, out StatusBlock? block))
                {
                    block.Reload();
                }
                else
                {
                    block = new(status) { Width = control.Width - 2 * StatusBlockListMargin };
                    statusBlocks.Add(status, block);
                    control.Controls.Add(block);
                }
                block.Location = new(StatusBlockListMargin, y);
                y += StatusBlockListMargin + block.Height;
            }
        }

        private void DropDownLabel_Click(object sender, EventArgs e)
        {
            _isOpen = _canOpen && !_isOpen;
            Reload();
        }
    }
}
