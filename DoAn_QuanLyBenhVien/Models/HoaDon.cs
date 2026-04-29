using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class HoaDon: BaseViewModel
    {
        public string MaHoaDon { get; set; }
        public string MaPhieuKham { get; set; }
        public string MaNV { get; set; }
        public DateTime NgayLap { get; set; }
        private string _trangThai;
        public string TrangThai
        {
            get => _trangThai;
            set { _trangThai = value; OnPropertyChanged(); } 
        }
        public string TrangThai { get; set; }
        public string TenThuNgan { get; set; }
        public string TenBenhNhan { get; set; }
        public decimal TongTien { get; set; }
    }
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

    }
}
