// =============================================================================
// LocalModels.cs
// File này chứa các class UI-helper dùng trong ViewModels.
// KHÔNG phải EF Entity. Đây là các model phục vụ hiển thị trên giao diện.
// =============================================================================

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    // -------------------------------------------------------------------------
    // Dùng cho DonThuocViewModel - đại diện một đơn thuốc trên UI
    // -------------------------------------------------------------------------
    public class DonThuocLocal : INotifyPropertyChanged
    {
        public string MaDonThuoc { get; set; }
        public string MaPhieuKham { get; set; }
        public DateTime NgayKe { get; set; }
        public string MaNV_Ke { get; set; }
        public string TenBacSiKe { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // -------------------------------------------------------------------------
    // Dùng cho DonThuocViewModel & PhieuXuatThuocViewModel - chi tiết một đơn thuốc
    // -------------------------------------------------------------------------
    public class ChiTietDonThuocLocal : INotifyPropertyChanged
    {
        public string MaDonThuoc { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DVT { get; set; }

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set { _soLuong = value; OnPropertyChanged(); OnPropertyChanged(nameof(ThanhTien)); }
        }

        private decimal _giaBan;
        public decimal GiaBan
        {
            get => _giaBan;
            set { _giaBan = value; OnPropertyChanged(); OnPropertyChanged(nameof(ThanhTien)); }
        }

        public string LieuDung { get; set; }
        public decimal ThanhTien => SoLuong * GiaBan;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // -------------------------------------------------------------------------
    // Dùng cho HoaDonViewModel - chi tiết hóa đơn trên UI
    // -------------------------------------------------------------------------
    public class ChiTietHoaDonLoc : INotifyPropertyChanged
    {
        public string Loai { get; set; }
        public string MaDoiTuong { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // -------------------------------------------------------------------------
    // Dùng cho DonThuocViewModel - thông tin bác sĩ để hiển thị trong ComboBox
    // -------------------------------------------------------------------------
    public class BacSiLocal
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
    }

    // -------------------------------------------------------------------------
    // Dùng cho DonThuocViewModel - thông tin thuốc gốc để tra cứu giá
    // -------------------------------------------------------------------------
    public class ThuocGocLocal
    {
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DVT { get; set; }
        public decimal GiaBan { get; set; }
    }

    // -------------------------------------------------------------------------
    // Dùng cho HoSoKham - đại diện dịch vụ y tế trong màn hình khám
    // -------------------------------------------------------------------------
    public class DichVuKhamLocal : INotifyPropertyChanged
    {
        public string MA_DICHVU { get; set; }

        private string _tenDichVu;
        public string TenDichVu
        {
            get => _tenDichVu;
            set { _tenDichVu = value; OnPropertyChanged(); }
        }

        private double _giaTien;
        public double GiaTien
        {
            get => _giaTien;
            set { _giaTien = value; OnPropertyChanged(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
