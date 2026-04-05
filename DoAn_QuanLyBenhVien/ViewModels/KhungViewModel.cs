using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DoAn_QuanLyBenhVien.Views;
namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class KhungViewModel :BaseViewModel
    {
        UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }
        private string _currentDate;
        public string CurrentDate
        {
            get => _currentDate;
            set { _currentDate = value; OnPropertyChanged(); }
        }
        private string _tenTab;
        public string TenTab
        {
            get => _tenTab;
            set { _tenTab = value; OnPropertyChanged(); }
        }

        public RelayCommand NavBacSi { get; set;  }
        public RelayCommand NavThuoc { get; set; }
        public RelayCommand NavHoSo { get; set; }
        public RelayCommand NavBenhNhan { get; set;  }
        public KhungViewModel()
        {
            CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
            NavBacSi = new RelayCommand(o =>
            {
                CurrentView = new UC_NhanVien();
            });
            NavThuoc = new RelayCommand(o =>
            {
                CurrentView = new UC_Thuoc();
            });
            NavHoSo = new RelayCommand(o =>
            {
                CurrentView = new UC_HoSo();

            });
            NavBenhNhan = new RelayCommand(o =>
            {
                CurrentView = new UC_BenhNhan();
            });
            CurrentView = new UC_NhanVien();
            TenTab = CurrentView.ToString();
        }
    }
}
