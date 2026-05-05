using DoAn_QuanLyBenhVien.Models;
using System;
using System.Linq;
using System.Data.Entity;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class TrangChuViewModel : BaseViewModel
    {
        private int _tongBenhNhan;
        public int TongBenhNhan { get => _tongBenhNhan; set { _tongBenhNhan = value; OnPropertyChanged(); } }

        private int _tongDoanhThu;
        public int TongDoanhThu { get => _tongDoanhThu; set { _tongDoanhThu = value; OnPropertyChanged(); } }

        private int _soDonThuocCho;
        public int SoDonThuocCho { get => _soDonThuocCho; set { _soDonThuocCho = value; OnPropertyChanged(); } }

        private int _soBenhNhanCho;
        public int SoBenhNhanCho { get => _soBenhNhanCho; set { _soBenhNhanCho = value; OnPropertyChanged(); } }

        public TrangChuViewModel()
        {
            LoadThongKe();
        }

        private void LoadThongKe()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    TongBenhNhan  = db.BENHNHANs.Count();
                    
                    TongDoanhThu = (int)(db.HOADONs.Where(x => x.TRANGTHAI == "Đã thanh toán").Sum(x => x.TONGTIEN) ?? 0);
                    
                    SoDonThuocCho = db.DONTHUOCs.Count(x => x.TRANGTHAI == "Chờ xuất");
                    
                    var homNay = DateTime.Today;
                    // Lấy số phiếu khám chưa hoàn thành hoặc số bệnh nhân trong bảng (tuỳ logic)
                    SoBenhNhanCho = db.PHIEUKHAMs.Count(x => x.TRANGTHAI != "Hoàn thành" && DbFunctions.TruncateTime(x.NGAYKHAM) == homNay);
                }
            }
            catch { }
        }
    }
}
