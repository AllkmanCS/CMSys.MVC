using AutoMapper;
using CMSys.Common.Paging;
using CMSys.Core.Entities.Catalog;
using CMSys.UI.ViewModels;

namespace CMSys.UI.Automapper
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<PagedList<Course>, CoursesViewModel>()
                .ForMember(cvm => cvm.Items, pl => pl.MapFrom(list => list.Items));
            CreateMap<Course, CourseViewModel>()
                .ReverseMap();
            CreateMap<CourseTrainer, CourseViewModel>().ReverseMap();
            CreateMap<CourseGroup, CourseGroupViewModel>().ReverseMap();

            CreateMap<CourseType, CourseTypeViewModel>();
            CreateMap<CourseTrainer, CourseTrainerViewModel>().ReverseMap();
            CreateMap<TrainerViewModel, CourseTrainerViewModel>().ReverseMap();
        }
    }
}
