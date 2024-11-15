using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webEcom.Models;
using webEcom.ViewModels;

namespace webEcom.Data
{
    public class _IdentityDbContext : IdentityDbContext<Users>
    {
        public _IdentityDbContext(DbContextOptions<_IdentityDbContext> options) 
            : base(options) { }
        public DbSet<webEcom.ViewModels.RegisterViewModel> RegistorViewModel { get; set; } = default!;
    }
}   
