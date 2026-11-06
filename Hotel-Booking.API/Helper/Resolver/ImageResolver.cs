using AutoMapper;

namespace Hotel_Booking.API.Helper.Resolver
{
    public class ImageResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, string, string>
    {
        private readonly IConfiguration _config;

        public ImageResolver(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string Resolve(TSource source, TDestination destination, string sourceMember, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(sourceMember))
                return $"{_config["BasePictureUrl"]}/Files/Images/{sourceMember}";
            return string.Empty;
        }
    }
}
