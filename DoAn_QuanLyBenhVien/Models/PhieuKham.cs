using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class PhieuKham
    {
        public string MaPhieuKham { get; set; }
        public string MaBenhNhan { get; set; }
        public string MaNV { get; set; }
        public DateTime NgayKham { get; set; }
        public string TrieuChung { get; set; }
        public string ChanDoan { get; set; }
        public string GhiChu { get; set; }
        public string TenBenhNhan { get; set; }
        public string TenBacSi { get; set; }
    }

    public class PhieuKhamDichVu
    {
        public string MaPhieuKham { get; set; }
        public string MaDichVu { get; set; }
        public int SoLuong { get; set; }
        public string KetQua { get; set; }
        public string TenDichVu { get; set; }
        public decimal GiaDV { get; set; }
    }
}
