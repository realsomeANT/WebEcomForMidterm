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
            // �֧�Թ�������ش
            var recentProducts = _context.PRODUCT
                .OrderByDescending(p => p.ProductCreateTime)
                .Take(6)
                .ToList();

            // �֧�Թ����й�
            var recommendedProducts = _context.PRODUCT
                .Where(p => p.ProductTag != null && p.ProductTag.Contains("�Թ����й�"))
                .OrderByDescending(p => p.ProductCreateTime)
                .Take(6)
                .ToList();

            // �֧�������Թ��� (����ӡѹ)
            var categories = _context.PRODUCT
                .Select(p => p.ProductType)
                .Distinct()
                .Where(p => p != null) // ��ͧ�������Թ��ҷ������� null
                .ToList();

            // ��������ŷ������ ViewModel
            var viewModel = new HomeViewModel
            {
                RecentProducts = recentProducts,
                RecommendedProducts = recommendedProducts,
                Categories = categories
            };

            // �� ViewModel ��ѧ View
            return View(viewModel);
        }
    }
}