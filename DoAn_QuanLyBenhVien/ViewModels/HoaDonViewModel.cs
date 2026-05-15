using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class HoaDonViewModel : BaseViewModel
    {
        // Dùng EF entity HOADON
        private ObservableCollection<HOADON> _listHoaDon;
        public ObservableCollection<HOADON> ListHoaDon
        {
            get => _listHoaDon;
            set { _listHoaDon = value; OnPropertyChanged(); }
        }

        // Dùng ChiTietHoaDonLoc (UI-only) từ LocalModels.cs
        private ObservableCollection<ChiTietHoaDonLoc> _listChiTiet;
        public ObservableCollection<ChiTietHoaDonLoc> ListChiTiet
        {
            get => _listChiTiet;
            set { _listChiTiet = value; OnPropertyChanged(); }
        }

        private HOADON _selectedHoaDon;
        public HOADON SelectedHoaDon
        {
            get => _selectedHoaDon;
            set
            {
                _selectedHoaDon = value;
                OnPropertyChanged();
                if (_selectedHoaDon != null) LoadChiTiet();
                else { ListChiTiet = new ObservableCollection<ChiTietHoaDonLoc>(); TongTien = 0; }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ExecuteFilter(); }
        }

        private List<HOADON> _allHoaDon;

        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set { _tongTien = value; OnPropertyChanged(); } }

        public RelayCommand ThanhToanCommand { get; }

        public HoaDonViewModel()
        {
            // ✅ Load từ Database thật
            LoadData();
            ThanhToanCommand = new RelayCommand(
                p => ExecuteThanhToan(),
                p => SelectedHoaDon != null && SelectedHoaDon.TRANGTHAI != "Đã thanh toán"
            );
        }

        // ✅ LoadData(): Load từ bảng HOADON
        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    _allHoaDon = db.HOADONs
                        .Include(h => h.PHIEUKHAM.BENHNHAN)
                        .OrderByDescending(x => x.MA_HOADON)
                        .ToList();
                    ListHoaDon = new ObservableCollection<HOADON>(_allHoaDon);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                _allHoaDon = new List<HOADON>();
                ListHoaDon = new ObservableCollection<HOADON>();
            }
        }

        // ✅ LoadChiTiet(): Query EF thay vì switch/case mock
        private void LoadChiTiet()
        {
            if (SelectedHoaDon == null) return;

            var tempDetails = new ObservableCollection<ChiTietHoaDonLoc>();
            var maPhieu = SelectedHoaDon.MA_PHIEUKHAM?.Trim();

            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    // Load dịch vụ từ PHIEUKHAM_DICHVU join DICHVU
                    var dichVus = db.PHIEUKHAM_DICHVU
                        .Where(x => x.MA_PHIEUKHAM == maPhieu)
                        .ToList();

                    foreach (var pkdv in dichVus)
                    {
                        tempDetails.Add(new ChiTietHoaDonLoc
                        {
                            Loai        = "Dịch vụ",
                            MaDoiTuong  = pkdv.DICHVU?.TEN_DICHVU ?? pkdv.MA_DICHVU,
                            SoLuong     = pkdv.SOLUONG ?? 1,
                            DonGia      = pkdv.GIA_LUC_DUNG
                        });
                    }

                    // Load thuốc từ DONTHUOC → CHITIET_DONTHUOC join THUOC
                    var donThuoc = db.DONTHUOCs
                        .FirstOrDefault(x => x.MA_PHIEUKHAM == maPhieu);

                    if (donThuoc != null)
                    {
                        var chiTiets = db.CHITIET_DONTHUOC
                            .Where(x => x.MA_DONTHUOC == donThuoc.MA_DONTHUOC)
                            .ToList();

                        foreach (var ct in chiTiets)
                        {
                            tempDetails.Add(new ChiTietHoaDonLoc
                            {
                                Loai       = "Thuốc",
                                MaDoiTuong = ct.THUOC?.TEN_THUOC ?? ct.MA_THUOC,
                                SoLuong    = ct.SOLUONG ?? 1,
                                DonGia     = ct.GIA_LUC_BAN
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ListChiTiet = tempDetails;
            TongTien    = ListChiTiet.Sum(x => x.ThanhTien);
        }

        // ✅ ExecuteThanhToan(): Cập nhật TRANGTHAI = 'Đã thanh toán' vào DB
        private void ExecuteThanhToan()
        {
            if (SelectedHoaDon == null) return;
            var result = MessageBox.Show(
                $"Xác nhận thu tiền hóa đơn {SelectedHoaDon.MA_HOADON?.Trim()} (Phiếu khám: {SelectedHoaDon.MA_PHIEUKHAM?.Trim()})?\nTổng tiền: {TongTien:N0} VNĐ",
                "Xác nhận", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        // ✅ Cập nhật trạng thái hóa đơn trong Database
                        var hd = db.HOADONs.Find(SelectedHoaDon.MA_HOADON);
                        if (hd != null)
                        {
                            hd.TRANGTHAI = "Đã thanh toán";
                            hd.TONGTIEN  = TongTien;
                            db.Entry(hd).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    SelectedHoaDon.TRANGTHAI = "Đã thanh toán";
                    MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData(); // Refresh danh sách
                    SelectedHoaDon = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thanh toán: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteFilter()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                ListHoaDon = new ObservableCollection<HOADON>(_allHoaDon);
            }
            else
            {
                var kw = SearchText.ToLower();
                var filtered = _allHoaDon.Where(x =>
                    (x.MA_HOADON    != null && x.MA_HOADON.ToLower().Contains(kw)) ||
                    (x.MA_PHIEUKHAM != null && x.MA_PHIEUKHAM.ToLower().Contains(kw))
                ).ToList();
                ListHoaDon = new ObservableCollection<HOADON>(filtered);
            }
        }
    }
}
