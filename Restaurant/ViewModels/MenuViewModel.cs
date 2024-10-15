using X.PagedList;
using Restaurant.Models;

namespace Restaurant.ViewModels
{
    public class MenuViewModel
    {
        public IEnumerable<CategoryModel> category { get; set; }
        public IPagedList<DishModel> dish { get; set; }  
    }
}
