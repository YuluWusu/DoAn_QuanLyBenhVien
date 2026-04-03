using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DoAn_QuanLyBenhVien.Helper;
using DoAn_QuanLyBenhVien.Models;
namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DanhSachThuocViewModel:BaseViewModel
    {
        // 1. DANH SÁCH DỮ LIỆU
        private ObservableCollection<Thuoc> _listThuoc;
        public ObservableCollection<Thuoc> ListThuoc
        {
            get => _listThuoc;
            set { _listThuoc = value; OnPropertyChanged(); }
        }
        private List<Thuoc> _originalData = new List<Thuoc>();

        // 2. CÁC BIẾN NHẬP LIỆU (INPUT)
        private string _maThuoc;
        public string MaThuoc { get => _maThuoc; set { _maThuoc = value; OnPropertyChanged(); } }

        private string _tenThuoc;
        public string TenThuoc { get => _tenThuoc; set { _tenThuoc = value; OnPropertyChanged(); } }

        private string _dvt;
        public string DVT { get => _dvt; set { _dvt = value; OnPropertyChanged(); } }

        private decimal? _giaNhap;
        public decimal? GiaNhap { get => _giaNhap; set { _giaNhap = value; OnPropertyChanged(); } }

        private decimal? _giaBan;
        public decimal? GiaBan { get => _giaBan; set { _giaBan = value; OnPropertyChanged(); } }

        private DateTime? _hanSD;
        public DateTime? HanSD { get => _hanSD; set { _hanSD = value; OnPropertyChanged(); } }

        private string _huongDan;
        public string HuongDan { get => _huongDan; set { _huongDan = value; OnPropertyChanged(); } }

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

        // 3. ĐỐI TƯỢNG ĐANG CHỌN TRÊN GRID
        private Thuoc _selectedItem;
        public Thuoc SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                // Đổ dữ liệu ra Input nếu không ở chế độ Thêm/Sửa
                if (_selectedItem != null && !IsAdding && !IsEditing)
                {
                    MaThuoc = _selectedItem.MaThuoc;
                    TenThuoc = _selectedItem.TenThuoc;
                    DVT = _selectedItem.DVT;
                    GiaNhap = _selectedItem.GiaNhap;
                    GiaBan = _selectedItem.GiaBan;
                    HanSD = _selectedItem.HanSD;
                    HuongDan = _selectedItem.HuongDan;
                }
                SuaCommand.RaiseCanExecuteChanged();
                XoaCommand.RaiseCanExecuteChanged();
            }
        }

        // 4. QUẢN LÝ TRẠNG THÁI UI
        private bool _isAdding;
        public bool IsAdding { get => _isAdding; set { _isAdding = value; OnPropertyChanged(); UpdateUI(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); UpdateUI(); } }

        public bool IsInputEnabled => IsAdding || IsEditing;
        public string AddButtonText => IsAdding ? "Hủy" : "Thêm mới";
        public string EditButtonText => IsEditing ? "Hủy sửa" : "Cập nhật";

        // 5. COMMANDS
        public RelayCommand ThemCommand { get; }
        public RelayCommand SuaCommand { get; }
        public RelayCommand XoaCommand { get; }
        public RelayCommand LuuCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearCommand { get; }

        public DanhSachThuocViewModel()
        {
            LoadData();

            ThemCommand = new RelayCommand(p => AddOrCancel(), p => !IsEditing);
            SuaCommand = new RelayCommand(p => EditOrCancel(), p => SelectedItem != null && !IsAdding);
            LuuCommand = new RelayCommand(p => Save(), p => IsAdding || IsEditing);
            XoaCommand = new RelayCommand(p => Delete(), p => SelectedItem != null && !IsAdding && !IsEditing);
            SearchCommand = new RelayCommand(p => ExecuteSearch());
            ClearCommand = new RelayCommand(p => { ClearForm(); ResetState(); });
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
            if (string.IsNullOrWhiteSpace(MaThuoc) || string.IsNullOrWhiteSpace(TenThuoc))
            {
                MessageBox.Show("Vui lòng nhập Mã và Tên thuốc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (GiaBan < GiaNhap)
            {
                MessageBox.Show("Giá bán không nên thấp hơn giá nhập!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return true;
        }

        private void Save()
        {
            if (!IsValidInput()) return;

            if (IsAdding)
            {
                if (_originalData.Any(x => x.MaThuoc == MaThuoc))
                {
                    MessageBox.Show("Mã thuốc này đã tồn tại!");
                    return;
                }

                var newThuoc = new Thuoc
                {
                    MaThuoc = MaThuoc,
                    TenThuoc = TenThuoc,
                    DVT = DVT,
                    GiaNhap = GiaNhap,
                    GiaBan = GiaBan,
                    HanSD = HanSD,
                    HuongDan = HuongDan
                };

                _originalData.Add(newThuoc);
                ExecuteSearch();
                MessageBox.Show("Thêm thuốc thành công!");
            }
            else // Chế độ Sửa
            {
                var thuoc = _originalData.FirstOrDefault(x => x.MaThuoc == SelectedItem.MaThuoc);
                if (thuoc != null)
                {
                    // 1. Cập nhật vào danh sách gốc
                    thuoc.TenThuoc = TenThuoc;
                    thuoc.DVT = DVT;
                    thuoc.GiaNhap = GiaNhap;
                    thuoc.GiaBan = GiaBan;
                    thuoc.HanSD = HanSD;
                    thuoc.HuongDan = HuongDan;

                    // 2. Cập nhật trực tiếp lên SelectedItem để DataGrid thay đổi ngay
                    SelectedItem.TenThuoc = TenThuoc;
                    SelectedItem.DVT = DVT;
                    SelectedItem.GiaNhap = GiaNhap;
                    SelectedItem.GiaBan = GiaBan;
                    SelectedItem.HanSD = HanSD;
                    SelectedItem.HuongDan = HuongDan;

                    ExecuteSearch(); // Cập nhật lại ObservableCollection
                    MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo");
                }
            }
            ResetState();
        }

        private void Delete()
        {
            if (MessageBox.Show("Xác nhận xóa thuốc này?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _originalData.Remove(SelectedItem);
                ExecuteSearch();
                ClearForm();
            }
        }

        private void ResetState()
        {
            IsAdding = IsEditing = false;
            if (SelectedItem != null)
            {
                MaThuoc = SelectedItem.MaThuoc;
                TenThuoc = SelectedItem.TenThuoc;
                DVT = SelectedItem.DVT;
                GiaNhap = SelectedItem.GiaNhap;
                GiaBan = SelectedItem.GiaBan;
                HanSD = SelectedItem.HanSD;
                HuongDan = SelectedItem.HuongDan;
            }
            else ClearForm();
        }

        private void ClearForm()
        {
            MaThuoc = TenThuoc = DVT = HuongDan = "";
            GiaNhap = GiaBan = 0;
            HanSD = DateTime.Now;
        }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(AddButtonText));
            OnPropertyChanged(nameof(EditButtonText));
            OnPropertyChanged(nameof(IsInputEnabled));
            LuuCommand.RaiseCanExecuteChanged();
        }

        private void LoadData()
        {
            _originalData = new List<Thuoc>();
            // Có thể thêm dữ liệu mẫu ở đây
            ListThuoc = new ObservableCollection<Thuoc>(_originalData);
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ListThuoc = new ObservableCollection<Thuoc>(_originalData);
            }
            else
            {
                var filtered = _originalData.Where(x =>
                    x.TenThuoc.ToLower().Contains(SearchText.ToLower()) ||
                    x.MaThuoc.ToLower().Contains(SearchText.ToLower())
                ).ToList();
                ListThuoc = new ObservableCollection<Thuoc>(filtered);
            }
        }
    }
}

