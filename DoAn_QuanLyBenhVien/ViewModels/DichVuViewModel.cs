using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DichVuViewModel : BaseViewModel
    {
        // Dùng đúng EF entity DICHVU (MA_DICHVU, TEN_DICHVU, GIA_DV)
        private ObservableCollection<DICHVU> _danhSachDichVu;
        public ObservableCollection<DICHVU> DanhSachDichVu
        {
            get => _danhSachDichVu;
            set { _danhSachDichVu = value; OnPropertyChanged(); }
        }

        // Input fields
        private string _maDichVu;
        public string MaDichVu { get => _maDichVu; set { _maDichVu = value; OnPropertyChanged(); } }

        private string _tenDichVu;
        public string TenDichVu { get => _tenDichVu; set { _tenDichVu = value; OnPropertyChanged(); } }

        private decimal _giaDV;
        public decimal GiaDV { get => _giaDV; set { _giaDV = value; OnPropertyChanged(); } }

        // Selected item
        private DICHVU _selectedDichVu;
        public DICHVU SelectedDichVu
        {
            get => _selectedDichVu;
            set
            {
                _selectedDichVu = value;
                OnPropertyChanged();
                if (_selectedDichVu != null && !_isAdding && !_isEditing)
                {
                    MaDichVu  = _selectedDichVu.MA_DICHVU?.Trim();
                    TenDichVu = _selectedDichVu.TEN_DICHVU;
                    GiaDV     = _selectedDichVu.GIA_DV;
                    IsEditDeleteEnabled = true;
                }
            }
        }

        // UI State
        private bool _isEditable = false;
        public bool IsEditable { get => _isEditable; set { _isEditable = value; OnPropertyChanged(); } }

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

        // Commands
        public ICommand AddCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public DichVuViewModel()
        {
            // ✅ Load từ Database thật (không còn mock data)
            LoadData();

            AddCommand    = new RelayCommand(ExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate, CanExecuteUpdateDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteUpdateDelete);
            SaveCommand   = new RelayCommand(ExecuteSave, CanExecuteSave);

            ResetUIState();
        }

        // ✅ LoadData(): Load từ bảng DICHVU trong Database
        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    DanhSachDichVu = new ObservableCollection<DICHVU>(
                        db.DICHVUs.OrderBy(x => x.MA_DICHVU).ToList()
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                DanhSachDichVu = new ObservableCollection<DICHVU>();
            }
        }

        private void ExecuteAdd(object obj)
        {
            if (!_isAdding && !_isEditing)
            {
                IsAddingEditingState(true);
                IsSaveEnabled = true; IsEditDeleteEnabled = false; IsEditable = true;
                ClearFields();
            }
            else { IsAddingEditingState(false); ClearFields(); ResetUIState(); }
        }

        private void ExecuteUpdate(object obj)
        {
            _isEditing = true; IsProcessing = true;
            IsSaveEnabled = true; IsEditDeleteEnabled = false;
            IsEditable = true; AddButtonContent = "❌ Hủy";
        }

        private void ExecuteDelete(object obj)
        {
            if (SelectedDichVu != null &&
                MessageBox.Show("Bạn có chắc muốn xóa dịch vụ này?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        // ✅ Xóa khỏi Database
                        var dv = db.DICHVUs.Find(SelectedDichVu.MA_DICHVU);
                        if (dv != null)
                        {
                            db.DICHVUs.Remove(dv);
                            db.SaveChanges();
                        }
                    }
                    DanhSachDichVu.Remove(SelectedDichVu);
                    ClearFields(); ResetUIState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteSave(object obj)
        {
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    if (_isAdding)
                    {
                        if (db.DICHVUs.Any(x => x.MA_DICHVU.Trim() == MaDichVu.Trim()))
                        {
                            MessageBox.Show("Mã dịch vụ đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var newDV = new DICHVU { MA_DICHVU = MaDichVu, TEN_DICHVU = TenDichVu, GIA_DV = GiaDV };
                        // ✅ Thêm vào Database
                        db.DICHVUs.Add(newDV);
                        db.SaveChanges();
                        DanhSachDichVu.Add(newDV);
                    }
                    else if (_isEditing && SelectedDichVu != null)
                    {
                        // ✅ Cập nhật vào Database
                        var existing = db.DICHVUs.Find(SelectedDichVu.MA_DICHVU);
                        if (existing != null)
                        {
                            existing.TEN_DICHVU = TenDichVu;
                            existing.GIA_DV     = GiaDV;
                            db.Entry(existing).State = EntityState.Modified;
                            db.SaveChanges();

                            // Cập nhật UI collection
                            var uiItem = DanhSachDichVu.FirstOrDefault(x => x.MA_DICHVU == SelectedDichVu.MA_DICHVU);
                            if (uiItem != null) { uiItem.TEN_DICHVU = TenDichVu; uiItem.GIA_DV = GiaDV; }
                        }
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
            LoadData(); // Refresh lại danh sách
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
            _isAdding = state; _isEditing = false;
            IsProcessing = state; IsEditable = state;
            AddButtonContent = state ? "❌ Hủy" : "➕ Thêm";
        }

        private void ResetUIState()
        {
            AddButtonContent = "➕ Thêm"; IsSaveEnabled = false;
            IsEditDeleteEnabled = (SelectedDichVu != null);
            IsProcessing = false; _isAdding = false; _isEditing = false; IsEditable = false;
        }

        private void ClearFields()
        {
            MaDichVu = ""; TenDichVu = ""; GiaDV = 0;
            SelectedDichVu = null;
            OnPropertyChanged(nameof(SelectedDichVu));
        }
    }
}
