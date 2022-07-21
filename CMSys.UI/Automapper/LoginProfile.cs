using CMSys.Core.Entities.Membership;
using AutoMapper;
using CMSys.UI.ViewModels;

namespace CMSys.UI.Automapper
{
    public class LoginProfile : Profile
    {
        public LoginProfile()
        {
            CreateMap<User, LoginViewModel>();
        }
    }
}
