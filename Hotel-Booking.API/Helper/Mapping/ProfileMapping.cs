using AutoMapper;
using Hotel_booking.Core.Models.Accounts;
using Hotel_booking.Core.ModelsDTO.ModelsDtoRequest.AccountServices;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse.AccountDTO;

namespace Hotel_Booking.API.Helper.Mapping
{
    public class ProfileMapping : Profile
    {
        public ProfileMapping()
        {
            CreateMap<RegisterRequest, ApplicationUser>();
            CreateMap<ApplicationUser, RegisterResponse>();
        }
    }
}
