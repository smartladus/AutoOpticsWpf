using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using AutoOpticsWpf.Utils;

namespace AutoOpticsWpf.ViewModels
{
    internal class HomeViewModel: ObservableRecipient
    {
        private readonly AutoWhiteBalanceManager _awbManager = AutoWhiteBalanceManager.GetInstance();


        public ICommand PerformAutoWhiteBalanceCommand { get; }

        private void PerformAutoWhiteBalance()
        {
            _awbManager.PerformAutoWhiteBalance(10);
        }

        public HomeViewModel()
        {
            PerformAutoWhiteBalanceCommand = new RelayCommand(PerformAutoWhiteBalance);

        }
    }
}
