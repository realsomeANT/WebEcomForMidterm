using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yot_Login2.Models;
using Yot_Login2.ViewModels;

namespace Yot_Login2.Data
{
    public class _IdentityDbContext : IdentityDbContext<Users>
    {
        public _IdentityDbContext(DbContextOptions options) 
            : base(options) { }
        public DbSet<Yot_Login2.ViewModels.RegisterViewModel> RegistorViewModel { get; set; } = default!;
    }
}
