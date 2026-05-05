using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DoAn_QuanLyBenhVien.Models;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class DSNhanVienViewModel : BaseViewModel
    {
        // -------------------------------------------------------------------
        // Dữ liệu - dùng EF entity NHANVIEN và CHUYENKHOA
        // -------------------------------------------------------------------
        private ObservableCollection<NHANVIEN> _listNhanVien;
        public ObservableCollection<NHANVIEN> ListNhanVien
        {
            get => _listNhanVien;
            set { _listNhanVien = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CHUYENKHOA> ListChuyenKhoa { get; set; }
        private List<NHANVIEN> _originalData = new List<NHANVIEN>();

        // -------------------------------------------------------------------
        // Input fields (chỉ dùng những gì entity NHANVIEN thật sự có)
        // -------------------------------------------------------------------
        private string _maNV;
        public string MaNV { get => _maNV; set { _maNV = value; OnPropertyChanged(); } }

        private string _hoTen;
        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }

        private string _chucVu;
        public string ChucVu { get => _chucVu; set { _chucVu = value; OnPropertyChanged(); } }

        private string _maCK;
        public string MaCK { get => _maCK; set { _maCK = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); if (string.IsNullOrEmpty(value)) ExecuteSearch(); }
        }

        private CHUYENKHOA _selectedChuyenKhoa;
        public CHUYENKHOA SelectedChuyenKhoa
        {
            get => _selectedChuyenKhoa;
            set { _selectedChuyenKhoa = value; OnPropertyChanged(); if (value != null) MaCK = value.MaCK; }
        }

        private NHANVIEN _selectedItem;
        public NHANVIEN SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (_selectedItem != null && !IsAdding && !IsEditing)
                {
                    MaNV   = _selectedItem.MaNV;
                    HoTen  = _selectedItem.HoTen;
                    ChucVu = _selectedItem.ChucVu;
                    SelectedChuyenKhoa = ListChuyenKhoa?.FirstOrDefault(x => x.MaCK == _selectedItem.MaCK);
                }
                SuaCommand.RaiseCanExecuteChanged();
                XoaCommand.RaiseCanExecuteChanged();
            }
        }

        // -------------------------------------------------------------------
        // UI State
        // -------------------------------------------------------------------
        private bool _isAdding;
        public bool IsAdding { get => _isAdding; set { _isAdding = value; OnPropertyChanged(); UpdateUI(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); UpdateUI(); } }

        public bool IsInputEnabled => IsAdding || IsEditing;
        public string AddButtonText => IsAdding ? "Hủy" : "Thêm";
        public string EditButtonText => IsEditing ? "Hủy Sửa" : "Sửa";

        // -------------------------------------------------------------------
        // Commands
        // -------------------------------------------------------------------
        public RelayCommand ThemCommand { get; }
        public RelayCommand SuaCommand { get; }
        public RelayCommand XoaCommand { get; }
        public RelayCommand LuuCommand { get; }
        public RelayCommand SearchCommand { get; }

        public DSNhanVienViewModel()
        {
            LoadData();

            ThemCommand   = new RelayCommand(p => AddOrCancel(), p => !IsEditing);
            SuaCommand    = new RelayCommand(p => EditOrCancel(), p => SelectedItem != null && !IsAdding);
            LuuCommand    = new RelayCommand(p => Save(), p => IsAdding || IsEditing);
            XoaCommand    = new RelayCommand(p => Delete(), p => SelectedItem != null && !IsAdding && !IsEditing);
            SearchCommand = new RelayCommand(p => ExecuteSearch());
        }

        // -------------------------------------------------------------------
        // Command Handlers
        // -------------------------------------------------------------------
        private void AddOrCancel() 
        { 
            if (IsAdding) { ResetState(); return; } 
            ClearForm(); 
            IsAdding = true; 
            
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    MaNV = IdGenerator.GetNextMaNhanVien(db);
                }
            } catch { }
        }
        
        private void EditOrCancel() { if (IsEditing) { ResetState(); return; } IsEditing = true; }

        private bool IsValidInput()
        {
            if (string.IsNullOrWhiteSpace(MaNV) || string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Mã NV và Họ tên!", "Thông báo",
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
                        if (db.NHANVIENs.Any(x => x.MaNV == MaNV))
                        { MessageBox.Show("Mã nhân viên đã tồn tại!"); return; }

                        var newNV = new NHANVIEN { MaNV = MaNV, HoTen = HoTen, ChucVu = ChucVu, MaCK = string.IsNullOrWhiteSpace(MaCK) ? null : MaCK };
                        // ✅ Thêm vào Database
                        db.NHANVIENs.Add(newNV);
                        db.SaveChanges();

                        _originalData.Add(newNV);
                        ExecuteSearch();
                        MessageBox.Show("Thêm nhân viên thành công!");
                    }
                    else
                    {
                        // ✅ Cập nhật vào Database
                        var nv = db.NHANVIENs.Find(SelectedItem.MaNV);
                        if (nv != null)
                        {
                            nv.HoTen = HoTen; nv.ChucVu = ChucVu; nv.MaCK = string.IsNullOrWhiteSpace(MaCK) ? null : MaCK;
                            db.Entry(nv).State = EntityState.Modified;
                            db.SaveChanges();

                            // Cập nhật cache local
                            var local = _originalData.FirstOrDefault(x => x.MaNV == SelectedItem.MaNV);
                            if (local != null) { local.HoTen = HoTen; local.ChucVu = ChucVu; local.MaCK = MaCK; }
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
            if (MessageBox.Show("Xác nhận xóa nhân viên này?", "Thông báo",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        // ✅ Xóa khỏi Database
                        var nv = db.NHANVIENs.Find(SelectedItem.MaNV);
                        if (nv != null)
                        {
                            db.NHANVIENs.Remove(nv);
                            db.SaveChanges();
                        }
                    }
                    _originalData.Remove(SelectedItem);
                    ExecuteSearch();
                    ClearForm();
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
                MaNV = SelectedItem.MaNV; HoTen = SelectedItem.HoTen;
                ChucVu = SelectedItem.ChucVu;
                SelectedChuyenKhoa = ListChuyenKhoa?.FirstOrDefault(x => x.MaCK == SelectedItem.MaCK);
            }
            else ClearForm();
        }

        private void ClearForm()
        {
            MaNV = HoTen = ChucVu = MaCK = "";
            SelectedChuyenKhoa = null;
        }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(AddButtonText));
            OnPropertyChanged(nameof(EditButtonText));
            OnPropertyChanged(nameof(IsInputEnabled));
            LuuCommand.RaiseCanExecuteChanged();
        }

        // ✅ LoadData(): Kết nối Database thực
        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    ListChuyenKhoa = new ObservableCollection<CHUYENKHOA>(db.CHUYENKHOAs.ToList());
                    _originalData  = db.NHANVIENs.ToList();
                    ListNhanVien   = new ObservableCollection<NHANVIEN>(_originalData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                ListChuyenKhoa = new ObservableCollection<CHUYENKHOA>();
                ListNhanVien   = new ObservableCollection<NHANVIEN>();
            }
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                ListNhanVien = new ObservableCollection<NHANVIEN>(_originalData);
            else
            {
                var kw = SearchText.ToLower();
                var filtered = _originalData.Where(x =>
                    (x.HoTen != null && x.HoTen.ToLower().Contains(kw)) ||
                    (x.MaNV  != null && x.MaNV.ToLower().Contains(kw))
                ).ToList();
                ListNhanVien = new ObservableCollection<NHANVIEN>(filtered);
            }
        }
    }
}
