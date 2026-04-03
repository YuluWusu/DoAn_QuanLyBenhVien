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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoAn_QuanLyBenhVien.Views
{
    /// <summary>
    /// Interaction logic for UC_DanhSachBacSi.xaml
    /// </summary>
    public partial class UC_DanhSachBacSi : UserControl
    {
        public UC_DanhSachBacSi()
        {
            InitializeComponent();
            dgBacSi.ItemsSource = new List<object>
    {
        new { MaNV = "BS001", HoTen = "Nguyễn Ngọc Châu", TenCHUYENKHOA = "Nội Khoa", SoDienThoai = "0901234567" },
        new { MaNV = "BS002", HoTen = "Lê Văn Tám", TenCHUYENKHOA = "Ngoại Khoa", SoDienThoai = "0988777666" }
    };
        }
    }
}
