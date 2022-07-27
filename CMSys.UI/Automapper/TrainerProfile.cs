using AutoMapper;
using CMSys.Core.Entities.Catalog;
using CMSys.Core.Entities.Membership;
using CMSys.UI.ViewModels;

namespace CMSys.UI.Automapper
{
    public class TrainerProfile : Profile
    {
        public TrainerProfile()
        {
            CreateMap<Trainer, TrainerViewModel>().ReverseMap();
            CreateMap<Trainer, UserViewModel>().ReverseMap();
            CreateMap<TrainerViewModel, UserViewModel>().ReverseMap();

            CreateMap<TrainerGroup, TrainerGroupViewModel>().ReverseMap();
            CreateMap<TrainerViewModel, UserViewModel>().ReverseMap();
            CreateMap<User, UserViewModel>().ReverseMap();
        }
    }
}
