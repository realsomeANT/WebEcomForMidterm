
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using webEcom.Data;
using webEcom.Models;
using Microsoft.VisualBasic;
using System.Diagnostics;





namespace webEcom.Controllers
{
    public class AdminController : Controller
    {

        private readonly webEcomDBContext _context;


        public AdminController(webEcomDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.PRODUCT.ToListAsync());
        }


        public IActionResult Search(string querry)
        {
            {
                if (string.IsNullOrEmpty(querry))
                {
                    return View("Index", _context.PRODUCT.ToList());
                }
                else
                {
                    return View("Index", _context.PRODUCT.Where(p =>
                    p.ProductType.Contains(querry) ||
                    p.ProductName.Contains(querry) ||
                    p.ProductTag.Contains(querry) ||
                    p.ProductPrice.ToString().Equals(querry)).ToList());

                }


            }

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
