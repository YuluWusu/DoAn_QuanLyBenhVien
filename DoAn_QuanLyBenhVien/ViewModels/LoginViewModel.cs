using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
namespace DoAn_QuanLyBenhVien.ViewModels
{
        public class LoginViewModel : BaseViewModel
        {
            private static readonly List<(string TenDangNhap, string MatKhau, string HoTen, string Quyen)> _mockAccounts = new List<(string, string, string, string)>
        {
            ("letan1",   "123", "Nguyễn Thị Lan",   "Lễ tân"),
            ("bacsi1",   "123", "BS. Trần Văn Minh", "Bác sĩ"),
            ("duocsi1",  "123", "DS. Lê Thị Hoa",   "Dược sĩ"),
            ("thungan1", "123", "Phạm Văn Tú",       "Thu ngân"),
            ("admin",    "123", "Admin Hệ thống",    "Admin"),
        };
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


            public static string HoTenHienTai { get; private set; }
            public static string QuyenHienTai { get; private set; }

            public bool DangNhap()
            {
                if (string.IsNullOrWhiteSpace(TenDangNhap) || string.IsNullOrWhiteSpace(MatKhau))
                {
                    ThongBaoLoi = "Vui lòng nhập đầy đủ thông tin.";
                    CoLoi = true;
                    return false;
                }

                foreach (var acc in _mockAccounts)
                {
                    if (acc.TenDangNhap == TenDangNhap && acc.MatKhau == MatKhau)
                    {
                        HoTenHienTai = acc.HoTen;
                        QuyenHienTai = acc.Quyen;
                        CoLoi = false;
                        return true;
                    }
                }

                ThongBaoLoi = "Tên đăng nhập hoặc mật khẩu không đúng.";
                CoLoi = true;
                return false;
            }
        }
    }
