using DoAn_QuanLyBenhVien.Models;
using System.Linq;

namespace DoAn_QuanLyBenhVien.ViewModels
{
    public static class IdGenerator
    {
        // Sinh mã chung: Prefix + Số (VD: BN + 004 -> BN004)
        private static string GenerateNextId(string lastId, string prefix, int numberLength = 3)
        {
            if (string.IsNullOrEmpty(lastId))
            {
                return prefix + 1.ToString().PadLeft(numberLength, '0');
            }

            string numberPart = lastId.Substring(prefix.Length);
            if (int.TryParse(numberPart, out int currentNumber))
            {
                return prefix + (currentNumber + 1).ToString().PadLeft(numberLength, '0');
            }

            return prefix + 1.ToString().PadLeft(numberLength, '0');
        }

        public static string GetNextMaBenhNhan(QL_PHONG_KHAM db)
        {
            var lastBN = db.BENHNHANs
                .OrderByDescending(x => x.MA_BENHNHAN)
                .FirstOrDefault();
            return GenerateNextId(lastBN?.MA_BENHNHAN?.Trim(), "BN", 3);
        }

        public static string GetNextMaThuoc(QL_PHONG_KHAM db)
        {
            var lastT = db.THUOCs
                .OrderByDescending(x => x.MA_THUOC)
                .FirstOrDefault();
            return GenerateNextId(lastT?.MA_THUOC?.Trim(), "T", 3);
        }

        public static string GetNextMaNhanVien(QL_PHONG_KHAM db)
        {
            var lastNV = db.NHANVIENs
                .OrderByDescending(x => x.MaNV)
                .FirstOrDefault();
            return GenerateNextId(lastNV?.MaNV?.Trim(), "NV", 2);
        }

        public static string GetNextMaDichVu(QL_PHONG_KHAM db)
        {
            var lastDV = db.DICHVUs
                .OrderByDescending(x => x.MA_DICHVU)
                .FirstOrDefault();
            return GenerateNextId(lastDV?.MA_DICHVU?.Trim(), "DV", 2);
        }
    }
}
