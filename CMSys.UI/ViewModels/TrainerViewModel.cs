using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMSys.UI.ViewModels
{
    public class TrainerViewModel
    {
        public const int DescriptionLength = 4000;

        public Guid? Id => User == null ? null : User.Id; 
        public int VisualOrder { get; set; }
        public Guid? TrainerGroupId  => TrainerGroup == null ? null : TrainerGroup.Id;
        public string Description { get; set; }

        public UserViewModel User { get; set; }
        public TrainerGroupViewModel TrainerGroup { get; set; }
        public ICollection<SelectListItem> TrainerGroupSelector { get; set; }
        public ICollection<SelectListItem> Users { get; set; }
    }
}
