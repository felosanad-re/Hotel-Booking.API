namespace Hotel_Booking.API.Extensions
{
    public static class CookieExtensions
    {

        public static void SetRefreshToken(
            this HttpResponse response,
            string refreshToken)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/api/auth/refresh-token"
            };

            response.Cookies.Append(
                "refreshToken",
                refreshToken,
                options
                );
        }


        public static string? GetRefreshToken(this HttpRequest request)
        {
            request.Cookies.TryGetValue(
                "refreshToken",
                out var refreshToken
                );

            return refreshToken;
        }


        public static void DeleteRefreshToken(this HttpResponse response)
        {
            response.Cookies.Delete(
                "refreshToken",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/api/auth/refresh-token"
                }
                );
        }


    }
}
