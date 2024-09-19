using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Repository.Components
{
    public class CategoriesViewComponents : ViewComponent
    {
        private readonly DataContext _dataContext;
        public CategoriesViewComponents(DataContext Context)
        {
            _dataContext = Context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Category.ToListAsync());
    }
}
