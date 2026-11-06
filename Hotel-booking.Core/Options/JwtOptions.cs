namespace Hotel_booking.Core.Options
{
    public class JwtOptions
    {
        public const string SectionName = "JWT";

        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public double AccessTokenExpirationMinutes { get; set; }
        public double RefreshTokenExpirationDays { get; set; }
        public int ClockSkewMinutes { get; set; }
        public string Key { get; set; } = string.Empty;
    }
}
