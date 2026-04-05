using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class BenhNhan
    {
        public string MaBenhNhan { get; set; }
        public string TenBenhNhan { get; set; }
        public string GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string TienSuBenhLy { get; set; }
        public DateTime NgayTaoHoSo { get; set; }
    }
}
