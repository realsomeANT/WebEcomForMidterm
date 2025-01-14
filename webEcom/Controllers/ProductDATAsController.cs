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
using webEcom.ViewModels;
using System.Net.Mail;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Net;
using System.Net.Mime;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Configuration;


namespace webEcom.Controllers
{
    public class ProductDATAsController : Controller
    {
        private readonly webEcomDBContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IConfiguration _configuration;



        public ProductDATAsController(webEcomDBContext context, UserManager<Users> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }
        public IActionResult Index(string query, string sortOrder, string productTag, string productType, float? minPrice, float? maxPrice, int pageNumber = 1, int pageSize = 6)
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
            products = products.Where(p => p.ProductCount > 0 && p.ProductStatus == "Market");

            // เงื่อนไขการค้นหา
            if (!string.IsNullOrEmpty(query))
            {
                products = products.Where(p =>
                    (p.ProductName != null && p.ProductName.Contains(query)) ||
                    (p.ProductType != null && p.ProductType.Contains(query)) ||
                    (p.ProductTag != null && p.ProductTag.Contains(query))
                );
            }

            // เงื่อนไขการฟิลเตอร์
            // เงื่อนไขการฟิลเตอร์ (Tag และ Type)
            if (!string.IsNullOrEmpty(productTag))
            {
                products = products.Where(p => p.ProductTag != null && p.ProductTag.Contains(productTag));
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

        public async Task<IActionResult> ProductStatus()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var products = _context.PRODUCT
                .Where(p => p.ProductUserTel == currentUser.UserPhoneNumber.ToString()
                            && p.ProductEmail == currentUser.Email
                            && (p.ProductStatus == "รอตรวจสอบ"
                                || p.ProductStatus == "ชำระแล้ว"
                                || p.ProductStatus == "ส่งแล้ว"))
                .ToList();

            return View(products);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProductStatus(int productId)
        {
            var product = await _context.PRODUCT.FindAsync(productId);
            if (product != null)
            {
                product.ProductStatus = "ส่งสำเร็จแล้ว";
                product.ProductSendTimesuccess = DateTime.Now;
                _context.Update(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ProductStatus));
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
        [ValidateAntiForgeryToken]

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
                product.ProductUserTel = currentUser.UserPhoneNumber.ToString();
                product.ProductUserName = $"{currentUser.UserFirstName} {currentUser.UserSurname}";
                product.ProductUserAddress = currentUser.UserAddress;
                product.ProductStatus = "In Cart";  
                product.ProductUserCart = DateTime.Now;
                product.ProductEmail = currentUser.Email;
                _context.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction("Cart"); // นำไปหน้าตะกร้า
            }

            return RedirectToAction("Login", "Account"); // หากยังไม่ล็อกอิน
        }

        public IActionResult Cart()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _context.PRODUCT
                .Where(p => p.ProductUserName == $"{currentUser.UserFirstName} {currentUser.UserSurname}" && p.ProductStatus == "In Cart")
                .ToList();

            var totalPrice = cartItems.Sum(item => item.ProductPrice.GetValueOrDefault() * item.ProductCount.GetValueOrDefault());

            var viewModel = new CartViewModel
            {
                CartItems = cartItems,
                TotalPrice = totalPrice
            };

            return View(viewModel);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var product = await _context.PRODUCT.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                product.ProductUserTel = null;
                product.ProductUserName = null;
                product.ProductUserAddress = null;
                product.ProductStatus = "Market";
                product.ProductUserCart = null;
                product.ProductEmail = null;
                _context.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction("Cart");
            }

