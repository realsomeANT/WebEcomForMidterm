﻿using  System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace webEcom.Models
{
    public class ProductDATA
    {

        [Key]
        public int IdProduct { get; set; }
        [Required]
        [Display(Name = "ประเภทสินค้า")]

        public string? ProductType { get; set; }
        [Required]
        [Display(Name = "ชื่อสินค้า")]
        public string? ProductName { get; set; }

        [Required]
        [Display(Name = "ป้ายสินค้า")]
        public string? ProductTag { get; set; } 

        [Required]
        [Display(Name = "คำอธิบายสินค้า")]
        public string?   ProductDescription { get; set; }
        [Required]
        [Display(Name = "ราคาสินค้า")]
        public float? ProductPrice { get; set; }

        [Required]
        [Display(Name = "จำนวน")]

        public int? ProductCount { get; set; }


        [NotMapped]
        [Display(Name = "ลงรูปสินค้า")]
        public IFormFile? ProductImage { get; set; }




        [Display(Name = "รูปภาพสินค้า")]
        public byte[]?  IformfileProductInputImage { get; set; }

        [Display(Name = "นามสกุลรูปภาพ")]
        public string? ProductImageType { get; set; }
        [Display(Name = "สถานะสินค้า")]

        public string? ProductStatus { get; set; }
        [Display(Name = "เบอร์โทรผู้ซื้อสินค้า")]
        public string? ProductUserTel { get; set; }
        [Display(Name = "ชื่อผู้ซื้อสินค้า")]

        public string? ProductUserName { get; set; }











    }
}
