using Microsoft.EntityFrameworkCore;

namespace webEcom.Data
{
    public class webEcomDBContext : DbContext
    {
        public webEcomDBContext() 
        {
        }

        public webEcomDBContext(DbContextOptions<webEcomDBContext> options) : base(options) 
        {
        }


        public DbSet<webEcom.Models.ProductDATA> PRODUCT { get;set; }


    }
}
