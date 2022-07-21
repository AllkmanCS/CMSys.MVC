using AutoMapper;
using CMSys.Core.Entities.Catalog;
using CMSys.UI.ViewModels;

namespace CMSys.UI.Automapper
{
    public class TrainerProfile : Profile
    {
        public TrainerProfile()
        {
            CreateMap<Trainer, TrainerViewModel>();
            CreateMap<TrainerGroup, TrainerGroupViewModel>();
        }
    }
}
