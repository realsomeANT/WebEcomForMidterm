namespace webEcom.Models
{
    public class HomeViewModel
    {
        public IEnumerable<ProductDATA>? RecentProducts { get; set; }
        public IEnumerable<ProductDATA>? RecommendedProducts { get; set; }
        public IEnumerable<string>? Categories { get; set; }
    }
}
