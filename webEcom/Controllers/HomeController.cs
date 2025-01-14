using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using webEcom.Data;
using webEcom.Models;

namespace webEcom.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly webEcomDBContext _context;

        public HomeController(webEcomDBContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // ดึงสินค้าล่าสุด
            var recentProducts = _context.PRODUCT
                .OrderByDescending(p => p.ProductCreateTime)
                .Take(6)
                .ToList();

            // ดึงสินค้าแนะนำ
            var recommendedProducts = _context.PRODUCT
                .Where(p => p.ProductTag != null && p.ProductTag.Contains("สินค้าแนะนำ"))
                .OrderByDescending(p => p.ProductCreateTime)
                .Take(6)
                .ToList();

            // ดึงประเภทสินค้า (ไม่ซ้ำกัน)
            var categories = _context.PRODUCT
                .Select(p => p.ProductType)
                .Distinct()
                .Where(p => p != null) // กรองประเภทสินค้าที่ไม่เป็น null
                .ToList();

            // รวมข้อมูลทั้งหมดใน ViewModel
            var viewModel = new HomeViewModel
            {
                RecentProducts = recentProducts,
                RecommendedProducts = recommendedProducts,
                Categories = categories
            };

            // ส่ง ViewModel ไปยัง View
            return View(viewModel);
        }
    }
}