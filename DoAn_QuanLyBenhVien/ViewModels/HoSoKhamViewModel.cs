using DoAn_QuanLyBenhVien.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public class HoSoKhamViewModel : BaseViewModel
    {
        // Biến tạm để lưu lại chính xác thời điểm nhấn nút "Thêm Mới"
        private DateTime _ngayVuaNhanThem;

        // --- Các Thuộc Tính Binding Từ Giao Diện ---
        private string _maPhieuInput;
        public string MaPhieuInput
        {
            get => _maPhieuInput;
            set { _maPhieuInput = value; OnPropertyChanged(); }
        }

        // ✅ THAY THẾ: Dùng BENHNHAN thay vì string TenBenhNhanInput để quản lý ComboBox
        private BENHNHAN _benhNhanDuocChon;
        public BENHNHAN BenhNhanDuocChon
        {
            get => _benhNhanDuocChon;
            set { _benhNhanDuocChon = value; OnPropertyChanged(); }
        }

        // ✅ THÊM: Danh sách bệnh nhân nguồn đổ vào ComboBox
        private ObservableCollection<BENHNHAN> _dsBenhNhan;
        public ObservableCollection<BENHNHAN> DSBenhNhan
        {
            get => _dsBenhNhan;
            set { _dsBenhNhan = value; OnPropertyChanged(); }
        }

        private bool _isChuaHoanThanh;
        public bool IsChuaHoanThanh
        {
            get => _isChuaHoanThanh;
            set { _isChuaHoanThanh = value; OnPropertyChanged(); }
        }

        private bool _isDaHoanThanh;
        public bool IsDaHoanThanh
        {
            get => _isDaHoanThanh;
            set { _isDaHoanThanh = value; OnPropertyChanged(); }
        }

        private string _tenNutThem = "Thêm Mới";
        public string TenNutThem
        {
            get => _tenNutThem;
            set { _tenNutThem = value; OnPropertyChanged(); }
        }

        private bool _isSua;
        public bool IsSua
        {
            get => _isSua;
            set { _isSua = value; OnPropertyChanged(); }
        }

        public bool CoChon => SelectedPhieuKham != null;

        // --- Danh Sách Hiển Thị Trên DataGrid ---
        private ObservableCollection<PHIEUKHAM> _dsPhieuKham;
        public ObservableCollection<PHIEUKHAM> DSPhieuKham
        {
            get => _dsPhieuKham;
            set { _dsPhieuKham = value; OnPropertyChanged(); }
        }

        private PHIEUKHAM _selectedPhieuKham;
        public PHIEUKHAM SelectedPhieuKham
        {
            get => _selectedPhieuKham;
            set
            {
                _selectedPhieuKham = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoChon));

                // Đổ dữ liệu từ hàng được chọn lên Form hiển thị
                if (value != null && !IsSua)
                {
                    MaPhieuInput = value.MA_PHIEUKHAM?.Trim();

                    // ✅ Tìm và chọn đúng Bệnh nhân trong danh sách nguồn của ComboBox
                    if (DSBenhNhan != null && value.MA_BENHNHAN != null)
                    {
                        BenhNhanDuocChon = DSBenhNhan.FirstOrDefault(x => x.MA_BENHNHAN == value.MA_BENHNHAN);
                    }
                    else
                    {
                        BenhNhanDuocChon = null;
                    }

                    IsChuaHoanThanh = value.TRANGTHAI == "Chưa hoàn thành";
                    IsDaHoanThanh = value.TRANGTHAI == "Đã hoàn thành" || value.TRANGTHAI == "Hoàn thành";
                }
            }
        }

        // --- Các Lệnh (Commands) ---
        public ICommand LenhThem { get; set; }
        public ICommand LenhSua { get; set; }
        public ICommand LenhXoa { get; set; }
        public ICommand LenhLuu { get; set; }

        public HoSoKhamViewModel()
        {
            DSPhieuKham = new ObservableCollection<PHIEUKHAM>();
            DSBenhNhan = new ObservableCollection<BENHNHAN>(); // ✅ Khởi tạo danh sách nguồn
            LoadData();

            // 1. LỆNH THÊM: Sinh mã tự động PK + 4 chữ số tăng dần (Tổng 6 ký tự)
            LenhThem = new RelayCommand(p =>
            {
                if (TenNutThem == "Thêm Mới")
                {
                    ResetForm();
                    IsSua = true;
                    TenNutThem = "Hủy Bỏ";
                    SelectedPhieuKham = null;

                    // Ghi nhận ngày nhập ngay tại thời điểm vừa nhấn Thêm
                    _ngayVuaNhanThem = DateTime.Now;

                    // TỰ ĐỘNG SINH MÃ TĂNG DẦN: PK + 4 số (Ví dụ: PK0001, PK0002,...)
                    using (var context = new QL_PHONG_KHAM())
                    {
                        int soPhieuTiepTheo = 1;

                        // Lấy danh sách tất cả các mã phiếu hiện tại có cấu trúc bắt đầu bằng "PK"
                        var danhSachMa = context.PHIEUKHAMs
                                                .Select(x => x.MA_PHIEUKHAM)
                                                .ToList();

                        if (danhSachMa.Any())
                        {
                            // Lọc các mã hợp lệ, cắt bỏ chữ "PK" và chuyển thành số để tìm số lớn nhất hiện tại
                            var danhSachSo = danhSachMa
                                .Where(m => m != null && m.Trim().StartsWith("PK") && m.Trim().Length == 6)
                                .Select(m => {
                                    int.TryParse(m.Trim().Substring(2), out int result);
                                    return result;
                                })
                                .ToList();

                            if (danhSachSo.Any())
                            {
                                soPhieuTiepTheo = danhSachSo.Max() + 1;
                            }
                        }

                        // Định dạng thành chuỗi 6 ký tự: PK + chuỗi số được điền thêm các số 0 ở trước cho đủ 4 chữ số
                        MaPhieuInput = "PK" + soPhieuTiepTheo.ToString("D4");
                    }
                }
                else
                {
                    ResetForm();
                    IsSua = false;
                    TenNutThem = "Thêm Mới";
                }
            });

            // 2. LỆNH SỬA: Chỉ mở khóa trạng thái chỉnh sửa trên giao diện
            LenhSua = new RelayCommand(p =>
            {
                IsSua = true;
                TenNutThem = "Hủy Bỏ";
            }, p => CoChon);

            // 3. LỆNH XÓA: Xóa bản ghi được chọn, không làm ảnh hưởng đến cấu trúc khác
            LenhXoa = new RelayCommand(p =>
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa hồ sơ khám này không?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var context = new QL_PHONG_KHAM())
                    {
                        try
                        {
                            string maPhieu = SelectedPhieuKham.MA_PHIEUKHAM;
                            var item = context.PHIEUKHAMs.FirstOrDefault(x => x.MA_PHIEUKHAM == maPhieu);
                            if (item != null)
                            {
                                // Xóa các ràng buộc phụ thuộc trực tiếp để tránh lỗi xung đột khóa ngoại
                                var hoaDons = context.HOADONs.Where(h => h.MA_PHIEUKHAM == maPhieu);
                                context.HOADONs.RemoveRange(hoaDons);

                                var dichVus = context.PHIEUKHAM_DICHVU.Where(d => d.MA_PHIEUKHAM == maPhieu);
                                context.PHIEUKHAM_DICHVU.RemoveRange(dichVus);

                                context.PHIEUKHAMs.Remove(item);
                                context.SaveChanges();

                                MessageBox.Show("Xóa hồ sơ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                                LoadData();
                                ResetForm();
                                IsSua = false;
                                TenNutThem = "Thêm Mới";
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi khi xóa dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }, p => CoChon);

            // 4. LỆNH LƯU: Thực hiện thêm mới hoặc chỉ cập nhật nội dung thay đổi trên View
            LenhLuu = new RelayCommand(p =>
            {
                if (string.IsNullOrWhiteSpace(MaPhieuInput))
                {
                    MessageBox.Show("Vui lòng không để trống Mã Phiếu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✅ Kiểm tra dữ liệu ComboBox bệnh nhân
                if (BenhNhanDuocChon == null)
                {
                    MessageBox.Show("Vui lòng chọn bệnh nhân cho phiếu khám!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var context = new QL_PHONG_KHAM())
                {
                    try
                    {
                        string trangThaiStr = IsDaHoanThanh ? "Đã hoàn thành" : "Chưa hoàn thành";

                        if (SelectedPhieuKham == null) // CHẾ ĐỘ THÊM MỚI
                        {
                            if (context.PHIEUKHAMs.Any(x => x.MA_PHIEUKHAM == MaPhieuInput))
                            {
                                MessageBox.Show("Mã phiếu khám này đã tồn tại!", "Trùng mã", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // ✅ Rút ngắn logic tìm bệnh nhân: Lấy thẳng trực tiếp ID từ ComboBox được chọn
                            var newPhieu = new PHIEUKHAM
                            {
                                MA_PHIEUKHAM = MaPhieuInput.Trim(),
                                MA_BENHNHAN = BenhNhanDuocChon.MA_BENHNHAN, // Lấy trực tiếp từ đối tượng đang chọn
                                TRANGTHAI = trangThaiStr,
                                // Sử dụng mã của nhân viên đang đăng nhập hệ thống
                                MANV = LoginViewModel.MaNVHienTai ?? "NV01",
                                // Gán lại chính xác ngày giờ lúc bấm nút Thêm Mới trước đó
                                NGAYKHAM = _ngayVuaNhanThem
                            };

                            context.PHIEUKHAMs.Add(newPhieu);
                            context.SaveChanges();
                            MessageBox.Show("Thêm mới hồ sơ thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else // CHẾ ĐỘ CHỈNH SỬA
                        {
                            string maPhieuTarget = SelectedPhieuKham.MA_PHIEUKHAM;
                            var editPhieu = context.PHIEUKHAMs.FirstOrDefault(x => x.MA_PHIEUKHAM == maPhieuTarget);
                            if (editPhieu != null)
                            {
                                // CHỈ ĐỔI nội dung hiển thị trên View (Trạng thái và Bệnh nhân nếu có đổi), còn lại giữ nguyên toàn bộ dữ liệu gốc
                                editPhieu.TRANGTHAI = trangThaiStr;
                                editPhieu.MA_BENHNHAN = BenhNhanDuocChon.MA_BENHNHAN;
                                context.SaveChanges();
                                MessageBox.Show("Cập nhật trạng thái thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        LoadData();
                        ResetForm();
                        IsSua = false;
                        TenNutThem = "Thêm Mới";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }, p => IsSua);
        }

        private void LoadData()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;
            try
            {
                using (var db = new QL_PHONG_KHAM())
                {
                    // 1. Tải danh sách phiếu khám
                    var list = db.PHIEUKHAMs.Include("BENHNHAN").OrderByDescending(x => x.NGAYKHAM).ToList();
                    DSPhieuKham = new ObservableCollection<PHIEUKHAM>(list);

                    // 2. ✅ Tải danh sách bệnh nhân cho nguồn ComboBox
                    var listBN = db.BENHNHANs.OrderBy(x => x.TEN_BENHNHAN).ToList();
                    DSBenhNhan = new ObservableCollection<BENHNHAN>(listBN);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                DSPhieuKham = new ObservableCollection<PHIEUKHAM>();
                DSBenhNhan = new ObservableCollection<BENHNHAN>();
            }
        }

        private void ResetForm()
        {
            MaPhieuInput = "";
            BenhNhanDuocChon = null; // ✅ Reset đối tượng được chọn trong combobox
            IsChuaHoanThanh = true;
            IsDaHoanThanh = false;
            SelectedPhieuKham = null;
        }
    }
}
