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
    public class DichVuViewModel : BaseViewModel
    {
        private ObservableCollection<DichVu> _danhSachDichVu;
        public ObservableCollection<DichVu> DanhSachDichVu
        {
            get => _danhSachDichVu;
            set { _danhSachDichVu = value; OnPropertyChanged(); }
        }

        private string _maDichVu;
        public string MaDichVu { get => _maDichVu; set { _maDichVu = value; OnPropertyChanged(); } }

        private string _tenDichVu;
        public string TenDichVu { get => _tenDichVu; set { _tenDichVu = value; OnPropertyChanged(); } }

        private decimal _giaDV;
        public decimal GiaDV { get => _giaDV; set { _giaDV = value; OnPropertyChanged(); } }

        private string _moTa;
        public string MoTa { get => _moTa; set { _moTa = value; OnPropertyChanged(); } }

        private bool _dangApDung;
        public bool DangApDung { get => _dangApDung; set { _dangApDung = value; OnPropertyChanged(); } }

        private DichVu _selectedDichVu;
        public DichVu SelectedDichVu
        {
            get => _selectedDichVu;
            set
            {
                _selectedDichVu = value;
                OnPropertyChanged();
                if (SelectedDichVu != null && !_isAdding && !_isEditing)
                {
                    MaDichVu = SelectedDichVu.MaDichVu;
                    TenDichVu = SelectedDichVu.TenDichVu;
                    GiaDV = SelectedDichVu.GiaDV;
                    MoTa = SelectedDichVu.MoTa;
                    DangApDung = SelectedDichVu.DangApDung;

                    IsEditDeleteEnabled = true;
                }
            }
        }
        
        private bool _isEditable = false;
        public bool IsEditable
        {
            get => _isEditable;
            set { _isEditable = value; OnPropertyChanged(); }
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

        public ICommand AddCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public DichVuViewModel()
        {
            DanhSachDichVu = new ObservableCollection<DichVu>()
            {
                new DichVu { MaDichVu = "DV01", TenDichVu = "Khám Tổng Quát", GiaDV = 200000, MoTa = "Khám lâm sàng", DangApDung = true },
                new DichVu { MaDichVu = "DV02", TenDichVu = "Xét Nghiệm Máu", GiaDV = 150000, MoTa = "Xét nghiệm chỉ số", DangApDung = true }
            };

            AddCommand = new RelayCommand(ExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate, CanExecuteUpdateDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteUpdateDelete);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);

            ResetUIState();
        }

        private void ExecuteAdd(object obj)
        {
            if (!_isAdding && !_isEditing)
            {
                IsAddingEditingState(true);
                IsSaveEnabled = true;
                IsEditDeleteEnabled = false;
                IsEditable = true;
                ClearFields();
            }
            else
            {
                IsAddingEditingState(false);
                ClearFields();
                ResetUIState();
            }
        }

        private void ExecuteUpdate(object obj)
        {
            _isEditing = true;
            IsProcessing = true;
            IsSaveEnabled = true;
            IsEditDeleteEnabled = false;
            IsEditable = true;
            AddButtonContent = "❌ Hủy";
        }

        private void ExecuteDelete(object obj)
        {
            if (SelectedDichVu != null)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa dịch vụ này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DanhSachDichVu.Remove(SelectedDichVu);
                    ClearFields();
                    ResetUIState();
                }
            }
        }

        private void ExecuteSave(object obj)
        {
            var dv = new DichVu
            {
                MaDichVu = MaDichVu,
                TenDichVu = TenDichVu,
                GiaDV = GiaDV,
                MoTa = MoTa,
                DangApDung = DangApDung
            };

            if (_isAdding)
            {
                if (DanhSachDichVu.Any(x => x.MaDichVu == MaDichVu))
                {
                    MessageBox.Show("Mã dịch vụ đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                DanhSachDichVu.Add(dv);
            }
            else if (_isEditing && SelectedDichVu != null)
            {
                var existing = DanhSachDichVu.FirstOrDefault(x => x.MaDichVu == SelectedDichVu.MaDichVu);
                if (existing != null)
                {
                    int index = DanhSachDichVu.IndexOf(existing);
                    DanhSachDichVu[index] = dv;
                }
            }

            MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            IsAddingEditingState(false);
            ResetUIState();
            ClearFields();
        }

        private bool CanExecuteSave(object obj)
        {
            if (!_isAdding && !_isEditing) return false;
            return !string.IsNullOrWhiteSpace(MaDichVu) && !string.IsNullOrWhiteSpace(TenDichVu) && GiaDV > 0;
        }

        private bool CanExecuteUpdateDelete(object obj)
        {
            if (IsProcessing) return false;
            return SelectedDichVu != null;
        }

        private void IsAddingEditingState(bool state)
        {
            _isAdding = state;
            _isEditing = false;
            IsProcessing = state;
            IsEditable = state;
            AddButtonContent = state ? "❌ Hủy" : "➕ Thêm";
        }

        private void ResetUIState()
        {
            AddButtonContent = "➕ Thêm";
            IsSaveEnabled = false;
            IsEditDeleteEnabled = (SelectedDichVu != null);
            IsProcessing = false;
            _isAdding = false;
            _isEditing = false;
            IsEditable = false;
        }

        private void ClearFields()
        {
            MaDichVu = string.Empty;
            TenDichVu = string.Empty;
            GiaDV = 0;
            MoTa = string.Empty;
            DangApDung = false;
            SelectedDichVu = null;
            OnPropertyChanged(nameof(SelectedDichVu));
        }
    }
}
