using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Repository;
using Restaurant.Utility;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class StoriesController : Controller
    {
        private readonly DataContext _dataContext;
        private const string CartSessionName = "CartSession";
        private const string WishlistCookieName = "wishlist";

        public StoriesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // GET: Stories/Index
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;

            var blogs = await _dataContext.blog
                .Where(b => b.status == "ACTIVE")
                .OrderByDescending(b => b.createdDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalBlogs = await _dataContext.blog
                .Where(b => b.status == "ACTIVE")
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            ViewData["NumberCart"] = carts.Count;

            var wishlists = CookieHelper.GetCookie<List<WishlistItemViewModel>>(HttpContext, WishlistCookieName) ?? new List<WishlistItemViewModel>();
            ViewData["NumberWishList"] = wishlists.Count;

            return View(blogs);
        }

        // GET: Stories/Details/{id}
        public async Task<IActionResult> Detail(long id)
        {
            var blog = await _dataContext.blog
                .Include(b => b.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(b => b.id == id);

            if (blog == null)
            {
                return NotFound();
            }

            blog.ViewCount++;
            _dataContext.Update(blog);
            await _dataContext.SaveChangesAsync();

            // Ensure that comments are loaded correctly
            if (blog.Comments != null)
            {
                // Debugging output (this will show the count of comments)
                Console.WriteLine($"Number of comments: {blog.Comments.Count}");
            }

            return View(blog);
        }


        // POST: Stories/PostComment
        [HttpPost]
        public async Task<IActionResult> PostComment(CommentModel comment)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid && comment.BlogId != 0)
            {
                comment.UserName = User.Identity.Name;
                comment.CreatedDate = DateTime.Now;

                await _dataContext.comment.AddAsync(comment);
                await _dataContext.SaveChangesAsync();

                return RedirectToAction("Detail", new { id = comment.BlogId });
            }

            return RedirectToAction("Detail", new { id = comment.BlogId });
        }

        private async Task DeleteCommentAndReplies(CommentModel comment)
        {
            var replies = await _dataContext.comment
                .Where(c => c.ParentCommentId == comment.CommentId)
                .ToListAsync();

            foreach (var reply in replies)
            {
                await DeleteCommentAndReplies(reply);
            }

            _dataContext.comment.Remove(comment);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(long commentId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = await _dataContext.comment
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return RedirectToAction("Index");
            }

            if (comment.UserName != User.Identity.Name && !User.IsInRole("Admin"))
            {
                return Unauthorized();
            }

            var blogId = comment.BlogId;

            await DeleteCommentAndReplies(comment);
            await _dataContext.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = blogId });
        }

        // POST: Stories/PostReply
        [HttpPost]
        public async Task<IActionResult> PostReply(long? parentCommentId, long blogId, string content)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Content is required.");
            }

            var reply = new CommentModel
            {
                UserName = User.Identity.Name,
                Content = content,
                BlogId = blogId,
                CreatedDate = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _dataContext.comment.Add(reply);

            try
            {
                await _dataContext.SaveChangesAsync();
                return RedirectToAction("Detail", new { id = blogId });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error posting reply: {ex.InnerException?.Message}");
            }
        }
    }
}
