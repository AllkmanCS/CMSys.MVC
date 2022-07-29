using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMSys.UI.ViewModels
{
    public class TrainerViewModel
    {
        public const int DescriptionLength = 4000;

        public Guid? Id { get; set; }
        public int VisualOrder { get; set; }
        public Guid TrainerGroupId { get; set; }
        public string Description { get; set; }

        public UserViewModel User { get; set; }
        public TrainerGroupViewModel TrainerGroup { get; set; }
        public ICollection<SelectListItem> TrainerGroupSelector { get; set; }
        public ICollection<SelectListItem> Users { get; set; }
    }
}
