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
    public class PhieuXuatThuocViewModel:BaseViewModel
    {
        // 1. DANH SÁCH DỮ LIỆU
        private ObservableCollection<DonThuoc> _listDonThuoc;
        public ObservableCollection<DonThuoc> ListDonThuoc
        {
            get => _listDonThuoc;
            set { _listDonThuoc = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChiTietDonThuoc> _listChiTietThuoc;
        public ObservableCollection<ChiTietDonThuoc> ListChiTietThuoc
        {
            get => _listChiTietThuoc;
            set { _listChiTietThuoc = value; OnPropertyChanged(); }
        }

        private List<DonThuoc> _originalData = new List<DonThuoc>();

        // 2. BIẾN QUẢN LÝ CHỌN DÒNG VÀ TÌM KIẾM
        private DonThuoc _selectedDonThuoc;
        public DonThuoc SelectedDonThuoc
        {
            get => _selectedDonThuoc;
            set
            {
                _selectedDonThuoc = value;
                OnPropertyChanged();
                if (_selectedDonThuoc != null)
                {
                    LoadChiTietThuoc();
                }
                else
                {
                    if (ListChiTietThuoc != null) ListChiTietThuoc.Clear();
                    TongTien = 0;
                }
                // Cập nhật trạng thái nút bấm
                XuatKhoCommand.RaiseCanExecuteChanged();
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
                ExecuteSearch();
            }
        }

        private decimal _tongTien;
        public decimal TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        // 3. COMMANDS
        public RelayCommand XuatKhoCommand { get; }
        public RelayCommand SearchCommand { get; }

        public PhieuXuatThuocViewModel()
        {
            LoadData();

            // 2. Khởi tạo Command
            SearchCommand = new RelayCommand(p => ExecuteSearch());
            XuatKhoCommand = new RelayCommand(
                p => ExecuteXuatKho(),
                p => SelectedDonThuoc != null && ListChiTietThuoc?.Count > 0
            );
        }

        // 4. CÁC HÀM XỬ LÝ LOGIC
        void LoadData()
        {
            // Giả lập danh sách Đơn Thuốc
            _originalData = new List<DonThuoc>
            {
                new DonThuoc { MaDonThuoc = "DT001", MaPhieuKham = "PK001", NgayKe = DateTime.Now },
                new DonThuoc { MaDonThuoc = "DT002", MaPhieuKham = "PK002", NgayKe = DateTime.Now.AddDays(-1) },
                new DonThuoc { MaDonThuoc = "DT003", MaPhieuKham = "PK003", NgayKe = DateTime.Now.AddDays(-2) }
            };

            ListDonThuoc = new ObservableCollection<DonThuoc>(_originalData);
        }

        void LoadChiTietThuoc()
        {
            if (SelectedDonThuoc == null) return;

            var details = new List<ChiTietDonThuoc>();

            // Giả lập chi tiết thuốc khác nhau dựa trên MaDonThuoc
            if (SelectedDonThuoc.MaDonThuoc == "DT001")
            {
                details.Add(new ChiTietDonThuoc { TenThuoc = "Panadol Extra", SoLuong = 10, GiaBan = 2000 });
                details.Add(new ChiTietDonThuoc { TenThuoc = "Decolgen", SoLuong = 5, GiaBan = 1500 });
            }
            else if (SelectedDonThuoc.MaDonThuoc == "DT002")
            {
                details.Add(new ChiTietDonThuoc { TenThuoc = "Vitamin C 500mg", SoLuong = 20, GiaBan = 5000 });
            }
            else if (SelectedDonThuoc.MaDonThuoc == "DT003")
            {
                details.Add(new ChiTietDonThuoc { TenThuoc = "Amoxicillin", SoLuong = 14, GiaBan = 3000 });
                details.Add(new ChiTietDonThuoc { TenThuoc = "Strepsils", SoLuong = 2, GiaBan = 35000 });
            }

            ListChiTietThuoc = new ObservableCollection<ChiTietDonThuoc>(details);
            TinhTongTien();
        }

        void TinhTongTien()
        {
            if (ListChiTietThuoc == null || ListChiTietThuoc.Count == 0)
            {
                TongTien = 0;
                return;
            }

            TongTien = ListChiTietThuoc.Sum(x => x.ThanhTien);
        }

        private void ExecuteSearch()
        {
            // Sử dụng _originalData để luôn lọc trên danh sách đầy đủ
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ListDonThuoc = new ObservableCollection<DonThuoc>(_originalData);
            }
            else
            {
                var searchKeyword = SearchText.ToLower();
                var filtered = _originalData.Where(x =>
                    (x.MaDonThuoc != null && x.MaDonThuoc.ToLower().Contains(searchKeyword)) ||
                    (x.MaPhieuKham != null && x.MaPhieuKham.ToLower().Contains(searchKeyword))
                ).ToList();

                ListDonThuoc = new ObservableCollection<DonThuoc>(filtered);
            }

            // Sau khi lọc xong, nếu đơn thuốc đang chọn không còn nằm trong danh sách hiện tại
            // thì nên bỏ chọn để tránh lỗi logic khi nhấn nút Xuất
            if (SelectedDonThuoc != null && !ListDonThuoc.Contains(SelectedDonThuoc))
            {
                SelectedDonThuoc = null;
            }
        }

        private void ExecuteXuatKho()
        {
            var result = MessageBox.Show(
                $"Xác nhận xuất kho đơn {SelectedDonThuoc.MaDonThuoc}?\nTổng tiền: {TongTien:N0} VNĐ",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Thực hiện lưu xuống Database tại đây
                MessageBox.Show("Xuất kho thành công!", "Thông báo");

                LoadData(); // Load lại danh sách mới
                SelectedDonThuoc = null;
            }
        }
    }
}
