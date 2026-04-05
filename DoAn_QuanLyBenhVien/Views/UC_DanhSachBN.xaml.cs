using DoAn_QuanLyBenhVien.ViewModels;
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
    /// Interaction logic for UC_DanhSachBN.xaml
    /// </summary>
    public partial class UC_DanhSachBN : UserControl
    {
        public UC_DanhSachBN()
        {
            InitializeComponent();
            this.DataContext = new DanhSachBN();
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
        }

    }
}
