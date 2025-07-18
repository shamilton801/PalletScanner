using PalletScanner.Data;
using System.ComponentModel;
using System.Windows.Input;

namespace PalletScanner.UI.ViewModel
{
    public class RotaryViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Member Variables
        private string _topPalletBarcode;
        private string _bottomPalletBarcode;
        private string _topPalletFailureMessage;
        private string _bottomPalletFailureMessage;
        private string _conveyorTextColor; 
        private string _conveyorListColor;
        #endregion

        #region Properties
        public string ConveyorListColor
        {
            get => _conveyorListColor;
            set => SetProperty(ref _conveyorListColor, value);
        }

        public string ConveyorTextColor
        {
            get => _conveyorTextColor;
            set => SetProperty(ref _conveyorTextColor, value);
        }

        public string TopPalletBarcode
        {
            get => _topPalletBarcode;
            set => SetProperty(ref _topPalletBarcode, value);
        }

        public string BottomPalletBarcode
        {
            get => _bottomPalletBarcode;
            set => SetProperty(ref _bottomPalletBarcode, value);
        }

        public string TopPalletFailureMessage
        {
            get => _topPalletFailureMessage;
            set => SetProperty(ref _topPalletFailureMessage, value);
        }

        public string BottomPalletFailureMessage
        {
            get => _bottomPalletFailureMessage;
            set => SetProperty(ref _bottomPalletFailureMessage, value);
        }
        #endregion

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RotaryViewModel(RotaryModel model)
        {
            StartCommand = new RelayCommand(model.StartScan);
            StopCommand = new RelayCommand(model.StopScan);
            model.StatusUpdated += UpdateDisplay;
        }

        private void UpdateDisplay(IEnumerable<IStatus> statuses)
        {
            ClearUI();

            foreach (var status in statuses)
            {
                switch (status.Type)
                {
                    case StatusType.Info:
                        ConveyorListColor = "#47CD89";
                        ConveyorTextColor = "#344054";
                        break;
                    default:
                        TopPalletFailureMessage = status.Message;
                        ConveyorListColor = "#F97066";
                        ConveyorTextColor = "#FFFFFF";
                        break;
                }
            }

            //TopPalletBarcode = args.UID;
            //if (args.State == GateKeeperServer.Events.EventState.Error)
            //{
            //    ShowTopFail = true;
            //    TopPalletFailureMessage = args.Message;
            //    ConveyorListColor = "#F97066";
            //    ConveyorTextColor = "#FFFFFF";
            //}
            //else
            //{
            //    ShowTopPass = true;
            //    ConveyorListColor = "#47CD89";
            //    ConveyorTextColor = "#344054";
            //}
        }

        private void ClearUI()
        {
            TopPalletBarcode = string.Empty;
            TopPalletFailureMessage = string.Empty;
            BottomPalletBarcode = string.Empty;
            BottomPalletBarcode = string.Empty;
        }
    }
}
