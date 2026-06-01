using System;
using DoAn_QuanLyBenhVien.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DoAn_QuanLyBenhVien.Views;
using System.Windows.Threading;
using System.Windows.Media;
using DoAn_QuanLyBenhVien.Models;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class VM_Khung : BaseViewModel
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

        private string _userName;
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        private string _role;
        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        private string _dbConnectionStatus;
        public string DbConnectionStatus
        {
            get => _dbConnectionStatus;
            set { _dbConnectionStatus = value; OnPropertyChanged(); }
        }

        private Brush _dbConnectionColor;
        public Brush DbConnectionColor
        {
            get => _dbConnectionColor;
            set { _dbConnectionColor = value; OnPropertyChanged(); }
        }

        private string _tenTab;
        public string TenTab
        {
            get => _tenTab;
            set { _tenTab = value; OnPropertyChanged(); }
        }

        private DispatcherTimer _timer;

        public RelayCommand NavTrangChu { get; set; }
        public RelayCommand NavBacSi { get; set;  }
        public RelayCommand NavThuoc { get; set; }
        public RelayCommand NavHoSo { get; set; }
        public RelayCommand NavBenhNhan { get; set;  }
        public RelayCommand NavPhieuXuatThuoc { get; set;  }
        public RelayCommand NavDichVu {  get; set; }
        public RelayCommand NavDonThuoc { get; set; }
        public RelayCommand NavHoaDon {  get; set; }
        public RelayCommand NavThongKe { get; set; }
        public VM_Khung()
        {
            // Initialize User Info
            UserName = !string.IsNullOrEmpty(VM_Login.HoTenHienTai) ? VM_Login.HoTenHienTai : "Bác sĩ Minh";
            Role = !string.IsNullOrEmpty(VM_Login.QuyenHienTai) ? $"Phân quyền: {VM_Login.QuyenHienTai}" : "Phân quyền: Admin";
            
            // Set default DB Status
            DbConnectionStatus = "Đang kiểm tra CSDL...";
            DbConnectionColor = Brushes.Gray;

            // Timer for Real-time Clock
            CurrentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => { CurrentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); };
            _timer.Start();

            // Check DB Connection async
            CheckDatabaseConnection();
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
            NavPhieuXuatThuoc = new RelayCommand(o => { CurrentView = new UC_PhieuXuatThuoc(); });
            NavDichVu = new RelayCommand(o => { CurrentView = new UC_DichVu(); });
            NavHoaDon = new RelayCommand(o => { CurrentView = new UC_HoaDon(); });
            NavDonThuoc = new RelayCommand(o => { CurrentView = new UC_DonThuoc(); });
            NavThongKe = new RelayCommand(o => { CurrentView = new UC_ThongKe(); });
            NavTrangChu = new RelayCommand(o => { CurrentView = new UC_TrangChu(); });
            CurrentView = new UC_TrangChu();
            TenTab = CurrentView.ToString();
        }

        private async void CheckDatabaseConnection()
        {
            bool isConnected = false;
            await Task.Run(() =>
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        isConnected = db.Database.Exists();
                    }
                }
                catch { isConnected = false; }
            });

            if (isConnected)
            {
                DbConnectionStatus = "Đã kết nối CSDL";
                DbConnectionColor = (Brush)new BrushConverter().ConvertFrom("#10B981"); // Xanh lục
            }
            else
            {
                DbConnectionStatus = "Mất kết nối CSDL";
                DbConnectionColor = (Brush)new BrushConverter().ConvertFrom("#EF4444"); // Đỏ
            }
        }
    }
}



