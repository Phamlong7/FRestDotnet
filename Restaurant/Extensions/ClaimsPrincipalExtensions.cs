using System.Security.Claims;

namespace Restaurant.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Kiểm tra xem người dùng đăng nhập có phải admin hay không
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
