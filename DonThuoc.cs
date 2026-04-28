using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class DonThuoc
    {
        public string MaDonThuoc { get; set; }
        public string MaPhieuKham { get; set; }
        public DateTime NgayKe { get; set; }
        public string MaNV_Ke { get; set; }
        public string TenBacSiKe { get; set; }
    }

    public class ChiTietDonThuoc
    {
        public string MaDonThuoc { get; set; }
        public string MaThuoc { get; set; }
        public int SoLuong { get; set; }
        public string LieuDung { get; set; }
        public string TenThuoc { get; set; }
        public decimal? GiaBan { get; set; }
        public string DVT { get; set; }
        public decimal ThanhTien => (GiaBan ?? 0) * SoLuong;
    }
 }
