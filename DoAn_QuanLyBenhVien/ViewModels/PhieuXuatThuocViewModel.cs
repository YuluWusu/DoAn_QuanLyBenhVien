using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class PhieuXuatThuocViewModel : BaseViewModel
    {
        // Dùng EF entity DONTHUOC cho danh sách đơn thuốc cần xuất
        private ObservableCollection<DONTHUOC> _listDonThuoc;
        public ObservableCollection<DONTHUOC> ListDonThuoc
        {
            get => _listDonThuoc;
            set { _listDonThuoc = value; OnPropertyChanged(); }
        }

        // Dùng ChiTietDonThuocLocal (UI-only) cho chi tiết hiển thị
        private ObservableCollection<ChiTietDonThuocLocal> _listChiTietThuoc;
        public ObservableCollection<ChiTietDonThuocLocal> ListChiTietThuoc
        {
            get => _listChiTietThuoc;
            set { _listChiTietThuoc = value; OnPropertyChanged(); }
        }

        private List<DONTHUOC> _originalData = new List<DONTHUOC>();

        private DONTHUOC _selectedDonThuoc;
        public DONTHUOC SelectedDonThuoc
        {
            get => _selectedDonThuoc;
            set
            {
                _selectedDonThuoc = value;
                OnPropertyChanged();
                if (_selectedDonThuoc != null) LoadChiTietThuoc();
                else
                {
                    ListChiTietThuoc?.Clear();
                    TongTien = 0;
                }
                XuatKhoCommand.RaiseCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ExecuteSearch(); }
        }

        private decimal _tongTien;
        public decimal TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        public RelayCommand XuatKhoCommand { get; }
        public RelayCommand SearchCommand { get; }

        public PhieuXuatThuocViewModel()
        {
            LoadData();
            SearchCommand = new RelayCommand(p => ExecuteSearch());
            XuatKhoCommand = new RelayCommand(
                p => ExecuteXuatKho(),
                p => SelectedDonThuoc != null && ListChiTietThuoc?.Count > 0
            );
        }

        // ✅ LoadData(): Thay mock data → query bảng DONTHUOC (chỉ lấy đơn chưa xuất)
        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    _originalData = db.DONTHUOCs
                        .Include(x => x.PHIEUKHAM)
                        .Where(x => x.TRANGTHAI.Trim().ToLower() == "chờ xuất")
                        .OrderByDescending(x => x.MA_DONTHUOC)
                        .ToList();
                    ListDonThuoc = new ObservableCollection<DONTHUOC>(_originalData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                _originalData = new List<DONTHUOC>();
                ListDonThuoc  = new ObservableCollection<DONTHUOC>();
            }
        }

        // ✅ LoadChiTietThuoc(): Thay switch/case mock → query EF theo MA_DONTHUOC
        private void LoadChiTietThuoc()
        {
            if (SelectedDonThuoc == null) return;

            var details = new List<ChiTietDonThuocLocal>();

            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    var chiTiets = db.CHITIET_DONTHUOC
                        .Include(x => x.THUOC)
                        .Where(x => x.MA_DONTHUOC == SelectedDonThuoc.MA_DONTHUOC)
                        .ToList();

                    foreach (var ct in chiTiets)
                    {
                        details.Add(new ChiTietDonThuocLocal
                        {
                            MaDonThuoc = ct.MA_DONTHUOC?.Trim(),
                            MaThuoc = ct.MA_THUOC?.Trim(),
                            TenThuoc = ct.THUOC?.TEN_THUOC ?? "N/A",
                            DVT = "", // Tạm thời để trống vì Model THUOC không có cột này
                            SoLuong = ct.SOLUONG ?? 0,
                            GiaBan = ct.GIA_LUC_BAN
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết thuốc: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ListChiTietThuoc = new ObservableCollection<ChiTietDonThuocLocal>(details);
            TongTien = ListChiTietThuoc.Sum(x => x.ThanhTien);
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ListDonThuoc = new ObservableCollection<DONTHUOC>(_originalData);
            }
            else
            {
                var kw = SearchText.ToLower();
                var filtered = _originalData.Where(x =>
                    (x.MA_DONTHUOC  != null && x.MA_DONTHUOC.ToLower().Contains(kw)) ||
                    (x.MA_PHIEUKHAM != null && x.MA_PHIEUKHAM.ToLower().Contains(kw))
                ).ToList();
                ListDonThuoc = new ObservableCollection<DONTHUOC>(filtered);
            }

            if (SelectedDonThuoc != null && !ListDonThuoc.Contains(SelectedDonThuoc))
                SelectedDonThuoc = null;
        }

        // ✅ ExecuteXuatKho(): Cập nhật TRANGTHAI + giảm SOLUONG_TON trong DB
        private void ExecuteXuatKho()
        {
            var result = MessageBox.Show(
                $"Xác nhận xuất kho đơn {SelectedDonThuoc.MA_DONTHUOC?.Trim()}?\nTổng tiền: {TongTien:N0} VNĐ",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        // ✅ Cập nhật trạng thái đơn thuốc
                        var dt = db.DONTHUOCs.Find(SelectedDonThuoc.MA_DONTHUOC);
                        if (dt != null)
                        {
                            dt.TRANGTHAI  = "Đã xuất";
                            dt.NGAYXUAT   = DateTime.Now;
                            dt.MANV_XUAT  = LoginViewModel.MaNVHienTai ?? "NV01";
                            db.Entry(dt).State = EntityState.Modified;
                        }

                        // ✅ Giảm SOLUONG_TON trong bảng THUOC
                        var chiTiets = db.CHITIET_DONTHUOC
                            .Where(x => x.MA_DONTHUOC == SelectedDonThuoc.MA_DONTHUOC)
                            .ToList();

                        foreach (var ct in chiTiets)
                        {
                            var thuoc = db.THUOCs.Find(ct.MA_THUOC);
                            if (thuoc != null && thuoc.SOLUONG_TON.HasValue)
                                thuoc.SOLUONG_TON = thuoc.SOLUONG_TON.Value - (ct.SOLUONG ?? 1);
                        }

                        db.SaveChanges();
                    }

                    MessageBox.Show("Xuất kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData(); // Refresh - đơn vừa xuất sẽ biến mất khỏi danh sách
                    SelectedDonThuoc = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất kho: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
