using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DoAn_QuanLyBenhVien.Models;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // ✅ Lưu thông tin nhân viên đang đăng nhập (dùng cho các ViewModel khác)
        public static string HoTenHienTai { get; private set; }
        public static string QuyenHienTai  { get; private set; }
        public static string MaNVHienTai   { get; private set; }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string _tenDangNhap;
        public string TenDangNhap
        {
            get => _tenDangNhap;
            set => SetProperty(ref _tenDangNhap, value);
        }

        public string MatKhau { get; set; }

        private string _thongBaoLoi;
        public string ThongBaoLoi
        {
            get => _thongBaoLoi;
            set => SetProperty(ref _thongBaoLoi, value);
        }

        private bool _coLoi;
        public bool CoLoi
        {
            get => _coLoi;
            set => SetProperty(ref _coLoi, value);
        }

        // ✅ DangNhap(): Thay mock accounts → query bảng TAIKHOAN từ Database
        public bool DangNhap()
        {
            if (string.IsNullOrWhiteSpace(TenDangNhap) || string.IsNullOrWhiteSpace(MatKhau))
            {
                ThongBaoLoi = "Vui lòng nhập đầy đủ thông tin.";
                CoLoi = true;
                return false;
            }

            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    // Query bảng TAIKHOAN, join NHANVIEN và QUYEN
                    var tk = db.TAIKHOANs
                        .Where(x => x.TenDangNhap == TenDangNhap && x.MatKhau == MatKhau)
                        .FirstOrDefault();

                    if (tk != null && (tk.TrangThai == null || tk.TrangThai != "Khóa"))
                    {
                        // Lưu thông tin người dùng vào static props
                        MaNVHienTai   = tk.MaNV?.Trim();
                        HoTenHienTai  = tk.NHANVIEN?.HoTen ?? TenDangNhap;
                        QuyenHienTai  = tk.QUYEN?.TenQuyen ?? tk.MaQuyen ?? "Nhân viên";
                        CoLoi = false;
                        return true;
                    }
                    else if (tk != null && tk.TrangThai == "Khóa")
                    {
                        ThongBaoLoi = "Tài khoản của bạn đã bị khóa.";
                        CoLoi = true;
                        return false;
                    }
                    else
                    {
                        ThongBaoLoi = "Tên đăng nhập hoặc mật khẩu không đúng.";
                        CoLoi = true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback: nếu không kết nối được DB, thông báo lỗi chi tiết kèm InnerException
                string msg = ex.Message;
                if (ex.InnerException != null) msg += "\nChi tiết: " + ex.InnerException.Message;
                
                ThongBaoLoi = "Lỗi kết nối Database: " + msg;
                CoLoi = true;
                return false;
            }
        }
    }
}
