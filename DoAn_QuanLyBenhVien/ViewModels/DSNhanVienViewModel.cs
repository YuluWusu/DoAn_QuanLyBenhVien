using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DoAn_QuanLyBenhVien.Models;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DSNhanVienViewModel:BaseViewModel
    {
        // 1. DANH SÁCH DỮ LIỆU
        private ObservableCollection<NhanVien> _listNhanVien;
        public ObservableCollection<NhanVien> ListNhanVien
        {
            get => _listNhanVien;
            set { _listNhanVien = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ChuyenKhoa> ListChuyenKhoa { get; set; } = new ObservableCollection<ChuyenKhoa>();
        private List<NhanVien> _originalData = new List<NhanVien>();

        // 2. CÁC BIẾN NHẬP LIỆU (INPUT)
        private string _maNV;
        public string MaNV { get => _maNV; set { _maNV = value; OnPropertyChanged(); } }

        private string _hoTen;
        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }

        private DateTime? _ngaySinh;
        public DateTime? NgaySinh { get => _ngaySinh; set { _ngaySinh = value; OnPropertyChanged(); } }

        private string _gioiTinh;
        public string GioiTinh { get => _gioiTinh; set { _gioiTinh = value; OnPropertyChanged(); } }

        private string _chucVu;
        public string ChucVu { get => _chucVu; set { _chucVu = value; OnPropertyChanged(); } }

        private string _soDienThoai;
        public string SoDienThoai { get => _soDienThoai; set { _soDienThoai = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _maCK;
        public string MaCK { get => _maCK; set { _maCK = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Tự động lọc khi xóa hết chữ trong ô tìm kiếm
                if (string.IsNullOrEmpty(value)) ExecuteSearch();
            }
        }

        private ChuyenKhoa _selectedChuyenKhoa;
        public ChuyenKhoa SelectedChuyenKhoa
        {
            get => _selectedChuyenKhoa;
            set
            {
                _selectedChuyenKhoa = value;
                OnPropertyChanged();
                if (SelectedChuyenKhoa != null) MaCK = SelectedChuyenKhoa.MaCK;
            }
        }

        // 3. ĐỐI TƯỢNG ĐANG CHỌN TRÊN GRID
        private NhanVien _selectedItem;
        public NhanVien SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                // Nếu đang chọn một dòng và KHÔNG ở chế độ Thêm/Sửa thì mới đổ dữ liệu ra Input
                if (_selectedItem != null && !IsAdding && !IsEditing)
                {
                    MaNV = _selectedItem.MaNV;
                    HoTen = _selectedItem.HoTen;
                    NgaySinh = _selectedItem.NgaySinh;
                    GioiTinh = _selectedItem.GioiTinh;
                    ChucVu = _selectedItem.ChucVu;
                    SoDienThoai = _selectedItem.SoDienThoai;
                    Email = _selectedItem.Email;
                    SelectedChuyenKhoa = ListChuyenKhoa.FirstOrDefault(x => x.MaCK == _selectedItem.MaCK);
                }
                // Cập nhật trạng thái nhấn của các nút
                SuaCommand.RaiseCanExecuteChanged();
                XoaCommand.RaiseCanExecuteChanged();
            }
        }

        // 4. QUẢN LÝ TRẠNG THÁI UI (Giống logic quản lý tài khoản)
        private bool _isAdding;
        public bool IsAdding { get => _isAdding; set { _isAdding = value; OnPropertyChanged(); UpdateUI(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); UpdateUI(); } }

        public bool IsInputEnabled => IsAdding || IsEditing;
        public string AddButtonText => IsAdding ? "Hủy" : "Thêm";
        public string EditButtonText => IsEditing ? "Hủy Sửa" : "Sửa";

        // 5. COMMANDS
        public RelayCommand ThemCommand { get; }
        public RelayCommand SuaCommand { get; }
        public RelayCommand XoaCommand { get; }
        public RelayCommand LuuCommand { get; }
        public RelayCommand SearchCommand { get; }

        public DSNhanVienViewModel()
        {
            LoadData();

            // Khởi tạo các lệnh
            ThemCommand = new RelayCommand(p => AddOrCancel(), p => !IsEditing);
            SuaCommand = new RelayCommand(p => EditOrCancel(), p => SelectedItem != null && !IsAdding);
            LuuCommand = new RelayCommand(p => Save(), p => IsAdding || IsEditing);
            XoaCommand = new RelayCommand(p => Delete(), p => SelectedItem != null && !IsAdding && !IsEditing);
            SearchCommand = new RelayCommand(p => ExecuteSearch());
        }

        private void AddOrCancel()
        {
            if (IsAdding) { ResetState(); return; }
            ClearForm();
            IsAdding = true;
        }

        private void EditOrCancel()
        {
            if (IsEditing) { ResetState(); return; }
            IsEditing = true;
        }
        private bool IsValidInput()
        {
            // 1. Kiểm tra để trống các trường bắt buộc
            if (string.IsNullOrWhiteSpace(MaNV) || string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(SoDienThoai))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ các thông tin bắt buộc: Mã, Họ tên và SĐT!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 2. Kiểm tra số điện thoại (Phải là 10 số, bắt đầu bằng số 0)
            // Châu dùng Regex để kiểm tra chuỗi số
            string sdtPattern = @"^0\d{9}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(SoDienThoai.Trim(), sdtPattern))
            {
                MessageBox.Show("Số điện thoại không hợp lệ! (Phải đủ 10 số và bắt đầu bằng số 0)", "Lỗi định dạng", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // 3. Kiểm tra Email (nếu người dùng có nhập thì phải đúng định dạng)
            if (!string.IsNullOrWhiteSpace(Email))
            {
                string emailPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(Email.Trim(), emailPattern))
                {
                    MessageBox.Show("Email không đúng định dạng!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }
        private void Save()
        {
            if (!IsValidInput()) return;

            if (IsAdding)
            {
                if (_originalData.Any(x => x.MaNV == MaNV)) { MessageBox.Show("Mã nhân viên này đã tồn tại!"); return; }

                var newNV = new NhanVien
                {
                    MaNV = MaNV,
                    HoTen = HoTen,
                    NgaySinh = NgaySinh,
                    GioiTinh = GioiTinh,
                    ChucVu = ChucVu,
                    SoDienThoai = SoDienThoai,
                    Email = Email,
                    MaCK = MaCK,
                    TenChuyenKhoa = SelectedChuyenKhoa?.TenChuyenKhoa
                };

                _originalData.Add(newNV); // Thêm vào gốc
                ExecuteSearch(); // Cập nhật lại hiển thị
                MessageBox.Show("Thêm nhân viên thành công!");
            }
            else // Sửa
            {
                var nv = _originalData.FirstOrDefault(x => x.MaNV == SelectedItem.MaNV);
                if (nv != null)
                {
                    nv.HoTen = HoTen;
                    nv.NgaySinh = NgaySinh;
                    nv.GioiTinh = GioiTinh;
                    nv.ChucVu = ChucVu;
                    nv.SoDienThoai = SoDienThoai;
                    nv.Email = Email;
                    nv.MaCK = MaCK;
                    nv.TenChuyenKhoa = SelectedChuyenKhoa?.TenChuyenKhoa;

                    ExecuteSearch();
                    MessageBox.Show("Cập nhật thành công!");
                }
            }
            ResetState();
        }

        private void Delete()
        {
            if (MessageBox.Show("Xác nhận xóa?", "Thông báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _originalData.Remove(SelectedItem); // Xóa trong gốc
                ExecuteSearch(); // Cập nhật hiển thị
                ClearForm();
            }
        }

        private void ResetState()
        {
            IsAdding = IsEditing = false;
            if (SelectedItem != null)
            {
                MaNV = SelectedItem.MaNV;
                HoTen = SelectedItem.HoTen;
                NgaySinh = SelectedItem.NgaySinh;
                GioiTinh = SelectedItem.GioiTinh;
                ChucVu = SelectedItem.ChucVu;
                SoDienThoai = SelectedItem.SoDienThoai;
                Email = SelectedItem.Email;
                SelectedChuyenKhoa = ListChuyenKhoa.FirstOrDefault(x => x.MaCK == SelectedItem.MaCK);
            }
            else ClearForm();
        }

        private void ClearForm()
        {
            MaNV = HoTen = ChucVu = SoDienThoai = Email = MaCK = "";
            NgaySinh = DateTime.Now;
            GioiTinh = "Nam";
            SelectedChuyenKhoa = null;
        }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(AddButtonText));
            OnPropertyChanged(nameof(EditButtonText));
            OnPropertyChanged(nameof(IsInputEnabled));
            LuuCommand.RaiseCanExecuteChanged();
        }

        void LoadData()
        {
            ListChuyenKhoa = new ObservableCollection<ChuyenKhoa>();
            _originalData = new List<NhanVien>(); // Khởi tạo list gốc
            ListNhanVien = new ObservableCollection<NhanVien>(_originalData);
        }
        private void ExecuteSearch()
        {
            // Nếu ô tìm kiếm trống -> Hiện toàn bộ từ gốc
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ListNhanVien = new ObservableCollection<NhanVien>(_originalData);
            }
            else
            {
                var filtered = _originalData.Where(x =>
                    x.HoTen.ToLower().Contains(SearchText.ToLower()) ||
                    x.MaNV.ToLower().Contains(SearchText.ToLower())
                ).ToList();

                ListNhanVien = new ObservableCollection<NhanVien>(filtered);
            }
        }
    }
}
