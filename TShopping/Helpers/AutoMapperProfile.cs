using AutoMapper;
using TShopping.Data;
using TShopping.ViewModels;

namespace TShopping.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterCustomerVM, KhachHang>().ReverseMap();
        }
    }
}
