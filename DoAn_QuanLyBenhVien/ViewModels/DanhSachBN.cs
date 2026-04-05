using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DanhSachBN : BaseViewModel
    {
        private ObservableCollection<BenhNhan> _DSBN;
        public ObservableCollection<BenhNhan> DSBN
        {  
            get { return _DSBN; } 
            set { _DSBN = value; OnPropertyChanged(); }
        }

        public Action<BenhNhan> YeuCauChuyenTrangHoSo { get; set; }

        private BenhNhan _bnDangChon;
        public BenhNhan BNDangChon
        {
            get => _bnDangChon;
            set { _bnDangChon = value; OnPropertyChanged(); }
        }

        private BenhNhan _bnDuocChon;
        public BenhNhan BNDuocChon
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
        public bool IsSua
        {
            get => _isSua;
            set
            {
                _isSua = value;
                OnPropertyChanged();                
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _coChon = false;
        public bool CoChon
        {
            get => _coChon;
            set
            {
                _coChon = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _ngaySinh;
        public string NgaySinh
        {
            get => _ngaySinh;
            set
            {
                if (DateTime.TryParseExact(value, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
                {
                    _ngaySinh = value;
                }
                else
                {
                    _ngaySinh = value;
                }
                OnPropertyChanged();
            }
        }

        private string _tenNutThem = "Thêm Mới";
        public string TenNutThem { get => _tenNutThem; set { _tenNutThem = value; OnPropertyChanged(); } }

        public ICommand LenhThem { get; set; }
        public ICommand LenhLuu { get; set; }
        public ICommand LenhSua { get; set; }
        public ICommand LenhXoa { get; set; }
        public ICommand LenhThemPK { get; set; }

        public DanhSachBN()
        {
            DSBN = new ObservableCollection<BenhNhan>();
            BNDangChon = new BenhNhan { GioiTinh = "Nam" };
            BNDangChon = new BenhNhan();
            TaiDL();

            LenhThem = new RelayCommand(p => {
                if (TenNutThem == "Thêm Mới")
                {
                    IsSua = true;
                    TenNutThem = "Hủy Bỏ";
                    BNDangChon = new BenhNhan { NgayTaoHoSo = DateTime.Now, GioiTinh = "Nam" };
                    BNDuocChon = null;
                    CoChon = false;
                }
                else 
                {
                    IsSua = false;
                    CoChon = false;
                    TenNutThem = "Thêm Mới";
                    BNDuocChon = null;
                    BNDangChon = new BenhNhan { GioiTinh = "Nam" };
                }
                OnPropertyChanged(nameof(IsNam));
                OnPropertyChanged(nameof(IsNu));
                OnPropertyChanged(nameof(IsKhac));
            });

            LenhSua = new RelayCommand(p => {
                IsSua = true;
                TenNutThem = "Hủy Bỏ";
                MessageBox.Show($"Bạn có chắc chắn muốn sửa hồ sơ bệnh nhân {BNDangChon.TenBenhNhan} không?", "Xác nhận sửa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }, p => CoChon);

            LenhXoa = new RelayCommand(p => {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa hồ sơ: {BNDangChon.TenBenhNhan}?", "Xác nhận", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    DSBN.Remove(BNDangChon);

                    // Reset giao diện
                    IsSua = false;
                    CoChon = false;
                    BNDuocChon = null;
                    BNDangChon = new BenhNhan { GioiTinh = "Nam" };

                    OnPropertyChanged(nameof(IsNam));
                    OnPropertyChanged(nameof(IsNu));
                    OnPropertyChanged(nameof(IsKhac));
                    MessageBox.Show("Đã xóa hồ sơ thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }, p => CoChon);

            LenhLuu = new RelayCommand(p =>
            {
                if (!CheckDL())
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin: Mã BN, Tên và SĐT (đúng 10 số)!, Ngày sinh đúng định dạng đ/mm/yyyy",
                                    "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DSBN.Contains(BNDangChon))
                {
                    DSBN.Add(BNDangChon);
                }

                IsSua = false;
                CoChon = false;
                TenNutThem = "Thêm Mới";
                BNDuocChon = null; 
                BNDangChon = new BenhNhan { GioiTinh = "Nam" };

                OnPropertyChanged(nameof(IsNam));
                OnPropertyChanged(nameof(IsNu));
                OnPropertyChanged(nameof(IsKhac));

                MessageBox.Show("Đã lưu dữ liệu bệnh nhân thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }, p => IsSua);

            LenhThemPK = new RelayCommand(p => {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn thêm hồ sơ khám cho bệnh nhân: {BNDangChon.TenBenhNhan}?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (BNDangChon != null)
                    {
                        YeuCauChuyenTrangHoSo?.Invoke(BNDangChon);
                    }
                }
            }, p => CoChon);
        }

        private bool CheckDL()
        {
            return BNDangChon != null &&
                   !string.IsNullOrWhiteSpace(BNDangChon.MaBenhNhan) &&
                   !string.IsNullOrWhiteSpace(BNDangChon.TenBenhNhan) &&
                   !string.IsNullOrWhiteSpace(BNDangChon.GioiTinh) &&
                   !string.IsNullOrWhiteSpace(BNDangChon.SoDienThoai) &&
                   BNDangChon.NgaySinh != DateTime.MinValue && 
                   BNDangChon.SoDienThoai.Length == 10 &&
                   BNDangChon.SoDienThoai.All(char.IsDigit);
        }

        public bool IsNam
        {
            get => BNDangChon?.GioiTinh == "Nam";
            set
            {
                if (value && BNDangChon != null)
                {
                    BNDangChon.GioiTinh = "Nam";
                    OnPropertyChanged(nameof(IsNam));
                    OnPropertyChanged(nameof(IsNu));
                    OnPropertyChanged(nameof(IsKhac));
                }
            }
        }

        public bool IsNu
        {
            get => BNDangChon?.GioiTinh == "Nữ";
            set
            {
                if (value && BNDangChon != null)
                {
                    BNDangChon.GioiTinh = "Nữ";
                    OnPropertyChanged(nameof(IsNam));
                    OnPropertyChanged(nameof(IsNu));
                    OnPropertyChanged(nameof(IsKhac));
                }
            }
        }

        public bool IsKhac
        {
            get => BNDangChon?.GioiTinh == "Khác";
            set
            {
                if (value && BNDangChon != null)
                {
                    BNDangChon.GioiTinh = "Khác";
                    OnPropertyChanged(nameof(IsNam));
                    OnPropertyChanged(nameof(IsNu));
                    OnPropertyChanged(nameof(IsKhac));
                }
            }
        }

        private void TaiDL()
        {
            DSBN.Clear();
            DSBN.Add(new BenhNhan
            {
                MaBenhNhan = "BN001",
                TenBenhNhan = "Nguyễn Văn Anh",
                NgaySinh = new DateTime(1995, 5, 20),
                GioiTinh = "Nam",
                SoDienThoai = "0901234567",
                DiaChi = "123 Đường Lê Lợi, Quận 1, TP.HCM",
                TienSuBenhLy = "Viêm xoang mãn tính",
                NgayTaoHoSo = DateTime.Now.AddDays(-10)
            });
            DSBN.Add(new BenhNhan
            {
                MaBenhNhan = "BN002",
                TenBenhNhan = "Trần Thị Bình",
                NgaySinh = new DateTime(1990, 5, 12),
                GioiTinh = "Nữ",
                SoDienThoai = "0987654321",
                DiaChi = "456 Đường Hùng Vương, Bình Tân, TP.HCM",
                TienSuBenhLy = "Dị ứng phấn hoa",
                NgayTaoHoSo = DateTime.Now
            });

        } 
    }
}
