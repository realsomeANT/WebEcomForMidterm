using System.Collections.Generic;

namespace webEcom.Models
{
    public class CartViewModel
    {
        // รายการสินค้าที่อยู่ในตะกร้า
        public required IEnumerable<ProductDATA> CartItems { get; set; }

        // ราคาสินค้าทั้งหมดในตะกร้า
        public float TotalPrice { get; set; }
    }
}
