using CMSys.Common.Paging;
using CMSys.Core.Entities.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CMSys.UI.ViewModels
{
    public class CourseViewModel
    {
        public Guid? Id { get; set; }
        [StringLength(255, MinimumLength = 3)]
        [Required]
        public string? Name { get; set; }
        [StringLength(5000, MinimumLength = 3)]
        [Required]
        public string? Description { get; set; }
        [Required]
        public int? VisualOrder { get; set; }
        public bool IsNew { get; set; }
        public List<CourseTrainerViewModel>? Trainers { get; set; }
        public TrainerViewModel Trainer { get; set; }
        public PageInfo PageInfo { get; set; } = new PageInfo();
        public int PerPage { get; set; } = 5;
        public ICollection<SelectListItem> CourseTypes { get; set; }
        public ICollection<SelectListItem> CourseGroups { get; set; }
        public ICollection<SelectListItem> SelectionTrainers { get; set; } = new List<SelectListItem>();
        [Required]
        public CourseTypeViewModel CourseType { get; set; }
        [Required]
        public CourseGroupViewModel CourseGroup { get; set; }
    }
}
