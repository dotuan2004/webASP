using LTW.Models;
using System;
using System.Linq;

namespace LTW.Controllers
{
    public static class PriceHelper
    {
        public static decimal? GetDiscountedPrice(SanPham product)
        {
            var activePromotion = product.SanPham_KhuyenMais
                .Where(spkm => spkm.KhuyenMai.NgayBatDau <= DateTime.Now
                           && spkm.KhuyenMai.NgayKetThuc >= DateTime.Now)
                .OrderByDescending(spkm => spkm.KhuyenMai.PhanTramGiam)
                .FirstOrDefault()?.KhuyenMai;

            if (activePromotion != null)
            {
                return product.GiaSP * (1 - (decimal)activePromotion.PhanTramGiam / 100);
            }

            return null;
        }

        public static string GetPromotionText(SanPham product)
        {
            var activePromotion = product.SanPham_KhuyenMais
                .Where(spkm => spkm.KhuyenMai.NgayBatDau <= DateTime.Now
                           && spkm.KhuyenMai.NgayKetThuc >= DateTime.Now)
                .OrderByDescending(spkm => spkm.KhuyenMai.PhanTramGiam)
                .FirstOrDefault()?.KhuyenMai;

            if (activePromotion != null)
            {
                return $"Giảm {activePromotion.PhanTramGiam}% đến {activePromotion.NgayKetThuc:dd/MM/yyyy}";
            }

            return null;
        }
    }
}