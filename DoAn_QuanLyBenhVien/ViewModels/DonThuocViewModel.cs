using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DonThuocViewModel : BaseViewModel
    {
        // -------------------------------------------------------------------
        // Dữ liệu - dùng DonThuocLocal và ChiTietDonThuocLocal (UI helper)
        // -------------------------------------------------------------------
        public ObservableCollection<DonThuocLocal> DanhSachDonThuoc { get; set; }
        private ObservableCollection<ChiTietDonThuocLocal> _toanBoChiTietDon;

        private ObservableCollection<ChiTietDonThuocLocal> _danhSachChiTietDon_HienThi;
        public ObservableCollection<ChiTietDonThuocLocal> DanhSachChiTietDon_HienThi
        {
            get => _danhSachChiTietDon_HienThi;
            set { _danhSachChiTietDon_HienThi = value; OnPropertyChanged(); }
        }

        // Danh sách tra cứu thuốc & bác sĩ (load từ DB)
        public List<ThuocGocLocal> DanhSachThuocGoc { get; set; }
        private ObservableCollection<BacSiLocal> _danhSachBacSi;
        public ObservableCollection<BacSiLocal> DanhSachBacSi
        {
            get => _danhSachBacSi;
            set { _danhSachBacSi = value; OnPropertyChanged(); }
        }

        // -------------------------------------------------------------------
        // Trạng thái Tab
        // -------------------------------------------------------------------
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                if (_selectedTabIndex == 1) LọcChiTietDonThuoc();
                if (!_isAdding && !_isEditing) ResetUIState();
            }
        }

        // -------------------------------------------------------------------
        // UI State
        // -------------------------------------------------------------------
        private string _addButtonContent = "➕ Thêm";
        public string AddButtonContent { get => _addButtonContent; set { _addButtonContent = value; OnPropertyChanged(); } }

        private bool _isSaveEnabled = false;
        public bool IsSaveEnabled { get => _isSaveEnabled; set { _isSaveEnabled = value; OnPropertyChanged(); } }

        private bool _isEditDeleteEnabled = false;
        public bool IsEditDeleteEnabled { get => _isEditDeleteEnabled; set { _isEditDeleteEnabled = value; OnPropertyChanged(); } }

        private bool _isAdding = false;
        private bool _isEditing = false;

        private bool _isProcessing = false;
        public bool IsProcessing { get => _isProcessing; set { _isProcessing = value; OnPropertyChanged(); } }

        // -------------------------------------------------------------------
        // Input fields - Tab 0 (Đơn thuốc)
        // -------------------------------------------------------------------
        private string _maDonThuocMoi;
        public string MaDonThuocMoi { get => _maDonThuocMoi; set { _maDonThuocMoi = value; OnPropertyChanged(); } }

        private string _maPhieuKhamMoi;
        public string MaPhieuKhamMoi { get => _maPhieuKhamMoi; set { _maPhieuKhamMoi = value; OnPropertyChanged(); } }

        private DateTime _ngayKeMoi = DateTime.Now;
        public DateTime NgayKeMoi { get => _ngayKeMoi; set { _ngayKeMoi = value; OnPropertyChanged(); } }

        private string _maNV_KeMoi;
        public string MaNV_KeMoi
        {
            get => _maNV_KeMoi;
            set
            {
                if (_maNV_KeMoi == value) return;
                _maNV_KeMoi = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(_maNV_KeMoi))
                {
                    var bs = DanhSachBacSi?.FirstOrDefault(x => x.MaNV.Trim().ToUpper() == _maNV_KeMoi.Trim().ToUpper());
                    if (SelectedBacSi != bs) SelectedBacSi = bs;
                }
                else SelectedBacSi = null;
            }
        }

        private DonThuocLocal _selectedDonThuoc;
        public DonThuocLocal SelectedDonThuoc
        {
            get => _selectedDonThuoc;
            set
            {
                _selectedDonThuoc = value;
                OnPropertyChanged();
                if (_selectedDonThuoc != null)
                {
                    MaDonThuocMoi  = _selectedDonThuoc.MaDonThuoc;
                    MaPhieuKhamMoi = _selectedDonThuoc.MaPhieuKham;
                    NgayKeMoi      = _selectedDonThuoc.NgayKe;
                    MaNV_KeMoi     = _selectedDonThuoc.MaNV_Ke;
                    SelectedBacSi  = DanhSachBacSi?.FirstOrDefault(x => x.MaNV?.Trim().ToUpper() == _selectedDonThuoc.MaNV_Ke?.Trim().ToUpper());
                    LọcChiTietDonThuoc();
                    IsEditDeleteEnabled = true;
                }
            }
        }

        // -------------------------------------------------------------------
        // Input fields - Tab 1 (Chi tiết đơn thuốc)
        // -------------------------------------------------------------------
        private string _maThuocMoi;
        public string MaThuocMoi
        {
            get => _maThuocMoi;
            set { _maThuocMoi = value; OnPropertyChanged(); AutoFillThuocInfo(); }
        }

        private string _dvtMoi;
        public string DVTMoi { get => _dvtMoi; set { _dvtMoi = value; OnPropertyChanged(); } }

        private int _soLuongMoi;
        public int SoLuongMoi { get => _soLuongMoi; set { _soLuongMoi = value; OnPropertyChanged(); } }

        private decimal _giaBanMoi;
        public decimal GiaBanMoi { get => _giaBanMoi; set { _giaBanMoi = value; OnPropertyChanged(); } }

        private string _lieuDungMoi;
        public string LieuDungMoi { get => _lieuDungMoi; set { _lieuDungMoi = value; OnPropertyChanged(); } }

        private BacSiLocal _selectedBacSi;
        public BacSiLocal SelectedBacSi
        {
            get => _selectedBacSi;
            set
            {
                if (_selectedBacSi == value) return;
                _selectedBacSi = value;
                OnPropertyChanged();
                if (_selectedBacSi != null && MaNV_KeMoi != _selectedBacSi.MaNV)
                    MaNV_KeMoi = _selectedBacSi.MaNV;
            }
        }

        private ChiTietDonThuocLocal _selectedChiTiet;
        public ChiTietDonThuocLocal SelectedChiTiet
        {
            get => _selectedChiTiet;
            set
            {
                _selectedChiTiet = value;
                OnPropertyChanged();
                if (_selectedChiTiet != null)
                {
                    MaThuocMoi = _selectedChiTiet.MaThuoc;
                    DVTMoi     = _selectedChiTiet.DVT;
                    SoLuongMoi = _selectedChiTiet.SoLuong;
                    LieuDungMoi = _selectedChiTiet.LieuDung;
                    IsEditDeleteEnabled = true;
                    IsSaveEnabled = false;
                    IsAddingEditingState(false);
                }
            }
        }

        private ThuocGocLocal _selectedThuocHienTai;
        public ThuocGocLocal SelectedThuocHienTai
        {
            get => _selectedThuocHienTai;
            set
            {
                _selectedThuocHienTai = value;
                OnPropertyChanged();
                if (_selectedThuocHienTai != null)
                {
                    MaThuocMoi = _selectedThuocHienTai.MaThuoc;
                    DVTMoi     = _selectedThuocHienTai.DVT;
                    GiaBanMoi  = _selectedThuocHienTai.GiaBan;
                }
            }
        }

        // -------------------------------------------------------------------
        // Commands
        // -------------------------------------------------------------------
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ChiTietCommand { get; }

        public DonThuocViewModel()
        {
            AddCommand     = new RelayCommand(ExecuteAdd);
            UpdateCommand  = new RelayCommand(ExecuteUpdate, CanExecuteUpdateDelete);
            DeleteCommand  = new RelayCommand(ExecuteDelete, CanExecuteUpdateDelete);
            SaveCommand    = new RelayCommand(ExecuteSave, CanExecuteSave);
            ChiTietCommand = new RelayCommand(ExecuteChiTiet, CanExecuteChiTiet);

            // ✅ Load từ Database thật
            LoadData();
            SelectedDonThuoc = null;
            SelectedChiTiet  = null;
            ClearFields();
            ResetUIState();
        }

        // -------------------------------------------------------------------
        // Command Handlers
        // -------------------------------------------------------------------
        private void ExecuteAdd(object obj)
        {
            if (!_isAdding && !_isEditing)
            {
                IsAddingEditingState(true);
                IsSaveEnabled = true; IsEditDeleteEnabled = false;
                AddButtonContent = "❌ Hủy"; ClearFields();
                if (SelectedTabIndex == 0)
                {
                    MaDonThuocMoi = "DT" + DateTime.Now.ToString("HHmmssff");
                }
            }
            else { IsAddingEditingState(false); ResetUIState(); ClearFields(); }
        }

        private void ExecuteUpdate(object obj)
        {
            _isEditing = true; IsProcessing = true;
            IsSaveEnabled = true; IsEditDeleteEnabled = false;
            AddButtonContent = "❌ Hủy";
        }

        private void ExecuteDelete(object obj)
        {
            if (SelectedTabIndex == 0 && SelectedDonThuoc != null)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa đơn thuốc này?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new QL_PHONG_KHAM())
                        {
                            // ✅ Xóa CHITIET trước, sau đó xóa DONTHUOC
                            var chitiets = db.CHITIET_DONTHUOC.Where(x => x.MA_DONTHUOC == SelectedDonThuoc.MaDonThuoc).ToList();
                            db.CHITIET_DONTHUOC.RemoveRange(chitiets);
                            var dt = db.DONTHUOCs.Find(SelectedDonThuoc.MaDonThuoc);
                            if (dt != null) db.DONTHUOCs.Remove(dt);
                            db.SaveChanges();
                        }
                        DanhSachDonThuoc.Remove(SelectedDonThuoc);
                        ClearFields(); ResetUIState();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (SelectedTabIndex == 1 && SelectedChiTiet != null)
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        // ✅ Xóa chi tiết đơn thuốc khỏi DB (tìm bằng FirstOrDefault vì composite key)
                        var ct = db.CHITIET_DONTHUOC.FirstOrDefault(x =>
                            x.MA_DONTHUOC == SelectedChiTiet.MaDonThuoc &&
                            x.MA_THUOC    == SelectedChiTiet.MaThuoc);
                        if (ct != null) { db.CHITIET_DONTHUOC.Remove(ct); db.SaveChanges(); }
                    }
                    _toanBoChiTietDon.Remove(SelectedChiTiet);
                    LọcChiTietDonThuoc();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            ClearFields(); ResetUIState();
        }

        private bool CanExecuteSave(object obj)
        {
            if (!_isAdding && !_isEditing) return false;
            if (SelectedTabIndex == 0)
                return !string.IsNullOrWhiteSpace(MaDonThuocMoi) && !string.IsNullOrWhiteSpace(MaPhieuKhamMoi);
            else
                return !string.IsNullOrWhiteSpace(MaThuocMoi) && SoLuongMoi > 0;
        }

        private bool CanExecuteUpdateDelete(object obj)
        {
            if (IsProcessing) return false;
            return SelectedTabIndex == 0 ? SelectedDonThuoc != null : SelectedChiTiet != null;
        }

        private bool CanExecuteChiTiet(object obj) => SelectedDonThuoc != null && !IsProcessing;

        private void ExecuteSave(object obj)
        {
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    if (SelectedTabIndex == 0)
                    {
                        var bsThat = DanhSachBacSi?.FirstOrDefault(x => x.MaNV?.Trim().ToUpper() == MaNV_KeMoi?.Trim().ToUpper());
                        if (bsThat == null)
                        { MessageBox.Show("Mã nhân viên không tồn tại!"); return; }

                        string tenHienThi = SelectedBacSi?.HoTen ?? bsThat.HoTen;

                        if (_isAdding)
                        {
                            // ✅ Kiểm tra mã phiếu khám tồn tại
                            if (!db.PHIEUKHAMs.Any(x => x.MA_PHIEUKHAM == MaPhieuKhamMoi))
                            { MessageBox.Show("Mã phiếu khám không tồn tại trong hệ thống!"); return; }

                            var newDT = new DONTHUOC
                            {
                                MA_DONTHUOC  = MaDonThuocMoi,
                                MA_PHIEUKHAM = MaPhieuKhamMoi,
                                MANV_KE      = MaNV_KeMoi,
                                TRANGTHAI    = "Chờ xuất"
                            };
                            db.DONTHUOCs.Add(newDT);
                            db.SaveChanges();

                            DanhSachDonThuoc.Add(new DonThuocLocal
                            {
                                MaDonThuoc = MaDonThuocMoi, MaPhieuKham = MaPhieuKhamMoi,
                                NgayKe = NgayKeMoi, MaNV_Ke = MaNV_KeMoi, TenBacSiKe = tenHienThi
                            });
                        }
                        else if (_isEditing && SelectedDonThuoc != null)
                        {
                            // ✅ Cập nhật DB
                            var dt = db.DONTHUOCs.Find(SelectedDonThuoc.MaDonThuoc);
                            if (dt != null)
                            {
                                dt.MA_PHIEUKHAM = MaPhieuKhamMoi;
                                dt.MANV_KE      = MaNV_KeMoi;
                                db.Entry(dt).State = EntityState.Modified;
                                db.SaveChanges();

                                var idx = DanhSachDonThuoc.IndexOf(SelectedDonThuoc);
                                if (idx >= 0)
                                {
                                    var updated = new DonThuocLocal
                                    {
                                        MaDonThuoc = MaDonThuocMoi, MaPhieuKham = MaPhieuKhamMoi,
                                        NgayKe = NgayKeMoi, MaNV_Ke = MaNV_KeMoi, TenBacSiKe = tenHienThi
                                    };
                                    DanhSachDonThuoc[idx] = updated;
                                    SelectedDonThuoc = updated;
                                }
                            }
                        }
                    }
                    else if (SelectedTabIndex == 1)
                    {
                        var thuocInfo = DanhSachThuocGoc?.FirstOrDefault(x => x.MaThuoc == MaThuocMoi);

                        if (_isAdding && SelectedDonThuoc != null)
                        {
                            if (db.CHITIET_DONTHUOC.Any(x => x.MA_DONTHUOC == SelectedDonThuoc.MaDonThuoc && x.MA_THUOC == MaThuocMoi))
                            {
                                MessageBox.Show("Thuốc này đã có trong đơn, vui lòng chọn dòng đó và bấm Sửa để cập nhật số lượng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            // ✅ Thêm chi tiết đơn thuốc vào DB
                            db.CHITIET_DONTHUOC.Add(new CHITIET_DONTHUOC
                            {
                                MA_DONTHUOC = SelectedDonThuoc.MaDonThuoc,
                                MA_THUOC    = MaThuocMoi,
                                SOLUONG     = SoLuongMoi,
                                GIA_LUC_BAN = GiaBanMoi
                            });
                            db.SaveChanges();

                            _toanBoChiTietDon.Add(new ChiTietDonThuocLocal
                            {
                                MaDonThuoc = SelectedDonThuoc.MaDonThuoc, MaThuoc = MaThuocMoi,
                                TenThuoc = thuocInfo?.TenThuoc, DVT = DVTMoi,
                                SoLuong = SoLuongMoi, GiaBan = GiaBanMoi, LieuDung = LieuDungMoi
                            });
                        }
                        else if (_isEditing && SelectedChiTiet != null)
                        {
                            // ✅ Cập nhật chi tiết trong DB
                            var ct = db.CHITIET_DONTHUOC.Find(SelectedChiTiet.MaDonThuoc, SelectedChiTiet.MaThuoc);
                            if (ct != null)
                            {
                                ct.SOLUONG     = SoLuongMoi;
                                ct.GIA_LUC_BAN = GiaBanMoi;
                                db.Entry(ct).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            SelectedChiTiet.MaThuoc  = MaThuocMoi;
                            SelectedChiTiet.TenThuoc  = thuocInfo?.TenThuoc;
                            SelectedChiTiet.DVT       = DVTMoi;
                            SelectedChiTiet.SoLuong   = SoLuongMoi;
                            SelectedChiTiet.GiaBan    = GiaBanMoi;
                            SelectedChiTiet.LieuDung  = LieuDungMoi;
                            var idx = _toanBoChiTietDon.IndexOf(SelectedChiTiet);
                            if (idx >= 0) _toanBoChiTietDon[idx] = SelectedChiTiet;
                        }
                        LọcChiTietDonThuoc();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            IsAddingEditingState(false); ResetUIState(); ClearFields();
        }

        private void ExecuteChiTiet(object obj)
        {
            if (SelectedDonThuoc != null)
            { SelectedTabIndex = 1; LọcChiTietDonThuoc(); OnPropertyChanged(nameof(SelectedTabIndex)); }
            else
                MessageBox.Show("Vui lòng chọn một đơn thuốc trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // -------------------------------------------------------------------
        // Helper Methods
        // -------------------------------------------------------------------
        private void LọcChiTietDonThuoc()
        {
            if (SelectedDonThuoc != null && _toanBoChiTietDon != null)
            {
                var filtered = _toanBoChiTietDon.Where(x => x.MaDonThuoc == SelectedDonThuoc.MaDonThuoc).ToList();
                DanhSachChiTietDon_HienThi = new ObservableCollection<ChiTietDonThuocLocal>(filtered);
            }
            else
                DanhSachChiTietDon_HienThi = new ObservableCollection<ChiTietDonThuocLocal>();
        }

        private void AutoFillThuocInfo()
        {
            var thuoc = DanhSachThuocGoc?.FirstOrDefault(x => x.MaThuoc == MaThuocMoi);
            if (thuoc != null) { DVTMoi = thuoc.DVT; GiaBanMoi = thuoc.GiaBan; }
        }

        private void IsAddingEditingState(bool state)
        {
            _isAdding = state; _isEditing = false;
            IsProcessing = state;
            AddButtonContent = state ? "❌ Hủy" : "➕ Thêm";
        }

        private void ResetUIState()
        {
            AddButtonContent = "➕ Thêm"; IsSaveEnabled = false;
            IsEditDeleteEnabled = (SelectedDonThuoc != null || SelectedChiTiet != null);
            IsProcessing = false; _isAdding = false; _isEditing = false;
        }

        private void ClearFields()
        {
            if (SelectedTabIndex == 0)
            { MaDonThuocMoi = ""; MaPhieuKhamMoi = ""; NgayKeMoi = DateTime.Now; MaNV_KeMoi = ""; SelectedBacSi = null; }
            else
            { MaThuocMoi = ""; SelectedThuocHienTai = null; DVTMoi = ""; SoLuongMoi = 0; GiaBanMoi = 0; LieuDungMoi = ""; }
        }

        // ✅ LoadData(): Thay LoadDummyData() bằng load từ Database thực
        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    // 1. Load thuốc → ThuocGocLocal
                    DanhSachThuocGoc = db.THUOCs.ToList()
                        .Select(t => new ThuocGocLocal
                        {
                            MaThuoc  = t.MA_THUOC?.Trim(),
                            TenThuoc = t.TEN_THUOC,
                            DVT      = "",          // Không có trong entity THUOC
                            GiaBan   = t.GIA_BAN
                        }).ToList();

                    // 2. Load nhân viên → BacSiLocal
                    DanhSachBacSi = new ObservableCollection<BacSiLocal>(
                        db.NHANVIENs.ToList().Select(nv => new BacSiLocal
                        {
                            MaNV  = nv.MaNV?.Trim(),
                            HoTen = nv.HoTen
                        })
                    );

                    // 3. Load đơn thuốc → DonThuocLocal (join NHANVIEN để lấy tên)
                    var donThuocs = db.DONTHUOCs
                        .OrderByDescending(x => x.MA_DONTHUOC)
                        .ToList()
                        .Select(dt => new DonThuocLocal
                        {
                            MaDonThuoc  = dt.MA_DONTHUOC?.Trim(),
                            MaPhieuKham = dt.MA_PHIEUKHAM?.Trim(),
                            NgayKe      = DateTime.Today,
                            MaNV_Ke     = dt.MANV_KE?.Trim(),
                            TenBacSiKe  = dt.NHANVIEN?.HoTen ?? dt.MANV_KE
                        }).ToList();
                    DanhSachDonThuoc = new ObservableCollection<DonThuocLocal>(donThuocs);

                    // 4. Load chi tiết đơn thuốc → ChiTietDonThuocLocal (join THUOC để lấy tên)
                    var chiTiets = db.CHITIET_DONTHUOC
                        .ToList()
                        .Select(ct => new ChiTietDonThuocLocal
                        {
                            MaDonThuoc = ct.MA_DONTHUOC?.Trim(),
                            MaThuoc    = ct.MA_THUOC?.Trim(),
                            TenThuoc   = ct.THUOC?.TEN_THUOC,
                            DVT        = "",
                            SoLuong    = ct.SOLUONG ?? 1,
                            GiaBan     = ct.GIA_LUC_BAN,
                            LieuDung   = ""
                        }).ToList();
                    _toanBoChiTietDon = new ObservableCollection<ChiTietDonThuocLocal>(chiTiets);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                DanhSachThuocGoc  = new List<ThuocGocLocal>();
                DanhSachBacSi     = new ObservableCollection<BacSiLocal>();
                DanhSachDonThuoc  = new ObservableCollection<DonThuocLocal>();
                _toanBoChiTietDon = new ObservableCollection<ChiTietDonThuocLocal>();
            }
        }
    }
}