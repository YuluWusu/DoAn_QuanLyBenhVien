using DoAn_QuanLyBenhVien.Models;
using DoAn_QuanLyBenhVien.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DoAn_QuanLyBenhVien.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DanhSachBN _sharedDsVM;
        private HoSoKham _sharedHoSoVM;
        public MainWindow()
        {
            InitializeComponent();

            //quản lý UI chung
            this.DataContext = new DoAn_QuanLyBenhVien.ViewModels.TrangChu();

            // Khởi tạo hai ViewModel dùng chung cho bệnh nhân và phiếu khám
            _sharedDsVM = new DanhSachBN();
            _sharedHoSoVM = new HoSoKham();

            // Sao chép danh sách bệnh nhân hiện có sang danh sách chờ khám trong Hồ sơ
            _sharedHoSoVM.DSBNChuaKham = new ObservableCollection<BenhNhan>(_sharedDsVM.DSBN);

            //Chuyển dữ liệu từ bệnh nhân qua phiếu khám
            _sharedDsVM.DSBN.CollectionChanged += (s, e) =>
            {

                // Trường hợp THÊM bệnh nhân mới vào danh sách tổng
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (BenhNhan newBN in e.NewItems)
                    {
                        _sharedHoSoVM.DSBNChuaKham.Add(newBN);
                    }
                }

                // Trường hợp XÓA bệnh nhân khỏi danh sách tổng
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (BenhNhan oldBN in e.OldItems)
                    {
                        _sharedHoSoVM.DSBNChuaKham.Remove(oldBN);
 
                        var recordsToRemove = _sharedHoSoVM.DSLichSuKham
                            .Where(item =>
                            {
                                dynamic d = item;
                                return d.BN.MaBenhNhan == oldBN.MaBenhNhan;
                            }).ToList();

                        foreach (var record in recordsToRemove)
                        {
                            _sharedHoSoVM.DSLichSuKham.Remove(record);
                        }
                    }
                }
            };
        }
        //Chuyển qua UC_HoSoKham
        private void TreeItem_HoSoKham_Selected(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as DoAn_QuanLyBenhVien.ViewModels.TrangChu;
            if (vm != null)
            {
                var hoSoView = new UC_HoSoKham();
                hoSoView.DataContext = _sharedHoSoVM;
                vm.ChuyenTrang(hoSoView, "Hồ sơ khám bệnh", "Bệnh nhân > Hồ sơ");
            }
            e.Handled = true;
        }
        //Chuyển qua UC_DanhSachBN
        private void TreeItem_Selected(object sender, RoutedEventArgs e)
        {
            var vmTrangChu = this.DataContext as DoAn_QuanLyBenhVien.ViewModels.TrangChu;
            if (vmTrangChu != null)
            {
                var dsView = new UC_DanhSachBN();
                dsView.DataContext = _sharedDsVM;

                var hosoView = new UC_HoSoKham();
                hosoView.DataContext = _sharedHoSoVM;

                _sharedDsVM.YeuCauChuyenTrangHoSo = (bn) => {
                    _sharedHoSoVM.BNDangChonVaoKham = _sharedHoSoVM.DSBNChuaKham.FirstOrDefault(x => x.MaBenhNhan == bn.MaBenhNhan);
                    _sharedHoSoVM.SelectedTabIndex = 0;

                    hosoView.DataContext = _sharedHoSoVM;
                    vmTrangChu.ChuyenTrang(hosoView, "Hồ sơ khám bệnh", "Bệnh nhân > Lập phiếu");
                };

                vmTrangChu.ViewHienTai = dsView;
            }
            e.Handled = true;
        }
    }
}
