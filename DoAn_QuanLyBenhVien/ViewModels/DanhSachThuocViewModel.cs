using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DoAn_QuanLyBenhVien.ViewModels;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DanhSachThuocViewModel : BaseViewModel
    {
        // Dùng đúng EF entity THUOC (MA_THUOC, TEN_THUOC, GIA_BAN, SOLUONG_TON)
        private ObservableCollection<THUOC> _listThuoc;
        public ObservableCollection<THUOC> ListThuoc
        {
            get => _listThuoc;
            set { _listThuoc = value; OnPropertyChanged(); }
        }
        private List<THUOC> _originalData = new List<THUOC>();

        // Input fields (chỉ những gì entity THUOC có)
        private string _maThuoc;
        public string MaThuoc { get => _maThuoc; set { _maThuoc = value; OnPropertyChanged(); } }

        private string _tenThuoc;
        public string TenThuoc { get => _tenThuoc; set { _tenThuoc = value; OnPropertyChanged(); } }

        private decimal _giaBan;
        public decimal GiaBan { get => _giaBan; set { _giaBan = value; OnPropertyChanged(); } }

        private int? _soLuongTon;
        public int? SoLuongTon { get => _soLuongTon; set { _soLuongTon = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); if (string.IsNullOrEmpty(value)) ExecuteSearch(); }
        }

        private THUOC _selectedItem;
        public THUOC SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (_selectedItem != null && !IsAdding && !IsEditing)
                {
                    MaThuoc    = _selectedItem.MA_THUOC?.Trim();
                    TenThuoc   = _selectedItem.TEN_THUOC;
                    GiaBan     = _selectedItem.GIA_BAN;
                    SoLuongTon = _selectedItem.SOLUONG_TON;
                }
                SuaCommand.RaiseCanExecuteChanged();
                XoaCommand.RaiseCanExecuteChanged();
            }
        }

        // UI State
        private bool _isAdding;
        public bool IsAdding { get => _isAdding; set { _isAdding = value; OnPropertyChanged(); UpdateUI(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); UpdateUI(); } }

        public bool IsInputEnabled => IsAdding || IsEditing;
        public string AddButtonText => IsAdding ? "Hủy" : "Thêm mới";
        public string EditButtonText => IsEditing ? "Hủy sửa" : "Cập nhật";

        // Commands
        public RelayCommand ThemCommand { get; }
        public RelayCommand SuaCommand { get; }
        public RelayCommand XoaCommand { get; }
        public RelayCommand LuuCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearCommand { get; }

        public DanhSachThuocViewModel()
        {
            LoadData();

            ThemCommand   = new RelayCommand(p => AddOrCancel(), p => !IsEditing);
            SuaCommand    = new RelayCommand(p => EditOrCancel(), p => SelectedItem != null && !IsAdding);
            LuuCommand    = new RelayCommand(p => Save(), p => IsAdding || IsEditing);
            XoaCommand    = new RelayCommand(p => Delete(), p => SelectedItem != null && !IsAdding && !IsEditing);
            SearchCommand = new RelayCommand(p => ExecuteSearch());
            ClearCommand  = new RelayCommand(p => { ClearForm(); ResetState(); });
        }

        private void AddOrCancel() 
        { 
            if (IsAdding) { ResetState(); return; } 
            ClearForm(); 
            IsAdding = true; 
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    MaThuoc = IdGenerator.GetNextMaThuoc(db);
                }
            } catch { }
        }
        private void EditOrCancel() { if (IsEditing) { ResetState(); return; } IsEditing = true; }

        private bool IsValidInput()
        {
            if (string.IsNullOrWhiteSpace(MaThuoc) || string.IsNullOrWhiteSpace(TenThuoc))
            {
                MessageBox.Show("Vui lòng nhập Mã và Tên thuốc!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void Save()
        {
            if (!IsValidInput()) return;

            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    if (IsAdding)
                    {
                        if (db.THUOCs.Any(x => x.MA_THUOC.Trim() == MaThuoc.Trim()))
                        { MessageBox.Show("Mã thuốc đã tồn tại!"); return; }

                        if (db.THUOCs.Any(x => x.TEN_THUOC.ToLower() == TenThuoc.ToLower()))
                        {
                            if (MessageBox.Show("Đã tồn tại thuốc có cùng tên. Bạn có chắc muốn tiếp tục thêm?", "Cảnh báo trùng lặp", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            {
                                return;
                            }
                        }

                        var newThuoc = new THUOC
                        {
                            MA_THUOC    = MaThuoc,
                            TEN_THUOC   = TenThuoc,
                            GIA_BAN     = GiaBan,
                            SOLUONG_TON = SoLuongTon
                        };
                        // ✅ Thêm vào Database
                        db.THUOCs.Add(newThuoc);
                        db.SaveChanges();

                        _originalData.Add(newThuoc);
                        ExecuteSearch();
                        MessageBox.Show("Thêm thuốc thành công!");
                    }
                    else
                    {
                        // ✅ Cập nhật vào Database
                        var thuoc = db.THUOCs.Find(SelectedItem.MA_THUOC);
                        if (thuoc != null)
                        {
                            thuoc.TEN_THUOC   = TenThuoc;
                            thuoc.GIA_BAN     = GiaBan;
                            thuoc.SOLUONG_TON = SoLuongTon;
                            db.Entry(thuoc).State = EntityState.Modified;
                            db.SaveChanges();

                            // Cập nhật cache local
                            var local = _originalData.FirstOrDefault(x => x.MA_THUOC == SelectedItem.MA_THUOC);
                            if (local != null) { local.TEN_THUOC = TenThuoc; local.GIA_BAN = GiaBan; local.SOLUONG_TON = SoLuongTon; }
                            ExecuteSearch();
                            MessageBox.Show("Cập nhật thành công!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResetState();
        }

        private void Delete()
        {
            if (MessageBox.Show("Xác nhận xóa thuốc này?", "Thông báo",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        // ✅ Xóa khỏi Database
                        var thuoc = db.THUOCs.Find(SelectedItem.MA_THUOC);
                        if (thuoc != null)
                        {
                            db.THUOCs.Remove(thuoc);
                            db.SaveChanges();
                        }
                    }
                    _originalData.Remove(SelectedItem);
                    ExecuteSearch(); ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetState()
        {
            IsAdding = IsEditing = false;
            if (SelectedItem != null)
            {
                MaThuoc    = SelectedItem.MA_THUOC?.Trim();
                TenThuoc   = SelectedItem.TEN_THUOC;
                GiaBan     = SelectedItem.GIA_BAN;
                SoLuongTon = SelectedItem.SOLUONG_TON;
            }
            else ClearForm();
        }

        private void ClearForm() { MaThuoc = TenThuoc = ""; GiaBan = 0; SoLuongTon = 0; }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(AddButtonText));
            OnPropertyChanged(nameof(EditButtonText));
            OnPropertyChanged(nameof(IsInputEnabled));
            LuuCommand.RaiseCanExecuteChanged();
        }

        // ✅ LoadData(): Xóa empty list, load từ Database qua EF
        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    _originalData = db.THUOCs.OrderBy(x => x.MA_THUOC).ToList();
                    ListThuoc     = new ObservableCollection<THUOC>(_originalData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                _originalData = new List<THUOC>();
                ListThuoc     = new ObservableCollection<THUOC>();
            }
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                ListThuoc = new ObservableCollection<THUOC>(_originalData);
            else
            {
                var kw = SearchText.ToLower();
                var filtered = _originalData.Where(x =>
                    (x.TEN_THUOC != null && x.TEN_THUOC.ToLower().Contains(kw)) ||
                    (x.MA_THUOC  != null && x.MA_THUOC.ToLower().Contains(kw))
                ).ToList();
                ListThuoc = new ObservableCollection<THUOC>(filtered);
            }
        }
    }
}
