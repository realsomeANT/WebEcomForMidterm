using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using webEcom.Data;
using webEcom.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace webEcom.Controllers
{
    public class ProductDATAsController : Controller
    {
        private readonly webEcomDBContext _context;
        private readonly UserManager<Users> _userManager;



        public ProductDATAsController(webEcomDBContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index(string query, string sortOrder, string productTag, string productType, float? minPrice, float? maxPrice, int pageNumber = 1, int pageSize = 3)
        {
            // ดึงประเภทสินค้าที่มีอยู่ทั้งหมด (ProductType)
            var allTypes = _context.PRODUCT
                .Where(p => !string.IsNullOrEmpty(p.ProductType))
                .Select(p => p.ProductType)
                .Distinct()
                .ToList();

            // ดึงแท็กทั้งหมด (ProductTag)
            var allTags = _context.PRODUCT
                .Where(p => !string.IsNullOrEmpty(p.ProductTag))
                .Select(p => p.ProductTag)
                .Distinct()
                .ToList();

            // ส่งค่าประเภทและแท็กไปยัง View
            ViewData["AllTypes"] = allTypes;
            ViewData["AllTags"] = allTags;

            var products = _context.PRODUCT.AsQueryable();
            
            // กรองสินค้าที่จำนวนมากกว่า 0
            products = products.Where(p => p.ProductCount > 0);

            // เงื่อนไขการค้นหา
            if (!string.IsNullOrEmpty(query))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(query) ||
                    p.ProductType.Contains(query) ||
                    p.ProductTag.Contains(query)
                );
            }

            // เงื่อนไขการฟิลเตอร์
            // เงื่อนไขการฟิลเตอร์ (Tag และ Type)
            if (!string.IsNullOrEmpty(productTag))
            {
                products = products.Where(p => p.ProductTag.Contains(productTag));
            }

            if (!string.IsNullOrEmpty(productType))
            {
                products = products.Where(p => p.ProductType == productType);
            }

            // เงื่อนไขการกรองช่วงราคา (MinPrice, MaxPrice)
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            // การเรียงลำดับ
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                case "date_asc":
                    products = products.OrderBy(p => p.ProductCreateTime);
                    break;
                case "date_desc":
                    products = products.OrderByDescending(p => p.ProductCreateTime);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }

            // การจัดการ Pagination
            var totalProducts = products.Count();
            products = products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // ส่งค่ากลับไปยัง View
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentQuery"] = query;
            ViewData["CurrentType"] = productType;
            ViewData["CurrentMinPrice"] = minPrice;
            ViewData["CurrentMaxPrice"] = maxPrice;
            ViewData["CurrentTag"] = productTag;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalProducts / (double)pageSize);
            ViewData["CurrentPage"] = pageNumber;

            return View(products.ToList());
        }



        // GET: ProductDATAs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productDATA = await _context.PRODUCT
                .FirstOrDefaultAsync(m => m.IdProduct == id);
            if (productDATA == null)
            {
                return NotFound();
            }

            return View(productDATA);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _context.PRODUCT.FindAsync(productId);

            if (product == null)
            {
                return NotFound(); // หากสินค้าไม่พบ
            }

            var currentUser = await _userManager.GetUserAsync(User); // ดึงข้อมูลผู้ใช้ปัจจุบัน

            if (currentUser != null)
            {
                // บันทึกข้อมูลผู้ซื้อในฟิลด์ของสินค้า
                product.ProductUserTel = currentUser.PhoneNumber;
                product.ProductUserName = $"{currentUser.UserFirstName} {currentUser.UserSurname}";
                product.ProductUserAddress = currentUser.UserAddress;

                product.ProductUserCart = DateTime.Now; // เวลาใส่ตะกร้า (ถ้าต้องการ)
                _context.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction("Cart"); // นำไปหน้าตะกร้า
            }

            return RedirectToAction("Login", "Account"); // หากยังไม่ล็อกอิน
        }

        


        private bool ProductDATAExists(int id)
        {
            return _context.PRODUCT.Any(e => e.IdProduct == id);
        }
    }
}