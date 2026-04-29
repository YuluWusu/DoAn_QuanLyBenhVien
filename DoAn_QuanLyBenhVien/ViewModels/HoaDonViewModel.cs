using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class HoaDonViewModel:BaseViewModel
    {
        private ObservableCollection<HoaDon> _listHoaDon;
        public ObservableCollection<HoaDon> ListHoaDon { get => _listHoaDon; set { _listHoaDon = value; OnPropertyChanged(); } }

        private ObservableCollection<ChiTietHoaDon> _listChiTiet;
        public ObservableCollection<ChiTietHoaDon> ListChiTiet { get => _listChiTiet; set { _listChiTiet = value; OnPropertyChanged(); } }

        private HoaDon _selectedHoaDon;
        public HoaDon SelectedHoaDon
        {
            get => _selectedHoaDon;
            set
            {
                _selectedHoaDon = value;
                OnPropertyChanged();
                if (_selectedHoaDon != null) LoadChiTiet();
            }
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Cứ mỗi khi gõ chữ, hàm Filter sẽ được gọi
                ExecuteFilter();
            }
        }

        // Cần thêm một danh sách gốc để lưu dữ liệu khi chưa lọc
        private List<HoaDon> _allHoaDon;
        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set { _tongTien = value; OnPropertyChanged(); } }

        public RelayCommand ThanhToanCommand { get; }

        public HoaDonViewModel()
        {
            // 1. Khởi tạo dữ liệu gốc
            _allHoaDon = new List<HoaDon>
            {
                new HoaDon { MaPhieuKham="PK001", TenBenhNhan="Nguyễn Văn A", TrangThai="Chưa thanh toán" },
                new HoaDon { MaPhieuKham="PK002", TenBenhNhan="Trần Thị B", TrangThai="Chưa thanh toán" }
            };

            // 2. Gán dữ liệu hiển thị ban đầu
            ListHoaDon = new ObservableCollection<HoaDon>(_allHoaDon);

            ThanhToanCommand = new RelayCommand(p => ExecuteThanhToan(), p => SelectedHoaDon != null);
        }

        void LoadChiTiet()
        {
            if (SelectedHoaDon == null) return;

            // Xóa dữ liệu cũ trước khi nạp mới
            var tempDetails = new ObservableCollection<ChiTietHoaDon>();

            // Giả lập dữ liệu khác nhau dựa trên Mã Phiếu
            if (SelectedHoaDon.MaPhieuKham == "PK001")
            {
                tempDetails.Add(new ChiTietHoaDon { Loai = "Dịch vụ", MaDoiTuong = "Khám nội", SoLuong = 1, DonGia = 150000 });
                tempDetails.Add(new ChiTietHoaDon { Loai = "Thuốc", MaDoiTuong = "Paracetamol", SoLuong = 10, DonGia = 2000 });
            }
            else if (SelectedHoaDon.MaPhieuKham == "PK002")
            {
                tempDetails.Add(new ChiTietHoaDon { Loai = "Dịch vụ", MaDoiTuong = "Siêu âm bụng", SoLuong = 1, DonGia = 250000 });
                tempDetails.Add(new ChiTietHoaDon { Loai = "Thuốc", MaDoiTuong = "Vitamin C", SoLuong = 5, DonGia = 5000 });
            }

            ListChiTiet = tempDetails;
            TongTien = ListChiTiet.Sum(x => x.ThanhTien);
        }

        void ExecuteThanhToan()
        {
            if (SelectedHoaDon == null) return;

            var result = MessageBox.Show($"Xác nhận thu tiền cho bệnh nhân {SelectedHoaDon.TenBenhNhan}?",
                "Xác nhận", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                SelectedHoaDon.TrangThai = "Đã thanh toán";

                MessageBox.Show("Thanh toán thành công! Đã cập nhật hóa đơn.");

                SelectedHoaDon = null;
            }
        }
        void ExecuteFilter()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                ListHoaDon = new ObservableCollection<HoaDon>(_allHoaDon);
            }
            else
            {
                var lowerSearch = SearchText.ToLower();
                var filteredList = _allHoaDon.Where(x =>
                    x.TenBenhNhan.ToLower().Contains(lowerSearch) ||
                    x.MaPhieuKham.ToLower().Contains(lowerSearch)
                ).ToList();

                ListHoaDon = new ObservableCollection<HoaDon>(filteredList);
            }
        }
    }
}
