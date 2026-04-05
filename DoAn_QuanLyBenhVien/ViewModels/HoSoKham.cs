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
    public class KetQuaKham : BaseViewModel
    {
        public DateTime NgayKham { get; set; }
        public BenhNhan BN { get; set; }
        public string TrieuChung { get; set; }
        public string ChuanDoan { get; set; }
        public List<DichVu> DichVuDaDung { get; set; }
    }
    public class DichVu : BaseViewModel
    {
        private string _tenDichVu;
        public string TenDichVu { get => _tenDichVu; set { _tenDichVu = value; OnPropertyChanged(); } }

        private double _giaTien;
        public double GiaTien { get => _giaTien; set { _giaTien = value; OnPropertyChanged(); } }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
    }

    public class HoSoKham : BaseViewModel
    {
        private int _selectedTabIndex = 0;
        public int SelectedTabIndex { get => _selectedTabIndex; set { _selectedTabIndex = value; OnPropertyChanged(); } }

        private string _trieuChung;
        public string TrieuChung { get => _trieuChung; set { _trieuChung = value; OnPropertyChanged(); } }

        private string _chuanDoan;
        public string ChuanDoan { get => _chuanDoan; set { _chuanDoan = value; OnPropertyChanged(); } }

        public ObservableCollection<BenhNhan> DSBNChuaKham { get; set; }
        public ObservableCollection<DichVu> DSDichVu { get; set; }

        private ObservableCollection<KetQuaKham> _DSLichSuKham;
        public ObservableCollection<KetQuaKham> DSLichSuKham
        {
            get => _DSLichSuKham;
            set { _DSLichSuKham = value; OnPropertyChanged(); }
        }

        private Visibility _isVisibleDetail = Visibility.Collapsed;
        public Visibility IsVisibleDetail { get => _isVisibleDetail; set { _isVisibleDetail = value; OnPropertyChanged(); } }

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
                    foreach (var dvHeThong in DSDichVu)
                    {
                        dvHeThong.IsSelected = value.DichVuDaDung.Any(x => x.TenDichVu == dvHeThong.TenDichVu);
                    }
                }
            }
        }

        private string _thongTinMaBN;
        public string ThongTinMaBN { get => _thongTinMaBN; set { _thongTinMaBN = value; OnPropertyChanged(); } }

        private string _thongTinTenBN;
        public string ThongTinTenBN { get => _thongTinTenBN; set { _thongTinTenBN = value; OnPropertyChanged(); } }

        private string _thongTinGioiTinh;
        public string ThongTinGioiTinh { get => _thongTinGioiTinh; set { _thongTinGioiTinh = value; OnPropertyChanged(); } }

        private string _thongTinNgaySinh;
        public string ThongTinNgaySinh { get => _thongTinNgaySinh; set { _thongTinNgaySinh = value; OnPropertyChanged(); } }

        private string _thongTinTienSu;
        public string ThongTinTienSu { get => _thongTinTienSu; set { _thongTinTienSu = value; OnPropertyChanged(); } }

        private BenhNhan _bnDangChonVaoKham;
        public BenhNhan BNDangChonVaoKham
        {
            get => _bnDangChonVaoKham;
            set { 
                _bnDangChonVaoKham = value; OnPropertyChanged();
                if (value != null)
                {
                    ThongTinMaBN = value.MaBenhNhan;
                    ThongTinTenBN = value.TenBenhNhan;
                    ThongTinGioiTinh = value.GioiTinh;
                    ThongTinNgaySinh = value.NgaySinh.HasValue? value.NgaySinh.Value.ToString("dd/MM/yyyy"): "";
                    ThongTinTienSu = value.TienSuBenhLy;
                }
                else
                {
                    ThongTinMaBN = "";
                    ThongTinTenBN = "";
                    ThongTinGioiTinh = "";
                    ThongTinNgaySinh = "";
                    ThongTinTienSu = "";
                }
            }
        }

        public ICommand LenhLuu { get; set; }
        public ICommand LenhHuy { get; set; }
        public ICommand LenhSuaLichSu { get; set; }
        public ICommand LenhXoaLichSu { get; set; }
        public ICommand LenhLuuLichSu { get; set; }

        public HoSoKham()
        {
            DSLichSuKham = new ObservableCollection<KetQuaKham>();

            DSBNChuaKham = new ObservableCollection<BenhNhan>();

            DSDichVu = new ObservableCollection<DichVu>() {
                new DichVu { TenDichVu="Siêu âm tổng quát", GiaTien=200000 },
                new DichVu { TenDichVu="Xét nghiệm máu tổng hợp", GiaTien=350000 },
                new DichVu { TenDichVu="Chụp X-Quang phổi", GiaTien=150000 },
                new DichVu { TenDichVu="Nội soi dạ dày", GiaTien=600000 }
            };

            LenhLuu = new RelayCommand((p) => {
                if (BNDangChonVaoKham == null)
                {
                    MessageBox.Show("Vui lòng chọn một bệnh nhân từ danh sách chờ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool coDichVu = DSDichVu.Any(x => x.IsSelected);
                if (string.IsNullOrWhiteSpace(TrieuChung) || string.IsNullOrWhiteSpace(ChuanDoan) || !coDichVu)
                {
                    MessageBox.Show("BẮT BUỘC: Bạn phải nhập Triệu chứng, Chẩn đoán và chọn ít nhất 1 dịch vụ!",
                                    "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                KetQuaKham thongTinKhamMoi = new KetQuaKham
                {
                    NgayKham = DateTime.Now,
                    BN = BNDangChonVaoKham, 
                    TrieuChung = TrieuChung,
                    ChuanDoan = ChuanDoan,
                    DichVuDaDung = DSDichVu.Where(x => x.IsSelected).ToList()
                };
                DSLichSuKham.Insert(0, thongTinKhamMoi);
                DSBNChuaKham.Remove(BNDangChonVaoKham); 
                MessageBox.Show("Lưu hồ sơ khám thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                SelectedTabIndex = 0; 
            });

            LenhHuy = new RelayCommand((p) => {
                if (MessageBox.Show("Bạn có muốn xóa các thông tin đang nhập?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ResetForm();
                }
            });

            LenhXoaLichSu = new RelayCommand((p) => {
                if (MessageBox.Show("Bạn có chắc muốn xóa hồ sơ này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var bnCanTraLai = SelectedLichSu.BN;
                    if (!DSBNChuaKham.Contains(bnCanTraLai))
                    {
                        DSBNChuaKham.Add(bnCanTraLai); 
                    }
                    DSLichSuKham.Remove(SelectedLichSu);
                    IsSua = false;
                    IsVisibleDetail = Visibility.Collapsed;
                }
            }, (p) => SelectedLichSu != null);

            LenhSuaLichSu = new RelayCommand((p) => {
                IsSua = true; 
            }, (p) => SelectedLichSu != null && !IsSua);

            LenhLuuLichSu = new RelayCommand((p) => { 
                if (string.IsNullOrWhiteSpace(SelectedLichSu.TrieuChung) || string.IsNullOrWhiteSpace(SelectedLichSu.ChuanDoan))
                {
                    MessageBox.Show("Triệu chứng và Chẩn đoán không được để trống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                SelectedLichSu.DichVuDaDung = DSDichVu.Where(x => x.IsSelected).ToList();

                MessageBox.Show("Cập nhật hồ sơ thành công!", "Thông báo");
                IsSua = false;
                LamMoiGiaoDien();
                ResetForm();
            }, (p) => SelectedLichSu != null && IsSua);
        }

        private void ResetForm()
        {
            TrieuChung = "";
            ChuanDoan = "";
            BNDangChonVaoKham = null;
            IsSua = false;
            if (DSDichVu != null)
            {
                foreach (var dv in DSDichVu)
                {
                    dv.IsSelected = false;
                }
            }
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
