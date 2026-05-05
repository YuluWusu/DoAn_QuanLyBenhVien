using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    // KetQuaKham - lưu kết quả một lần khám trên UI
    public class KetQuaKham : BaseViewModel
    {
        public string MaPhieuKham { get; set; }
        public DateTime NgayKham { get; set; }
        public BENHNHAN BN { get; set; }
        public string TrieuChung { get; set; }
        public string ChuanDoan { get; set; }
        public System.Collections.Generic.List<DichVuKhamLocal> DichVuDaDung { get; set; }
    }

    public class HoSoKhamViewModel : BaseViewModel
    {
        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set { _selectedTabIndex = value; OnPropertyChanged(); }
        }

        private string _trieuChung;
        public string TrieuChung { get => _trieuChung; set { _trieuChung = value; OnPropertyChanged(); } }

        private string _chuanDoan;
        public string ChuanDoan { get => _chuanDoan; set { _chuanDoan = value; OnPropertyChanged(); } }

        // Danh sách bệnh nhân chờ khám (EF entity)
        public ObservableCollection<BENHNHAN> DSBNChuaKham { get; set; }

        // Danh sách dịch vụ (UI helper - có IsSelected) - ✅ Load từ DB
        public ObservableCollection<DichVuKhamLocal> DSDichVu { get; set; }

        // Danh sách thuốc đã chọn (EF entity THUOC)
        public ObservableCollection<THUOC> DSThuocChon { get; set; }

        private ObservableCollection<KetQuaKham> _dsLichSuKham;
        public ObservableCollection<KetQuaKham> DSLichSuKham
        {
            get => _dsLichSuKham;
            set { _dsLichSuKham = value; OnPropertyChanged(); }
        }

        private Visibility _isVisibleDetail = Visibility.Collapsed;
        public Visibility IsVisibleDetail
        {
            get => _isVisibleDetail;
            set { _isVisibleDetail = value; OnPropertyChanged(); }
        }

        private bool _isSua;
        public bool IsSua { get => _isSua; set { _isSua = value; OnPropertyChanged(); } }

        private KetQuaKham _selectedLichSu;
        public KetQuaKham SelectedLichSu
        {
            get => _selectedLichSu;
            set
            {
                _selectedLichSu = value;
                OnPropertyChanged();
                if (value != null)
                {
                    IsVisibleDetail = Visibility.Visible;
                    IsSua = false;
                    // Đánh dấu lại checkbox dịch vụ
                    foreach (var dv in DSDichVu)
                        dv.IsSelected = value.DichVuDaDung?.Any(x => x.MA_DICHVU == dv.MA_DICHVU) ?? false;
                }
            }
        }

        // Thông tin bệnh nhân đang hiển thị
        private string _thongTinMaBN;
        public string ThongTinMaBN { get => _thongTinMaBN; set { _thongTinMaBN = value; OnPropertyChanged(); } }

        private string _thongTinTenBN;
        public string ThongTinTenBN { get => _thongTinTenBN; set { _thongTinTenBN = value; OnPropertyChanged(); } }

        private string _thongTinGioiTinh;
        public string ThongTinGioiTinh { get => _thongTinGioiTinh; set { _thongTinGioiTinh = value; OnPropertyChanged(); } }

        private string _thongTinNgaySinh;
        public string ThongTinNgaySinh { get => _thongTinNgaySinh; set { _thongTinNgaySinh = value; OnPropertyChanged(); } }

        private BENHNHAN _bnDangChonVaoKham;
        public BENHNHAN BNDangChonVaoKham
        {
            get => _bnDangChonVaoKham;
            set
            {
                _bnDangChonVaoKham = value;
                OnPropertyChanged();
                if (value != null)
                {
                    ThongTinMaBN     = value.MA_BENHNHAN?.Trim();
                    ThongTinTenBN    = value.TEN_BENHNHAN;
                    ThongTinGioiTinh = value.GIOITINH;
                    ThongTinNgaySinh = value.NGAYSINH.HasValue
                        ? value.NGAYSINH.Value.ToString("dd/MM/yyyy") : "";
                }
                else
                {
                    ThongTinMaBN = ""; ThongTinTenBN = ""; ThongTinGioiTinh = ""; ThongTinNgaySinh = "";
                }
            }
        }

        public ICommand LenhLuu { get; set; }
        public ICommand LenhHuy { get; set; }
        public ICommand LenhSuaLichSu { get; set; }
        public ICommand LenhXoaLichSu { get; set; }
        public ICommand LenhLuuLichSu { get; set; }

        public HoSoKhamViewModel()
        {
            DSLichSuKham = new ObservableCollection<KetQuaKham>();
            DSThuocChon  = new ObservableCollection<THUOC>();

            // ✅ Load dữ liệu từ Database
            LoadData();

            // ----------------------------------------------------------------
            // LenhLuu: Lưu hồ sơ khám (Insert vào DB qua Entity Framework)
            // ----------------------------------------------------------------
            LenhLuu = new RelayCommand(p =>
            {
                if (BNDangChonVaoKham == null)
                {
                    MessageBox.Show("Vui lòng chọn một bệnh nhân từ danh sách chờ!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool coDichVu = DSDichVu.Any(x => x.IsSelected);
                if (string.IsNullOrWhiteSpace(TrieuChung) || string.IsNullOrWhiteSpace(ChuanDoan) || !coDichVu)
                {
                    MessageBox.Show("BẮT BUỘC: Nhập Triệu chứng, Chẩn đoán và chọn ít nhất 1 dịch vụ!",
                        "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- Lưu xuống Database bằng Entity Framework ---
                using (var context = new QL_PHONG_KHAM())
                {
                    try
                    {
                        string maPhieu    = "PK" + DateTime.Now.ToString("ddHHmm") + new Random().Next(10,99).ToString();
                        string maDonThuoc = "DT" + DateTime.Now.ToString("ddHHmm") + new Random().Next(10,99).ToString();

                        // Lấy mã NV đang đăng nhập
                        string maNVHienTai = LoginViewModel.MaNVHienTai ?? "NV01";

                        // 1. PHIEUKHAM
                        context.PHIEUKHAMs.Add(new PHIEUKHAM
                        {
                            MA_PHIEUKHAM = maPhieu,
                            MA_BENHNHAN  = BNDangChonVaoKham.MA_BENHNHAN,
                            MANV         = maNVHienTai,
                            NGAYKHAM     = DateTime.Now,
                            TRANGTHAI    = "Hoàn thành"
                        });

                        // 2. PHIEUKHAM_DICHVU (snapshot giá)
                        foreach (var dv in DSDichVu.Where(x => x.IsSelected))
                        {
                            context.PHIEUKHAM_DICHVU.Add(new PHIEUKHAM_DICHVU
                            {
                                MA_PHIEUKHAM = maPhieu,
                                MA_DICHVU    = dv.MA_DICHVU,
                                SOLUONG      = 1,
                                GIA_LUC_DUNG = (decimal)dv.GiaTien
                            });
                        }

                        // 3. DONTHUOC (chỉ tạo nếu có thuốc)
                        if (DSThuocChon.Count > 0)
                        {
                            context.DONTHUOCs.Add(new DONTHUOC
                            {
                                MA_DONTHUOC  = maDonThuoc,
                                MA_PHIEUKHAM = maPhieu,
                                MANV_KE      = maNVHienTai,
                                TRANGTHAI    = "Chờ xuất"
                            });

                            // 4. CHITIET_DONTHUOC (snapshot giá thuốc)
                            foreach (var thuoc in DSThuocChon)
                            {
                                context.CHITIET_DONTHUOC.Add(new CHITIET_DONTHUOC
                                {
                                    MA_DONTHUOC = maDonThuoc,
                                    MA_THUOC    = thuoc.MA_THUOC,
                                    SOLUONG     = 1,
                                    GIA_LUC_BAN = thuoc.GIA_BAN
                                });
                            }
                        }

                        // 5. HOADON (tạo hóa đơn ngay)
                        decimal tongTienDV    = DSDichVu.Where(x => x.IsSelected).Sum(x => (decimal)x.GiaTien);
                        decimal tongTienThuoc = DSThuocChon.Sum(x => x.GIA_BAN);
                        context.HOADONs.Add(new HOADON
                        {
                            MA_HOADON    = "HD" + DateTime.Now.ToString("ddHHmm") + new Random().Next(10,99).ToString(),
                            MA_PHIEUKHAM = maPhieu,
                            MANV         = maNVHienTai,
                            TONGTIEN     = tongTienDV + tongTienThuoc,
                            TRANGTHAI    = "Chưa thanh toán"
                        });

                        context.SaveChanges();
                        MessageBox.Show("Lưu hồ sơ khám vào Database thành công!", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        var errorMessages = dbEx.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"Bảng {x.PropertyName}: Lỗi '{x.ErrorMessage}'");
                        string fullError = string.Join("\n", errorMessages);
                        MessageBox.Show("Lỗi Validation Database:\n" + fullError, "Lỗi Validation",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi Database: " + ex.Message + (ex.InnerException != null ? "\n" + ex.InnerException.Message : ""), "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Cập nhật UI
                DSLichSuKham.Insert(0, new KetQuaKham
                {
                    NgayKham     = DateTime.Now,
                    BN           = BNDangChonVaoKham,
                    TrieuChung   = TrieuChung,
                    ChuanDoan    = ChuanDoan,
                    DichVuDaDung = DSDichVu.Where(x => x.IsSelected).ToList()
                });
                DSBNChuaKham.Remove(BNDangChonVaoKham);
                ResetForm();
                SelectedTabIndex = 0;
            });

            LenhHuy = new RelayCommand(p =>
            {
                if (MessageBox.Show("Bạn có muốn xóa các thông tin đang nhập?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    ResetForm();
            });

            LenhXoaLichSu = new RelayCommand(p =>
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa hồ sơ này?", "Xác nhận",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var bn = SelectedLichSu.BN;
                    if (!DSBNChuaKham.Contains(bn)) DSBNChuaKham.Add(bn);
                    DSLichSuKham.Remove(SelectedLichSu);
                    IsSua = false;
                    IsVisibleDetail = Visibility.Collapsed;
                }
            }, p => SelectedLichSu != null);

            LenhSuaLichSu = new RelayCommand(p => { IsSua = true; },
                p => SelectedLichSu != null && !IsSua);

            LenhLuuLichSu = new RelayCommand(p =>
            {
                if (string.IsNullOrWhiteSpace(SelectedLichSu.TrieuChung) || string.IsNullOrWhiteSpace(SelectedLichSu.ChuanDoan))
                {
                    MessageBox.Show("Triệu chứng và Chẩn đoán không được để trống!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                SelectedLichSu.DichVuDaDung = DSDichVu.Where(x => x.IsSelected).ToList();
                MessageBox.Show("Cập nhật hồ sơ thành công!", "Thông báo");
                IsSua = false;
                LamMoiGiaoDien();
                ResetForm();
            }, p => SelectedLichSu != null && IsSua);
        }

        // ✅ LoadData(): Load DSDichVu từ bảng DICHVU, DSBNChuaKham từ bảng BENHNHAN
        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    // Load danh sách dịch vụ từ DB → map sang DichVuKhamLocal (có IsSelected)
                    var dsDichVu = db.DICHVUs.OrderBy(x => x.MA_DICHVU).ToList()
                        .Select(dv => new DichVuKhamLocal
                        {
                            MA_DICHVU  = dv.MA_DICHVU?.Trim(),
                            TenDichVu  = dv.TEN_DICHVU,
                            GiaTien    = (double)dv.GIA_DV,
                            IsSelected = false
                        }).ToList();
                    DSDichVu = new ObservableCollection<DichVuKhamLocal>(dsDichVu);

                    // Load danh sách tất cả bệnh nhân làm "chờ khám" ban đầu
                    var dsBN = db.BENHNHANs.OrderBy(x => x.MA_BENHNHAN).ToList();
                    DSBNChuaKham = new ObservableCollection<BENHNHAN>(dsBN);

                    // Load Lịch Sử Khám (Do DB không có cột Triệu chứng/Chẩn đoán nên để trống tạm)
                    var dsLS = db.PHIEUKHAMs.OrderByDescending(x => x.NGAYKHAM).ToList()
                        .Select(pk => new KetQuaKham
                        {
                            MaPhieuKham = pk.MA_PHIEUKHAM,
                            NgayKham = pk.NGAYKHAM ?? DateTime.Now,
                            BN = pk.BENHNHAN,
                            TrieuChung = "N/A (Chưa hỗ trợ lưu DB)",
                            ChuanDoan = "N/A (Chưa hỗ trợ lưu DB)",
                            DichVuDaDung = pk.PHIEUKHAM_DICHVU.Select(d => new DichVuKhamLocal { MA_DICHVU = d.MA_DICHVU, TenDichVu = d.DICHVU?.TEN_DICHVU, GiaTien = (double)d.GIA_LUC_DUNG, IsSelected = true }).ToList()
                        }).ToList();
                    DSLichSuKham = new ObservableCollection<KetQuaKham>(dsLS);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                DSDichVu     = new ObservableCollection<DichVuKhamLocal>();
                DSBNChuaKham = new ObservableCollection<BENHNHAN>();
            }
        }

        private void ResetForm()
        {
            TrieuChung = "";
            ChuanDoan  = "";
            BNDangChonVaoKham = null;
            IsSua = false;
            DSThuocChon?.Clear();
            if (DSDichVu != null)
                foreach (var dv in DSDichVu) dv.IsSelected = false;
        }

        public void LamMoiGiaoDien()
        {
            var temp = DSLichSuKham;
            DSLichSuKham = null;
            DSLichSuKham = temp;
            OnPropertyChanged(nameof(BNDangChonVaoKham));
        }
    }
}
