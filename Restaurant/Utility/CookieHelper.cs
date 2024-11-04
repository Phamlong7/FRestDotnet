using Newtonsoft.Json;

namespace Restaurant.Utility
{
    public static class CookieHelper
    {
        public static void SetCookie<T>(HttpContext context, string key, T value, int expireTimeInMinutes)
        {
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(expireTimeInMinutes)
            };
            var jsonData = JsonConvert.SerializeObject(value);
            context.Response.Cookies.Append(key, jsonData, options);
        }

        public static T? GetCookie<T>(HttpContext context, string key)
        {
            var cookie = context.Request.Cookies[key];
            return string.IsNullOrEmpty(cookie) ? default : JsonConvert.DeserializeObject<T>(cookie);
        }

        public static void RemoveCookie(HttpContext context, string key)
        {
            context.Response.Cookies.Delete(key);
        }
    }
}