            return RedirectToAction("Login", "Account");
        }


        public async Task<IActionResult> Details2(int? id)
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

        /*
        private async Task SendOrderEmail(Users user, List<ProductDATA> cartItems)
        {
            var email = _configuration["SmtpSettings:Email"] ?? throw new ArgumentNullException("SmtpSettings:Email");
            var fromAddress = new MailAddress(email, "RyuChuKo");
            var toAddress = new MailAddress(user.Email ?? throw new ArgumentNullException(nameof(user.Email)),
                                            $"{user.UserFirstName ?? string.Empty} {user.UserSurname ?? string.Empty}");
            string fromPassword = _configuration["SmtpSettings:Password"] ?? throw new ArgumentNullException("SmtpSettings:Password"); // ใส่รหัสผ่านอีเมลของคุณ
            const string subject = "ใบสั่งซื้อสินค้า";
          


            var body = "<h1 style='text-align:center;'>รายการสินค้าที่คุณสั่งซื้อ:</h1><br/>";
            float totalPrice = 0;

            body += "<table style='width:100%; border-collapse: collapse;'font-size: 10px;>"
                 + "<tr>"
                 + "<th style='border: 1px solid black; padding: 8px;'>ชื่อสินค้า</th>"
                 + "<th style='border: 1px solid black; padding: 8px;'>ราคา (บาท)</th>"
                 + "</tr>";

            foreach (var item in cartItems)
            {
                body += "<tr>"
                      + $"<td style='border: 1px solid black; padding: 8px;'>{item.ProductName}</td>"
                      + $"<td style='border: 1px solid black; padding: 8px;'>{item.ProductPrice}</td>"
                      + "</tr>";
                totalPrice += item.ProductPrice ?? 0;
            }

            body += "</table>";
            body += $"<h2 >ราคาสินค้ารวมทั้งหมด: {totalPrice} บาท</h2>";
            body += $"<h2 >ชื่อผู้สั่งซื้อ: {user.UserFirstName} {user.UserSurname}</h2>";
            body += $"<h2 >เบอร์โทรศัพท์: {user.UserPhoneNumber}</h2>";
            body += $"<h2 >ที่อยู่: {user.UserAddress}</h2>";

            body += $"<h2 style='text-align:center;'>วิธีการชำระเงิน</h2>";
            body += $"<h4 >1. สแกน qr code เพื่อทำการจ่ายเงิน</h4>";
            body += $"<h4 >2. กรอกจำนวนเงินที่ต้องจ่ายตามราคาสินค้ารวมทั้งหมด</h4>";
            body += $"<h4 >3. โปรดตรวจสอบรายละเอียดต่าง ๆ ให้ถูกต้อง ** คำเตือน : กรอกจำนวนเงินให้ถูกต้องถ้ากรอกเกินหรือขาดจะไม่มีการโอนเงินคืนแต่อย่างไร **</h4>";
            body += $"<h4 >4. กดยืนยันการชำระเงิน</h4>";
            body += $"<h4 >5. ส่งหลักฐานการโอนเงิน โดยการตอบกลับเมลและแนบรูปสลิปหลักฐานการโอนเงินกลับมา</h4>";
            body += $"<img src='cid:slipImage' alt='ขั้นตอนการส่งสลิป' style='display:block;width:65vw; '/>";
            body += $"<h4 >6. รอระบบตรวจสอบหลักฐานการโอนเงิน 1-2 วัน เมื่อตรวจสอบผ่านระบบจะส่งใบเสร็จรับเงินกลับมาให้</h4>";
            body += $"<h4 >7. สินค้าจะถูกจัดส่งภายใน 3-5 วันทำการหลังจากส่งใบเสร็จกลับมาให้ ** สามารถตรวจสอบเลขพัสดุได้ในเว็บที่สถานะพัสดุ ** </h4>";
            body += $"<img src='cid:QR' alt='QRcode' style='display:block; '/>";
            body += $"<h2 style='text-align:center;'>หรือชะระทางช่องทางอื่น</h2>";
            body += $"<h4 >1. โอนเงินผ่านบัญชีธนาคาร</h4>";
            body += $"<h5 >   | เลขบัญชี : XXXXXXXXXXXXX ธนาคราร : XXXXXXXX </h4>";
            body += $"<h4 >2. โอนเงินผ่านทรูมันนี่วอลเล็ท</h4>";
            body += $"<h5 >   | เบอร์ : XXXXXXXXXX</h4>";


            var pdfBytes = GenerateInvoicePdf(cartItems);


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // ใช้ SMTP server ของ KMUTT
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            var slipImage = new LinkedResource("wwwroot/images/1.png", "image/png")
            {
                ContentId = "slipImage"
            };

            var QR = new LinkedResource("wwwroot/images/1732786927531.png", "image/png")
            {
                ContentId = "QR"
            };

            var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(slipImage);
            alternateView.LinkedResources.Add(QR);

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true // ระบุว่าเนื้อหาอีเมลเป็น HTML
            })
            {
                message.AlternateViews.Add(alternateView);

                // Attach PDF invoice
                var attachment = new Attachment(new MemoryStream(pdfBytes), "ใบแจ้งหนี้.pdf", MediaTypeNames.Application.Pdf);
                message.Attachments.Add(attachment);

                await smtp.SendMailAsync(message);
            }
        }
        */

        private byte[] GenerateInvoicePdf(List<ProductDATA> products)
        {
            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Load Thai font
                var baseFont = BaseFont.CreateFont("wwwroot/fonts/THSarabunNew.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var titleFont = new Font(baseFont, 18, Font.BOLD);
                var dateFont = new Font(baseFont, 12, Font.NORMAL);

                // Add store logo
                var logo = Image.GetInstance("wwwroot/images/Logo1.png");
                logo.ScaleToFit(150f, 150f);
                logo.Alignment = Element.ALIGN_RIGHT;
                document.Add(logo);

                // Add invoice title
                var title = new Paragraph("ใบแจ้งหนี้", titleFont)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                document.Add(title);

                // Add invoice date
                var date = new Paragraph($"Date: {DateTime.Now:dd/MM/yyyy}", dateFont)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                document.Add(date);

                // Add store information
                var storeInfo = new Paragraph("RyuChuKo\nร้านนี้ตั้งขึ้นมาเพื่อการศึกษาไม่ได้มีอยู่จริง\nสร้างขึ้นมาเพื่อจำลองเป็นเว็บร้านขายสินค้าญี่ปุ่นมือสองออนไลน์", dateFont)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                document.Add(storeInfo);

                // Add customer information
                var customerInfo = new Paragraph($"ชื่อผู้รับสินค้า: {products.First().ProductUserName}\n" +
                                                 $"เบอร์โทรศัพท์: {products.First().ProductUserTel}\n" +
                                                 $"Email: {products.First().ProductEmail}\n" +
                                                 $"ที่อยู่ผู้รับสินค้า: {products.First().ProductUserAddress}", dateFont)
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingBefore = -60f // ปรับค่าตามความเหมาะสม
                };
                document.Add(customerInfo);

                // Add a line separator
                document.Add(new Paragraph("\n"));

                // Add product table
                var table = new PdfPTable(3) { WidthPercentage = 100 };

                // Define the border width
                float borderWidth = 1f; // ปรับค่าความหนาของเส้นขอบตามต้องการ

                // Add header cells with increased border width
                var headerCell1 = new PdfPCell(new Phrase("ชื่อสินค้า", dateFont)) { BorderWidth = borderWidth };
                var headerCell2 = new PdfPCell(new Phrase("จำนวน", dateFont)) { BorderWidth = borderWidth };
                var headerCell3 = new PdfPCell(new Phrase("ราคา (บาท)", dateFont)) { BorderWidth = borderWidth };
                float totalPrice = 0;

                table.AddCell(headerCell1);
                table.AddCell(headerCell2);
                table.AddCell(headerCell3);

                // Add product rows with increased border width
                foreach (var product in products)
                {
                    var cell1 = new PdfPCell(new Phrase(product.ProductName, dateFont)) { BorderWidth = borderWidth };
                    var cell2 = new PdfPCell(new Phrase(product.ProductCount.ToString(), dateFont)) { BorderWidth = borderWidth };
                    var cell3 = new PdfPCell(new Phrase(product.ProductPrice.ToString(), dateFont)) { BorderWidth = borderWidth };

                    table.AddCell(cell1);
                    table.AddCell(cell2);
                    table.AddCell(cell3);

                    // Calculate total price
                    totalPrice += product.ProductPrice.GetValueOrDefault() * product.ProductCount.GetValueOrDefault();
                }

                // Add total price row with increased border width
                var totalCell1 = new PdfPCell(new Phrase("", dateFont)) { BorderWidth = borderWidth };
                var totalCell2 = new PdfPCell(new Phrase("ราคารวมทั้งหมด", dateFont)) { BorderWidth = borderWidth };
                var totalCell3 = new PdfPCell(new Phrase(totalPrice.ToString(), dateFont)) { BorderWidth = borderWidth };

                table.AddCell(totalCell1);
                table.AddCell(totalCell2);
                table.AddCell(totalCell3);

                document.Add(table);

                // Add a line separator
                document.Add(new Paragraph("\n"));

                // Add issuer information
                var issuerInfo = new Paragraph("ผู้ออกใบเสร็จ\nXXXXXXXXXXX", dateFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 40f
                };
                document.Add(issuerInfo);

                var paayInfo = new Paragraph("วิธีการจ่ายเงิน ** คำเตือนโปรดจ่ายภายใน 15 วัน **\n1. สแกนจ่ายทาง QRcode", dateFont)
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingBefore = 60f
                };
                document.Add(paayInfo);

                var logoQR = Image.GetInstance("wwwroot/images/LogoQR.png");
                logoQR.ScaleToFit(80f, 80f);
                logoQR.Alignment = Element.ALIGN_LEFT;
                document.Add(logoQR);

                var paayInfo1 = new Paragraph("2. โอนเงินผ่านบัญชีธนาคาร\n| เลขบัญชี : XXXXXXXXXXXXX ธนาคราร : XXXXXXXX\n3. โอนเงินผ่านทรูมันนี่วอลเล็ท\n| เบอร์ : XXXXXXXXXX\n** โปรดส่งหลักฐานการโอนเงินมาที่ < xxxxxxxxxxxxxxxxx@gmail.com > เพื่อให้ทางร้านตรวจสอบการโอนเงิน", dateFont)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(paayInfo1);

                var thankYouNote = new Paragraph("ร้านนี้ตั้งขึ้นมาเพื่อการศึกษาไม่ได้มีอยู่จริง", dateFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 40f
                };
                document.Add(thankYouNote);

                document.Close();
                return memoryStream.ToArray();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOrderEmail()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _context.PRODUCT
                .Where(p => p.ProductUserName == $"{currentUser.UserFirstName} {currentUser.UserSurname}" && p.ProductStatus == "In Cart")
                .ToList();

            if (cartItems.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var pdfBytes = GenerateInvoicePdf(cartItems);

            // Update product status after sending email
            foreach (var item in cartItems)
            {
                item.ProductStatus = "รอตรวจสอบ";
                item.ProductSendBill = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                _context.Update(item);
            }

            await _context.SaveChangesAsync();

            // Save the PDF to a temporary location
            var filePath = Path.Combine(Path.GetTempPath(), "Invoice.pdf");
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            // Return the file URL as JSON
            return Json(new { fileUrl = Url.Action("DownloadInvoice", new { filePath }) });
        }

        public IActionResult DownloadInvoice(string filePath)
        {
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", "Invoice.pdf");
        }

        // เพิ่มโค้ดเพื่อ reload หน้าเว็บหลังจากทำงานเสร็จ






        /*

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SendOrderEmail()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _context.PRODUCT
                .Where(p => p.ProductUserName == $"{currentUser.UserFirstName} {currentUser.UserSurname}" && p.ProductStatus == "In Cart")
                .ToList();

            await SendOrderEmail(currentUser, cartItems);

            // อัปเดตสถานะของสินค้าหลังจากส่งอีเมลแล้ว
            foreach (var item in cartItems)
            {
                item.ProductStatus = "รอตรวจสอบ";
                item.ProductSendBill = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                _context.Update(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Cart");
        }

        */

        private bool ProductDATAExists(int id)
        {
            return _context.PRODUCT.Any(e => e.IdProduct == id);
        }
    }
}