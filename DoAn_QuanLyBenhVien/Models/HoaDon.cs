using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class HoaDon
    {
        public string MaHoaDon { get; set; }
        public string MaPhieuKham { get; set; }
        public string MaNV { get; set; }
        public DateTime NgayLap { get; set; }
        public string TrangThai { get; set; }
        public string TenThuNgan { get; set; }
        public string TenBenhNhan { get; set; }
        public decimal TongTien { get; set; }
    }
}
