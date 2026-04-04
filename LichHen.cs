using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class LichHen
    {
        public string MaLichHen { get; set; }
        public string MaBenhNhan { get; set; }
        public string MaNV { get; set; }
        public DateTime NgayHen { get; set; }
        public string LyDoKham { get; set; }
        public string TrangThai { get; set; }

        // Các thuộc tính liên kết (Navigation Properties)
        public virtual BenhNhan BenhNhan { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}
