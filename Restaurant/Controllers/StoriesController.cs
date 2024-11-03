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

        public StoriesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // GET: Stories/Index
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6; // Set the number of blogs per page

            // Retrieve the list of active blogs ordered by creation date and apply pagination
            var blogs = await _dataContext.blog
                .Where(b => b.status == "ACTIVE") // Filter based on status if applicable
                .OrderByDescending(b => b.createdDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get the total number of active blogs for pagination
            var totalBlogs = await _dataContext.blog
                .Where(b => b.status == "ACTIVE")
                .CountAsync();

            // Calculate the total number of pages
            var totalPages = (int)System.Math.Ceiling((double)totalBlogs / pageSize);

            // Pass the current page and total pages to the view for pagination controls
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Retrieve the cart from session or initialize an empty list if none exists
            var carts = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionName) ?? new List<CartItemViewModel>();
            // Set the cart count in ViewData
            ViewData["NumberCart"] = carts.Count;

            return View(blogs);
        }

        // GET: Stories/Details/{id}
        public async Task<IActionResult> Detail(long id)
        {
            // Fetch the blog entry
            var blog = await _dataContext.blog
                .Include(b => b.Comments) // Include comments to display them in the view
                    .ThenInclude(c => c.Replies) // Optionally include replies
                .FirstOrDefaultAsync(b => b.id == id);

            // If the blog doesn't exist, return a 404 error
            if (blog == null)
            {
                return NotFound();
            }

            // Increment the view count
            blog.ViewCount++;
            _dataContext.Update(blog);
            await _dataContext.SaveChangesAsync();

            // Return the blog view with comments
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

            if (ModelState.IsValid)
            {
                if (comment.BlogId == 0)
                {
                    ModelState.AddModelError("BlogId", "Invalid BlogId.");
                    return RedirectToAction("Detail", "Stories", new { id = comment.BlogId });
                }

                // Set properties for the new comment
                comment.UserName = User.Identity.Name;
                comment.CreatedDate = DateTime.Now;
                comment.ParentCommentId = null; // Ensure this is a top-level comment

                // Add the comment to the database
                await _dataContext.comment.AddAsync(comment);
                await _dataContext.SaveChangesAsync();

                // Redirect back to the blog detail view
                return RedirectToAction("Detail", "Stories", new { id = comment.BlogId });
            }

            // If model state is not valid, redirect back with errors
            return RedirectToAction("Detail", "Stories", new { id = comment.BlogId });
        }

        private async Task DeleteCommentAndReplies(CommentModel comment)
        {
            // Get all replies for the current comment
            var replies = await _dataContext.comment
                .Where(c => c.ParentCommentId == comment.CommentId)
                .ToListAsync();

            // Recursively delete each reply
            foreach (var reply in replies)
            {
                await DeleteCommentAndReplies(reply);
            }

            // Finally, delete the current comment
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

            // Delete the comment and all its replies recursively
            await DeleteCommentAndReplies(comment);
            await _dataContext.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = blogId });
        }

        [HttpPost]
        public async Task<IActionResult> PostReply(long parentCommentId, long blogId, string content)
        {
            // Ensure the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate the content
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Content is required.");
            }

            // Retrieve the parent comment from the database
            var parentComment = await _dataContext.comment
                .Include(c => c.Replies) // Include the Replies navigation property
                .FirstOrDefaultAsync(c => c.CommentId == parentCommentId);

            if (parentComment == null)
            {
                return NotFound("Parent comment not found.");
            }

            // Create a new reply comment
            var reply = new CommentModel
            {
                UserName = User.Identity.Name, // Use the authenticated user's name
                Content = content,
                BlogId = blogId,
                ParentCommentId = parentCommentId, // Set the parent comment ID
                CreatedDate = DateTime.Now // Set the creation date
            };

            // Add the reply to the parent's Replies list (this part might not be necessary if you're directly adding the reply)
            parentComment.Replies.Add(reply);

            try
            {
                // Save changes to the database
                await _dataContext.SaveChangesAsync();
                // Redirect back to the blog detail view
                return RedirectToAction("Detail", "Stories", new { id = blogId });
            }
            catch (DbUpdateException ex)
            {
                // Log the error (you can implement logging here)
                return BadRequest($"Error posting reply: {ex.InnerException?.Message}");
            }
        }

    }
}