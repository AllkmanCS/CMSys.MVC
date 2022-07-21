using AutoMapper;
using CMSys.Core.Entities.Catalog;
using CMSys.UI.ViewModels;

namespace CMSys.UI.Automapper
{
    public class CourseTrainerProfile : Profile
    {
        public CourseTrainerProfile()
        {
            CreateMap<CourseTrainer, CourseTrainerViewModel>();
        }
    }
}
