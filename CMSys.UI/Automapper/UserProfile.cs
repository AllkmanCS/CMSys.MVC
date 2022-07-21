using AutoMapper;
using CMSys.Common.Paging;
using CMSys.Core.Entities.Membership;
using CMSys.UI.ViewModels;

namespace CMSys.UI.Automapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserViewModel>();
            CreateMap<PagedList<User>, UsersViewModel>();
        }
    }
}
