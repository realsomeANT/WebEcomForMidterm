using  System.ComponentModel.DataAnnotations;
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

        [Display(Name = "Email ผู้ซื้อสินค้า")]
        public string? ProductEmail { get; set; }

        [Display(Name = "ชื่อผู้ซื้อสินค้า")]
        public string? ProductUserName { get; set; }


        [Display(Name = "ที่อยู่ผู้ซื้อสินค้า")]
        public string? ProductUserAddress { get; set; }



        [Display(Name = "วันที่ลงสินค้า")]
        public DateTime? ProductCreateTime { get; set; } = DateTime.Now;

        [Display(Name = "เวลาที่ลูกค้านำสินค้าใส่ตะกล้า")]
        public DateTime? ProductUserCart { get; set; }


        [Display(Name = "เวลาที่ส่งใบสั่งซื้อ")]
        public DateTime? ProductSendBill { get; set; }

        [Display(Name = "เวลาที่ส่งบิล")]
        public DateTime? ProductBill { get; set; }


        [Display(Name = "เวลาที่ส่งสินค้า")]
        public DateTime? ProductSendTime { get; set; }

        [Display(Name = "เวลาที่ส่งสินค้า")]
        public DateTime? ProductSendTimesuccess { get; set; }

        [Display(Name = "เลขพัสดุ")]
        public string? ProductSendNumber { get; set; }

       

    }
}
