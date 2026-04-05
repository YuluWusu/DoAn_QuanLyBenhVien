using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_QuanLyBenhVien.Models
{
    public class Thuoc
    {
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DVT { get; set; }
        public decimal? GiaNhap { get; set; }
        public decimal? GiaBan { get; set; }
        public DateTime? HanSD { get; set; }
        public string HuongDan { get; set; }
    }
}
