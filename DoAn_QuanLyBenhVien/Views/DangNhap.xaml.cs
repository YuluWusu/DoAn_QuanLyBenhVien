using System;
using System.Collections.Generic;
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
using DoAn_QuanLyBenhVien.ViewModels;
namespace DoAn_QuanLyBenhVien.Views
{
    /// <summary>
    /// Interaction logic for DangNhapDangKy.xaml
    /// </summary>
    public partial class DangNhap : Window
    {
        private LoginViewModel _vm;

        public DangNhap()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            DataContext = _vm;
        }

        private void BtnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            _vm.MatKhau = TxtMatKhau.Password;
            TrangChu main = new TrangChu();
            if (_vm.DangNhap())
            { 
                main.Show();
                this.Close();
            }
        }
    }
}