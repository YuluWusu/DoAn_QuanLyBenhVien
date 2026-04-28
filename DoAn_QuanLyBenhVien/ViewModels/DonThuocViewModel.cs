using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class ThuocGoc
    {
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DVT { get; set; }
        public decimal GiaBan { get; set; }
    }

    public class BacSi
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
    }

    public class DonThuocViewModel : BaseViewModel
    {
        public ObservableCollection<DonThuoc> DanhSachDonThuoc { get; set; }

        private ObservableCollection<ChiTietDonThuoc> _toanBoChiTietDon;

        private ObservableCollection<ChiTietDonThuoc> _danhSachChiTietDon_HienThi;
        public ObservableCollection<ChiTietDonThuoc> DanhSachChiTietDon_HienThi
        {
            get => _danhSachChiTietDon_HienThi;
            set { _danhSachChiTietDon_HienThi = value; OnPropertyChanged(); }
        }

        public List<ThuocGoc> DanhSachThuocGoc { get; set; }
        private ObservableCollection<BacSi> _danhSachBacSi;
        public ObservableCollection<BacSi> DanhSachBacSi
        {
            get => _danhSachBacSi;
            set
            {
                _danhSachBacSi = value;
                OnPropertyChanged();
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                if (_selectedTabIndex == 1)
                {
                    LọcChiTietDonThuoc();
                }
                if (!_isAdding && !_isEditing) ResetUIState();
            }
        }

        private string _addButtonContent = "➕ Thêm";
        public string AddButtonContent
        {
            get => _addButtonContent;
            set { _addButtonContent = value; OnPropertyChanged(); }
        }

        private bool _isSaveEnabled = false;
        public bool IsSaveEnabled
        {
            get => _isSaveEnabled;
            set { _isSaveEnabled = value; OnPropertyChanged(); }
        }

        private bool _isEditDeleteEnabled = false;
        public bool IsEditDeleteEnabled
        {
            get => _isEditDeleteEnabled;
            set { _isEditDeleteEnabled = value; OnPropertyChanged(); }
        }

        private bool _isAdding = false;
        private bool _isEditing = false;

        private bool _isProcessing = false;
        public bool IsProcessing
        {
            get => _isProcessing;
            set { _isProcessing = value; OnPropertyChanged(); }
        }

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
                    if (SelectedBacSi != bs)
                    {
                        SelectedBacSi = bs;
                    }
                }
                else
                {
                    SelectedBacSi = null;
                }
            }
        }

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
                    MaDonThuocMoi = _selectedDonThuoc.MaDonThuoc;
                    MaPhieuKhamMoi = _selectedDonThuoc.MaPhieuKham;
                    NgayKeMoi = _selectedDonThuoc.NgayKe;
                    MaNV_KeMoi = _selectedDonThuoc.MaNV_Ke;

                    SelectedBacSi = DanhSachBacSi.FirstOrDefault(x =>x.MaNV.Trim().ToUpper() == _selectedDonThuoc.MaNV_Ke?.Trim().ToUpper());

                    LọcChiTietDonThuoc();
                    IsEditDeleteEnabled = true;
                }
            }
        }

        private string _maThuocMoi;
        public string MaThuocMoi
        {
            get => _maThuocMoi;
            set
            {
                _maThuocMoi = value;
                OnPropertyChanged();
                AutoFillThuocInfo();
            }
        }

        private string _dvtMoi;
        public string DVTMoi { get => _dvtMoi; set { _dvtMoi = value; OnPropertyChanged(); } }

        private int _soLuongMoi;
        public int SoLuongMoi { get => _soLuongMoi; set { _soLuongMoi = value; OnPropertyChanged(); } }

        private decimal _giaBanMoi; 
        public decimal GiaBanMoi { get => _giaBanMoi; set { _giaBanMoi = value; OnPropertyChanged(); } }

        private string _lieuDungMoi;
        public string LieuDungMoi { get => _lieuDungMoi; set { _lieuDungMoi = value; OnPropertyChanged(); } }
        private BacSi _selectedBacSi;
        public BacSi SelectedBacSi
        {
            get => _selectedBacSi;
            set
            {
                if (_selectedBacSi == value) return;
                _selectedBacSi = value;
                OnPropertyChanged();

                if (_selectedBacSi != null)
                {
                    if (MaNV_KeMoi != _selectedBacSi.MaNV)
                    {
                        MaNV_KeMoi = _selectedBacSi.MaNV;
                    }
                }
            }
        }

        private ChiTietDonThuoc _selectedChiTiet;
        public ChiTietDonThuoc SelectedChiTiet
        {
            get => _selectedChiTiet;
            set
            {
                _selectedChiTiet = value;
                OnPropertyChanged();
                if (_selectedChiTiet != null)
                {
                    MaThuocMoi = _selectedChiTiet.MaThuoc;
                    DVTMoi = _selectedChiTiet.DVT;
                    SoLuongMoi = _selectedChiTiet.SoLuong;
                    LieuDungMoi = _selectedChiTiet.LieuDung;

                    IsEditDeleteEnabled = true;
                    IsSaveEnabled = false;
                    IsAddingEditingState(false);
                }
            }
        }
        private ThuocGoc _selectedThuocHienTai;
        public ThuocGoc SelectedThuocHienTai
        {
            get => _selectedThuocHienTai;
            set
            {
                _selectedThuocHienTai = value;
                OnPropertyChanged();

                if (_selectedThuocHienTai != null)
                {
                    MaThuocMoi = _selectedThuocHienTai.MaThuoc;
                    DVTMoi = _selectedThuocHienTai.DVT;
                    GiaBanMoi = _selectedThuocHienTai.GiaBan;
                }
            }
        }


        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ChiTietCommand { get; }

        public DonThuocViewModel()
        {
            AddCommand = new RelayCommand(ExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate, CanExecuteUpdateDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteUpdateDelete);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            ChiTietCommand = new RelayCommand(ExecuteChiTiet, CanExecuteChiTiet);

            LoadDummyData();
            SelectedDonThuoc = null;
            SelectedChiTiet = null;
            ClearFields();
            ResetUIState();
        }

        private void ExecuteAdd(object obj)
        {
            if (!_isAdding && !_isEditing)
            {
                IsAddingEditingState(true);
                IsSaveEnabled = true;
                IsEditDeleteEnabled = false;
                AddButtonContent = "❌ Hủy";
                ClearFields();
            }
            else
            {
                IsAddingEditingState(false);
                ResetUIState();
                ClearFields();
            }
        }

        private void ExecuteUpdate(object obj)
        {
            _isEditing = true;
            IsProcessing = true;
            IsSaveEnabled = true;
            IsEditDeleteEnabled = false;
            AddButtonContent = "❌ Hủy";
        }

        private void ExecuteDelete(object obj)
        {
            if (SelectedTabIndex == 0 && SelectedDonThuoc != null)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa dịch vụ này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DanhSachDonThuoc.Remove(SelectedDonThuoc);
                    ClearFields();
                    ResetUIState();
                }
            }
            else if (SelectedTabIndex == 1 && SelectedChiTiet != null)
            {
                _toanBoChiTietDon.Remove(SelectedChiTiet);
                LọcChiTietDonThuoc(); 
            }
            ClearFields();
            ResetUIState();
        }

        private bool CanExecuteSave(object obj)
        {
            if (!_isAdding && !_isEditing) return false;
            if (SelectedTabIndex == 0)
            {
                return !string.IsNullOrWhiteSpace(MaDonThuocMoi) && !string.IsNullOrWhiteSpace(MaPhieuKhamMoi);
            }
            else
            {
                return !string.IsNullOrWhiteSpace(MaThuocMoi) && SoLuongMoi > 0;
            }
        }

        private bool CanExecuteUpdateDelete(object obj)
        {
            if (IsProcessing) return false;

            if (SelectedTabIndex == 0) return SelectedDonThuoc != null;
            return SelectedChiTiet != null;
        }

        private bool CanExecuteChiTiet(object obj)
        {
            return SelectedDonThuoc != null && !IsProcessing;
        }

        private void ExecuteSave(object obj)
        {
            if (SelectedTabIndex == 0)
            {
                var bsThat = DanhSachBacSi.FirstOrDefault(x =>x.MaNV.Trim().ToUpper() == MaNV_KeMoi?.Trim().ToUpper());
                if (bsThat == null)
                {
                    MessageBox.Show("Mã nhân viên không tồn tại! Vui lòng kiểm tra lại.", "Thông báo lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; 
                }
                string tenHienThi = "Chưa xác định";

                if (SelectedBacSi is BacSi selectedBS)
                {
                    tenHienThi = selectedBS.HoTen;
                }
                else
                {
                    var bsFromList = DanhSachBacSi.FirstOrDefault(x => x.MaNV == MaNV_KeMoi);
                    if (bsFromList != null)
                    {
                        tenHienThi = bsFromList.HoTen;
                    }
                }
                if (_isAdding)
                {
                    DanhSachDonThuoc.Add(new DonThuoc
                    {
                        MaDonThuoc = MaDonThuocMoi,
                        MaPhieuKham = MaPhieuKhamMoi,
                        NgayKe = NgayKeMoi,
                        MaNV_Ke = MaNV_KeMoi,
                        TenBacSiKe = tenHienThi 
                    });
                }
                else if (_isEditing && SelectedDonThuoc != null)
                {
                    var index = DanhSachDonThuoc.IndexOf(SelectedDonThuoc);
                    if (index >= 0)
                    {
                        var updateDon = new DonThuoc
                        {
                            MaDonThuoc = MaDonThuocMoi, 
                            MaPhieuKham = MaPhieuKhamMoi,
                            NgayKe = NgayKeMoi,
                            MaNV_Ke = MaNV_KeMoi,
                            TenBacSiKe = tenHienThi
                        };

                        DanhSachDonThuoc[index] = updateDon;
                        SelectedDonThuoc = updateDon; 
                    }
                }
            }
            else if (SelectedTabIndex == 1)
            {
                decimal tongGia = GiaBanMoi * SoLuongMoi;
                var thuocInfo = DanhSachThuocGoc.FirstOrDefault(x => x.MaThuoc == MaThuocMoi);

                if (_isAdding && SelectedDonThuoc != null)
                {
                    _toanBoChiTietDon.Add(new ChiTietDonThuoc
                    {
                        MaDonThuoc = SelectedDonThuoc.MaDonThuoc,
                        MaThuoc = MaThuocMoi,
                        TenThuoc = thuocInfo?.TenThuoc,
                        DVT = DVTMoi,
                        SoLuong = SoLuongMoi,
                        GiaBan = tongGia, 
                        LieuDung = LieuDungMoi
                    });
                }
                else if (_isEditing && SelectedChiTiet != null)
                {
                    SelectedChiTiet.MaThuoc = MaThuocMoi;
                    SelectedChiTiet.TenThuoc = thuocInfo?.TenThuoc;
                    SelectedChiTiet.DVT = DVTMoi;
                    SelectedChiTiet.SoLuong = SoLuongMoi;
                    SelectedChiTiet.GiaBan = tongGia; 
                    SelectedChiTiet.LieuDung = LieuDungMoi;
                    var index = _toanBoChiTietDon.IndexOf(SelectedChiTiet);
                    if (index >= 0) _toanBoChiTietDon[index] = SelectedChiTiet;
                }
                LọcChiTietDonThuoc(); 
            }

            MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            IsAddingEditingState(false);
            ResetUIState();
            ClearFields();
        }

        private void ExecuteChiTiet(object obj)
        {
            if (SelectedDonThuoc != null)
            {
                SelectedTabIndex = 1;
                LọcChiTietDonThuoc();

                OnPropertyChanged(nameof(SelectedTabIndex));
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một đơn thuốc trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LọcChiTietDonThuoc()
        {
            if (SelectedDonThuoc != null && _toanBoChiTietDon != null)
            {
                var filtered = _toanBoChiTietDon.Where(x => x.MaDonThuoc == SelectedDonThuoc.MaDonThuoc).ToList();
                DanhSachChiTietDon_HienThi = new ObservableCollection<ChiTietDonThuoc>(filtered);
            }
            else
            {
                DanhSachChiTietDon_HienThi = new ObservableCollection<ChiTietDonThuoc>();
            }
        }

        private void AutoFillThuocInfo()
        {
            var thuoc = DanhSachThuocGoc?.FirstOrDefault(x => x.MaThuoc == MaThuocMoi);
            if (thuoc != null)
            {
                DVTMoi = thuoc.DVT;
                GiaBanMoi = thuoc.GiaBan;
            }
        }

        private void IsAddingEditingState(bool state)
        {
            _isAdding = state;
            _isEditing = false;
            IsProcessing = state;
            AddButtonContent = state ? "❌ Hủy" : "➕ Thêm";
        }

        private void ResetUIState()
        {
            AddButtonContent = "➕ Thêm";
            IsSaveEnabled = false;
            IsEditDeleteEnabled = (SelectedDonThuoc != null || SelectedChiTiet != null);
            IsProcessing = false;
            _isAdding = false;
            _isEditing = false;
        }

        private void ClearFields()
        {
            if (SelectedTabIndex == 0)
            {
                MaDonThuocMoi = string.Empty;
                MaPhieuKhamMoi = string.Empty;
                NgayKeMoi = DateTime.Now;
                MaNV_KeMoi = string.Empty;
                SelectedBacSi = null;
            }
            else
            {
                MaThuocMoi = string.Empty;
                SelectedThuocHienTai = null;
                DVTMoi = string.Empty;
                SoLuongMoi = 0;
                GiaBanMoi = 0;
                LieuDungMoi = string.Empty;
            }
        }

        private void LoadDummyData()
        {
            DanhSachThuocGoc = new List<ThuocGoc>
            {
                new ThuocGoc { MaThuoc = "T01", TenThuoc = "Paracetamol 500mg", DVT = "Viên", GiaBan = 2000 },
                new ThuocGoc { MaThuoc = "T02", TenThuoc = "Amoxicillin 250mg", DVT = "Viên", GiaBan = 5000 },
                new ThuocGoc { MaThuoc = "T03", TenThuoc = "Vitamin C", DVT = "Vỉ", GiaBan = 15000 }
            };

            DanhSachDonThuoc = new ObservableCollection<DonThuoc>
            {
                new DonThuoc { MaDonThuoc = "DT001", MaPhieuKham = "PK001", NgayKe = DateTime.Now, MaNV_Ke = "NV01", TenBacSiKe = "Nguyễn Văn A" }
            };

            _toanBoChiTietDon = new ObservableCollection<ChiTietDonThuoc>
            {
                new ChiTietDonThuoc { MaDonThuoc = "DT001", MaThuoc = "T01", TenThuoc = "Paracetamol 500mg", SoLuong = 10, DVT = "Viên", GiaBan = 20000, LieuDung = "Ngày 2 viên" }
            };

            DanhSachBacSi = new ObservableCollection<BacSi>
            {
                new BacSi { MaNV = "NV01", HoTen = "Nguyễn Văn A" },
                new BacSi { MaNV = "NV02", HoTen = "Trần Thị B" },
                new BacSi { MaNV = "NV03", HoTen = "Lê Văn C" }
            };
            if (DanhSachDonThuoc.Count > 0)
            {
                var firstDon = DanhSachDonThuoc[0];
                _selectedDonThuoc = firstDon;
                OnPropertyChanged(nameof(SelectedDonThuoc));

                SelectedBacSi = DanhSachBacSi.FirstOrDefault(x => x.MaNV == firstDon.MaNV_Ke);

                LọcChiTietDonThuoc();

            }
            
        }
    }
}