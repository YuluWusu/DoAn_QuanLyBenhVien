using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DanhSachBNViewModel : BaseViewModel
    {
        private ObservableCollection<BENHNHAN> _DSBN;
        public ObservableCollection<BENHNHAN> DSBN
        {  
            get { return _DSBN; } 
            set { _DSBN = value; OnPropertyChanged(); }
        }

        public Action<BENHNHAN> YeuCauChuyenTrangHoSo { get; set; }

        private BENHNHAN _bnDangChon;
        public BENHNHAN BNDangChon
        {
            get => _bnDangChon;
            set { _bnDangChon = value; OnPropertyChanged(); }
        }

        private BENHNHAN _bnDuocChon;
        public BENHNHAN BNDuocChon
        {
            get => _bnDuocChon;
            set
            {
                _bnDuocChon = value;
                OnPropertyChanged();
                if (value != null)
                {
                    BNDangChon = value;
                    CoChon = true;
                    IsSua = false;
                }
                OnPropertyChanged(nameof(IsNam));
                OnPropertyChanged(nameof(IsNu));
                OnPropertyChanged(nameof(IsKhac));
            }
        }

        private bool _isSua = false;
        public bool IsSua { get => _isSua; set { _isSua = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }

        private bool _coChon = false;
        public bool CoChon { get => _coChon; set { _coChon = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }

        private string _tenNutThem = "Thêm Mới";
        public string TenNutThem { get => _tenNutThem; set { _tenNutThem = value; OnPropertyChanged(); } }

        public ICommand LenhThem { get; set; }
        public ICommand LenhLuu { get; set; }
        public ICommand LenhSua { get; set; }
        public ICommand LenhXoa { get; set; }
        public ICommand LenhThemPK { get; set; }

        public DanhSachBNViewModel()
        {
            DSBN = new ObservableCollection<BENHNHAN>();
            BNDangChon = new BENHNHAN { GIOITINH = "Nam" };
            TaiDL();

            LenhThem = new RelayCommand(p => {
                if (TenNutThem == "Thêm Mới")
                {
                    IsSua = true; TenNutThem = "Hủy Bỏ"; 
                    BNDangChon = new BENHNHAN { GIOITINH = "Nam" }; 
                    BNDuocChon = null; CoChon = false;
                    
                    // Auto-generate ID
                    try
                    {
                        using (var db = new QL_PHONG_KHAM())
                        {
                            BNDangChon.MA_BENHNHAN = IdGenerator.GetNextMaBenhNhan(db);
                            OnPropertyChanged(nameof(BNDangChon));
                        }
                    } catch { }
                }
                else 
                {
                    IsSua = false; CoChon = false; TenNutThem = "Thêm Mới"; BNDuocChon = null; BNDangChon = new BENHNHAN { GIOITINH = "Nam" };
                }
                OnPropertyChanged(nameof(IsNam)); OnPropertyChanged(nameof(IsNu)); OnPropertyChanged(nameof(IsKhac));
            }, p => !IsSua || TenNutThem == "Hủy Bỏ");

            LenhSua = new RelayCommand(p => { IsSua = true; TenNutThem = "Hủy Bỏ"; }, p => CoChon && !IsSua);

            LenhXoa = new RelayCommand(p => {
                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa hồ sơ: {BNDangChon.TEN_BENHNHAN}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        
                        using (var db = new QL_PHONG_KHAM())
                        {
                            var bn = db.BENHNHANs.Find(BNDangChon.MA_BENHNHAN);
                            if (bn != null)
                            {
                                db.BENHNHANs.Remove(bn);
                                db.SaveChanges();
                            }
                        }
                        DSBN.Remove(BNDangChon);
                        IsSua = false; CoChon = false; BNDuocChon = null; BNDangChon = new BENHNHAN { GIOITINH = "Nam" };
                        OnPropertyChanged(nameof(IsNam)); OnPropertyChanged(nameof(IsNu)); OnPropertyChanged(nameof(IsKhac));
                        MessageBox.Show("Đã xóa hồ sơ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }, p => CoChon && !IsSua);

            LenhLuu = new RelayCommand(p =>
            {
                if (!CheckDL()) { MessageBox.Show("Vui lòng nhập đầy đủ thông tin: Mã BN, Tên và Ngày sinh!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        var existing = db.BENHNHANs.Find(BNDangChon.MA_BENHNHAN);
                        if (existing == null)
                        {
                            // Kiểm tra trùng tên (Cảnh báo mềm)
                            if (db.BENHNHANs.Any(x => x.TEN_BENHNHAN.ToLower() == BNDangChon.TEN_BENHNHAN.ToLower()))
                            {
                                if (MessageBox.Show("Đã tồn tại bệnh nhân có cùng tên. Bạn có chắc muốn tiếp tục thêm?", "Cảnh báo trùng lặp", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                {
                                    return;
                                }
                            }

                            // ✅ Thêm mới vào Database
                            db.BENHNHANs.Add(new BENHNHAN
                            {
                                MA_BENHNHAN  = BNDangChon.MA_BENHNHAN?.Trim(),
                                TEN_BENHNHAN = BNDangChon.TEN_BENHNHAN,
                                NGAYSINH     = BNDangChon.NGAYSINH,
                                GIOITINH     = BNDangChon.GIOITINH
                            });
                            db.SaveChanges();

                            if (!DSBN.Contains(BNDangChon)) DSBN.Add(BNDangChon);
                        }
                        else
                        {
                            // ✅ Cập nhật vào Database
                            existing.TEN_BENHNHAN = BNDangChon.TEN_BENHNHAN;
                            existing.NGAYSINH     = BNDangChon.NGAYSINH;
                            existing.GIOITINH     = BNDangChon.GIOITINH;
                            db.Entry(existing).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    IsSua = false; CoChon = false; TenNutThem = "Thêm Mới"; BNDuocChon = null; BNDangChon = new BENHNHAN { GIOITINH = "Nam" };
                    OnPropertyChanged(nameof(IsNam)); OnPropertyChanged(nameof(IsNu)); OnPropertyChanged(nameof(IsKhac));
                    MessageBox.Show("Đã lưu dữ liệu bệnh nhân thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, p => IsSua);

            LenhThemPK = new RelayCommand(p => {
                if (MessageBox.Show($"Bạn có chắc chắn muốn thêm hồ sơ khám cho bệnh nhân: {BNDangChon.TEN_BENHNHAN}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (BNDangChon != null) YeuCauChuyenTrangHoSo?.Invoke(BNDangChon);
                }
            }, p => CoChon && !IsSua);
        }

        private bool CheckDL()
        {
            return BNDangChon != null && !string.IsNullOrWhiteSpace(BNDangChon.MA_BENHNHAN) &&
                   !string.IsNullOrWhiteSpace(BNDangChon.TEN_BENHNHAN) && !string.IsNullOrWhiteSpace(BNDangChon.GIOITINH);
        }

        public bool IsNam
        {
            get => BNDangChon?.GIOITINH == "Nam";
            set { if (value && BNDangChon != null) { BNDangChon.GIOITINH = "Nam"; OnPropertyChanged(nameof(IsNam)); OnPropertyChanged(nameof(IsNu)); OnPropertyChanged(nameof(IsKhac)); } }
        }

        public bool IsNu
        {
            get => BNDangChon?.GIOITINH == "Nữ";
            set { if (value && BNDangChon != null) { BNDangChon.GIOITINH = "Nữ"; OnPropertyChanged(nameof(IsNam)); OnPropertyChanged(nameof(IsNu)); OnPropertyChanged(nameof(IsKhac)); } }
        }

        public bool IsKhac
        {
            get => BNDangChon?.GIOITINH == "Khác";
            set { if (value && BNDangChon != null) { BNDangChon.GIOITINH = "Khác"; OnPropertyChanged(nameof(IsNam)); OnPropertyChanged(nameof(IsNu)); OnPropertyChanged(nameof(IsKhac)); } }
        }

        // ✅ TaiDL(): Xóa mock data, load từ Database qua EF
        private void TaiDL()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    var list = db.BENHNHANs.OrderBy(x => x.MA_BENHNHAN).ToList();
                    DSBN = new ObservableCollection<BENHNHAN>(list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                DSBN = new ObservableCollection<BENHNHAN>();
            }
        }
    }
}
