using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using webEcom.Data;
using webEcom.Models;

public class CartExpiryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
  
    public CartExpiryService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
      
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<webEcomDBContext>();

                var expiredProducts = context.PRODUCT
                    .Where(p => p.ProductStatus == "In Cart" && p.ProductUserCart.HasValue &&
                                p.ProductUserCart.Value.AddMinutes(30) < DateTime.Now)
                    .ToList();

                foreach (var product in expiredProducts)
                {
                    product.ProductStatus = "Market";
                    product.ProductUserCart = null;
                    product.ProductUserTel = null;
                    product.ProductEmail = null;
                    product.ProductUserName = null;
                    product.ProductUserAddress = null;

                }

                context.UpdateRange(expiredProducts);
                await context.SaveChangesAsync();
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // ตรวจสอบทุกๆ 5 นาที
        }
    }
}
