using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DoAn_QuanLyBenhVien.Helper;
using DoAn_QuanLyBenhVien.Models;
using LiveCharts;
using LiveCharts.Wpf;
using OfficeOpenXml;
using Microsoft.Win32;
using System.IO;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class VM_ThongKe : BaseViewModel
    {
        private SeriesCollection _revenueSeries;
        public SeriesCollection RevenueSeries
        {
            get => _revenueSeries;
            set { _revenueSeries = value; OnPropertyChanged(); }
        }

        private string[] _revenueLabels;
        public string[] RevenueLabels
        {
            get => _revenueLabels;
            set { _revenueLabels = value; OnPropertyChanged(); }
        }

        private SeriesCollection _serviceRatioSeries;
        public SeriesCollection ServiceRatioSeries
        {
            get => _serviceRatioSeries;
            set { _serviceRatioSeries = value; OnPropertyChanged(); }
        }

        private int _totalPatients;
        public int TotalPatients
        {
            get => _totalPatients;
            set { _totalPatients = value; OnPropertyChanged(); }
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        public RelayCommand LoadDataCommand { get; }
        public RelayCommand ExportExcelCommand { get; }

        public VM_ThongKe()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Student");

            LoadDataCommand = new RelayCommand(async p => await LoadStatistics());
            ExportExcelCommand = new RelayCommand(p => ExportToExcel());

            // Run initial load
            _ = LoadStatistics();
        }

        private async Task LoadStatistics()
        {
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    // 1. Total Patients
                    TotalPatients = db.BENHNHANs.Count();

                    // 2. Total Revenue (HOADON.TONGTIEN)
                    TotalRevenue = db.HOADONs.Select(x => (decimal?)x.TONGTIEN).Sum() ?? 0;

                    // 3. Revenue Chart (by month of current year)
                    int currentYear = DateTime.Now.Year;
                    var invoices = db.HOADONs
                        .Where(x => x.PHIEUKHAM.NGAYKHAM != null && x.PHIEUKHAM.NGAYKHAM.Value.Year == currentYear)
                        .ToList();

                    var revenueByMonth = new decimal[12];
                    foreach (var inv in invoices)
                    {
                        revenueByMonth[inv.PHIEUKHAM.NGAYKHAM.Value.Month - 1] += (decimal)(inv.TONGTIEN ?? 0);
                    }

                    RevenueSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Doanh thu",
                            Values = new ChartValues<decimal>(revenueByMonth)
                        }
                    };
                    RevenueLabels = new[] { "Th1", "Th2", "Th3", "Th4", "Th5", "Th6", "Th7", "Th8", "Th9", "Th10", "Th11", "Th12" };

                    // 4. Service Ratio (Pie Chart)
                    var servicesCount = db.PHIEUKHAM_DICHVU
                        .GroupBy(x => x.DICHVU.TEN_DICHVU)
                        .Select(g => new { ServiceName = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(5)
                        .ToList();

                    var pieSeries = new SeriesCollection();
                    foreach (var s in servicesCount)
                    {
                        pieSeries.Add(new PieSeries
                        {
                            Title = s.ServiceName,
                            Values = new ChartValues<int> { s.Count },
                            DataLabels = true
                        });
                    }
                    ServiceRatioSeries = pieSeries;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thống kê: " + ex.Message);
            }
        }

        private void ExportToExcel()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu Báo Cáo Doanh Thu",
                FileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new QL_PHONG_KHAM())
                    {
                        var invoices = db.HOADONs
                            .Select(h => new
                            {
                                h.MA_HOADON,
                                NGAYLAP = h.PHIEUKHAM.NGAYKHAM,
                                h.PHIEUKHAM.BENHNHAN.TEN_BENHNHAN,
                                h.TONGTIEN
                            })
                            .OrderByDescending(x => x.NGAYLAP)
                            .ToList();

                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("Doanh Thu");

                            // Headers
                            worksheet.Cells[1, 1].Value = "Mã Hóa Đơn";
                            worksheet.Cells[1, 2].Value = "Ngày Lập";
                            worksheet.Cells[1, 3].Value = "Tên Bệnh Nhân";
                            worksheet.Cells[1, 4].Value = "Tổng Tiền";

                            worksheet.Cells["A1:D1"].Style.Font.Bold = true;

                            // Data
                            int row = 2;
                            foreach (var inv in invoices)
                            {
                                worksheet.Cells[row, 1].Value = inv.MA_HOADON;
                                worksheet.Cells[row, 2].Value = inv.NGAYLAP?.ToString("dd/MM/yyyy");
                                worksheet.Cells[row, 3].Value = inv.TEN_BENHNHAN;
                                worksheet.Cells[row, 4].Value = inv.TONGTIEN;
                                row++;
                            }

                            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                            File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                            MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
