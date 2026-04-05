using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class TrangChu : BaseViewModel
    {
        //Chuyển UC
        private object _viewHienTai;
        public object ViewHienTai
        {
            get => _viewHienTai;
            set
            {
                _viewHienTai = value;
                OnPropertyChanged();
            }
        }

        // Quản lý tiêu đề và thanh điều hướng
        private string _tiêuĐềTrang = "Tổng quan hôm nay";
        public string TieuDeTrang
        {
            get => _tiêuĐềTrang;
            set { _tiêuĐềTrang = value; OnPropertyChanged(); }
        }

        private string _dieuHuongPhu = "Dashboard";
        public string DieuHuongPhu
        {
            get => _dieuHuongPhu;
            set { _dieuHuongPhu = value; OnPropertyChanged(); }
        }

        // Quản lý đồng hồ hệ thống
        private string _thoiGianHienTai;
        public string ThoiGianHienTai
        {
            get => _thoiGianHienTai;
            set { _thoiGianHienTai = value; OnPropertyChanged(); }
        }

        private DispatcherTimer _boDemThoiGian;

        public TrangChu()
        {
            // Khởi động đồng hồ ngay khi ứng dụng chạy
            ChayDongHo();
        }

        private void ChayDongHo()
        {
            _boDemThoiGian = new DispatcherTimer();
            _boDemThoiGian.Interval = TimeSpan.FromSeconds(1);
            _boDemThoiGian.Tick += (s, e) =>
            {
                ThoiGianHienTai = DateTime.Now.ToString("HH:mm — dddd, dd/MM/yyyy");
            };
            _boDemThoiGian.Start();
        }

        // Hàm hỗ trợ chuyển trang
        public void ChuyenTrang(object trangMoi, string tieuDe, string dieuHuong)
        {
            ViewHienTai = trangMoi;
            TieuDeTrang = tieuDe;
            DieuHuongPhu = dieuHuong;
        }
    }
}
