// Models/AdminController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using webEcom.Data;
using webEcom.Models;
using Microsoft.VisualBasic;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Net;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.Extensions.Configuration;



namespace webEcom.Controllers
{
    public class AdminController : Controller
    {

        private readonly webEcomDBContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IConfiguration _configuration;


        public AdminController(webEcomDBContext context, UserManager<Users> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
             
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.PRODUCT.AsNoTracking().ToListAsync();
            return View(products);
        }


        public IActionResult ProductSendNumber()
        {
            var products = _context.PRODUCT
                .Where(p => p.ProductStatus == "ชำระแล้ว")
                .ToList();
            return View(products);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductSendNumber(Dictionary<int, string> ProductSendNumbers)
        {
            foreach (var item in ProductSendNumbers)
            {
                var product = await _context.PRODUCT.FindAsync(item.Key);
                if (product != null)
                {
                    product.ProductSendNumber = item.Value;

                    if(product.ProductSendNumber != null)
                    {
                        product.ProductStatus = "ส่งแล้ว";
                        product.ProductSendTime = DateTime.Now;
                    }
                    
                    _context.Update(product);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProductSendNumber));
        }



        public IActionResult bill()
        {
            var pRODUCTS = _context.PRODUCT
                .Where(p => p.ProductStatus == "รอตรวจสอบ")
                .GroupBy(p => new { p.ProductUserTel, p.ProductUserName, p.ProductUserAddress, p.ProductEmail ,p.ProductSendBill})
                .Select(g => new
                {
                    UserTel = g.Key.ProductUserTel,
                    UserName = g.Key.ProductUserName,
                    UserAddress = g.Key.ProductUserAddress,
                    Email = g.Key.ProductEmail,
                    SendBill = g.Key.ProductSendBill,
                    Products = g.ToList()
                })
                .ToList(); 

            return View(pRODUCTS);
        }

        [HttpPost]
        public IActionResult DeleteProduct(int productId)
        {
            var product = _context.PRODUCT.Find(productId);
            if (product != null)
            {
                product.ProductStatus = "Market";
                product.ProductUserTel = null;
                product.ProductUserName = null;
                product.ProductUserAddress = null;
                product.ProductEmail = null;
                product.ProductSendBill = null;
                product.ProductUserCart = null;

                _context.SaveChanges();
            }

            return RedirectToAction("bill");
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(string EmailAndSendBill)
        {
            if (string.IsNullOrEmpty(EmailAndSendBill))
            {
                return BadRequest("Email and SendBill are required.");
            }

            var values = EmailAndSendBill.Split('|');
            if (values.Length != 2)
            {
                return BadRequest("Invalid input format.");
            }

            var Email = values[0];
            if (!DateTime.TryParse(values[1], out var sendBill))
            {
                return BadRequest("Invalid SendBill format.");
            }

            var products = await _context.PRODUCT
                .Where(p => p.ProductEmail == Email && p.ProductStatus == "รอตรวจสอบ" && p.ProductSendBill == sendBill)
                .ToListAsync();

            if (products == null || !products.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var product in products)
            {
                product.ProductStatus = "ชำระแล้ว";
                product.ProductBill = DateTime.Now;
                _context.Update(product);
            }

            try
            {
                await _context.SaveChangesAsync();

              
                return RedirectToAction("bill");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /*
        private void SendInvoiceEmail(string email, byte[] invoicePdf, List<ProductDATA> products)
        {
            var fromAddress = new MailAddress(_configuration["SmtpSettings:Email"], "RyuChuKo");
            var toAddress = new MailAddress(email);
            const string subject = "ใบเสร็จสินค้า";
            var body = "<h1 style='text-align:center;'>รายการสินค้าที่คุณสั่งซื้อ:</h1><br/>";
            float totalPrice = 0;

            body += "<table style='width:100%; border-collapse: collapse;'font-size: 10px;>"
                 + "<tr>"
                 + "<th style='border: 1px solid black; padding: 8px;'>ชื่อสินค้า</th>"
                 + "<th style='border: 1px solid black; padding: 8px;'>ราคา (บาท)</th>"
                 + "</tr>";

            foreach (var item in products)
            {
                body += "<tr>"
                      + $"<td style='border: 1px solid black; padding: 8px;'>{item.ProductName}</td>"
                      + $"<td style='border: 1px solid black; padding: 8px;'>{item.ProductPrice}</td>"
                      + "</tr>";
                totalPrice += item.ProductPrice ?? 0;
            }

            body += "</table>";
            body += $"<h2 >ราคาสินค้ารวมทั้งหมด: {totalPrice} บาท</h2>";
            body += $"<h2 >ชื่อผู้สั่งซื้อ: {products.First().ProductUserName}</h2>";
            body += $"<h2 >เบอร์โทรศัพท์: {products.First().ProductUserTel}</h2>";
            body += $"<h2 >ที่อยู่: {products.First().ProductUserAddress}</h2>";





            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, _configuration["SmtpSettings:Password"])
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body ,
                IsBodyHtml = true
            })
            {
                message.Attachments.Add(new Attachment(new MemoryStream(invoicePdf), "ใบเสร็จรับเงิน.pdf"));
                smtp.Send(message);
            }
        }
        */


        public async Task<IActionResult> FilterByMarket()
        {
            var pRODUCTS = await _context.PRODUCT
                .Where(p => p.ProductStatus == "Market")
                .ToListAsync();
            return View("Index", pRODUCTS);
        }

        public async Task<IActionResult> FilterByInCart()
        {
            var pRODUCTS = await _context.PRODUCT
                .Where(p => p.ProductStatus == "In Cart")
                .ToListAsync();
            return View("Index", pRODUCTS);
        }

        public async Task<IActionResult> FilterByรอตรวจสอบ()
        {
            var pRODUCTS = await _context.PRODUCT
                .Where(p => p.ProductStatus == "รอตรวจสอบ")
                .ToListAsync();
            return View("Index", pRODUCTS);
        }

        public async Task<IActionResult> FilterByชำระแล้ว()
        {
            var pRODUCTS = await _context.PRODUCT
                .Where(p => p.ProductStatus == "ชำระแล้ว")
                .ToListAsync();
            return View("Index", pRODUCTS);
        }

        public async Task<IActionResult> FilterByส่งแล้ว()
        {
            var pRODUCTS = await _context.PRODUCT
                .Where(p => p.ProductStatus == "ส่งแล้ว")
                .ToListAsync();
            return View("Index", pRODUCTS);
        }

        public async Task<IActionResult> FilterByส่งสำเร็จแล้ว()
        {
            var pRODUCTS = await _context.PRODUCT
                .Where(p => p.ProductStatus == "ส่งสำเร็จแล้ว")
                .ToListAsync();
            return View("Index", pRODUCTS);
        }

        public IActionResult Search(string querry)
        {
            if (string.IsNullOrEmpty(querry))
            {
                return View("Index", _context.PRODUCT.ToList());
            }

            querry = querry.ToLower();
            var products = _context.PRODUCT.Where(p =>
                (p.ProductType != null && p.ProductType.ToLower().Contains(querry)) ||
                (p.ProductName != null && p.ProductName.ToLower().Contains(querry)) ||
                (p.ProductTag != null && p.ProductTag.ToLower().Contains(querry)) ||
                (p.ProductPrice != null && p.ProductPrice.ToString().Equals(querry))
            ).ToList();

            return View("Index", products);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var pRODUCTS = await _context.PRODUCT
                .FirstOrDefaultAsync(m => m.IdProduct == id);
            if (pRODUCTS == null)
            {
                return NotFound();
            }
            return View(pRODUCTS);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind(" IdProduct,ProductType,ProductName,ProductTag,ProductDescription," +
            "ProductPrice,ProductCount,IformfileProductInputImage,ProductImageType,ProductStatus,ProductCreateTime")] 
             ProductDATA pRODUCTS, IFormFile ProductImage)
        {
            if (ModelState.IsValid)
            {
                if (ProductImage != null && ProductImage.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        ProductImage.CopyTo(memoryStream);
                        pRODUCTS.IformfileProductInputImage = memoryStream.ToArray();

                        pRODUCTS.ProductImageType = ProductImage.ContentType; // เก็บประเภทของรูปภาพ


                    }
                }
                pRODUCTS.ProductStatus = "Market";
                _context.Add(pRODUCTS);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            return View(pRODUCTS);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pRODUCTS = await _context.PRODUCT.FindAsync(id);
            if (pRODUCTS == null)
            {
                return NotFound();
            }

            return View(pRODUCTS);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind(" IdProduct,ProductType,ProductName,ProductTag," +
                                                "ProductDescription,ProductPrice,ProductCount,ProductImage,IformfileProductInputImage," +
                                                "ProductImageType,ProductStatus,")]  ProductDATA pRODUCTS )
        {
            if (id != pRODUCTS.IdProduct)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {


                    if (pRODUCTS.ProductImage != null && pRODUCTS.ProductImage.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            pRODUCTS.ProductImage.CopyTo(memoryStream);
                            pRODUCTS.IformfileProductInputImage = memoryStream.ToArray();
                            pRODUCTS.ProductImageType = pRODUCTS.ProductImage.ContentType; // เก็บประเภทของรูปภาพ
                            pRODUCTS.ProductStatus = "Market";
                        }
                    }

                    else
                    {
                        // Keep the existing image if no new image is uploaded
                        var existingProduct = await _context.PRODUCT.AsNoTracking().FirstOrDefaultAsync(p => p.IdProduct == id);
                        if (existingProduct != null)
                        {
                            pRODUCTS.IformfileProductInputImage = existingProduct.IformfileProductInputImage;
                            pRODUCTS.ProductImageType = existingProduct.ProductImageType;
                            pRODUCTS.ProductStatus = existingProduct.ProductStatus;

                        }
                    }
                    

                    _context.Update(pRODUCTS);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductDATAExists(pRODUCTS.IdProduct))
                    {
                        return NotFound();
                    }
                    else
                    {

                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pRODUCTS);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pRODUCTS = await _context.PRODUCT
                .FirstOrDefaultAsync(m => m.IdProduct == id);
            if (pRODUCTS == null)
            {
                return NotFound();
            }

            return View(pRODUCTS);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfiremed(int id)
        {
            var pRODUCTS = await _context.PRODUCT.FindAsync(id);
            if (pRODUCTS != null)
            {
                _context.PRODUCT.Remove(pRODUCTS);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        private bool ProductDATAExists(int id)
        {
            return _context.PRODUCT.Any(m => m.IdProduct == id);
        }



    }
}
