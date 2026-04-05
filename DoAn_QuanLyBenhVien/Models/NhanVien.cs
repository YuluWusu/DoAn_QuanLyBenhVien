using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class NhanVien
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string MaCK { get; set; }
        public string ChucVu { get; set; }
        public string TenChuyenKhoa { get; set; }
    }
    public class TaiKhoan
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string MaNV { get; set; }
        public string MaQuyen { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public string HoTenNhanVien { get; set; }
        public string TenQuyen { get; set; }
    }
}
