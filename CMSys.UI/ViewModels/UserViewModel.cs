using CMSys.Core.Entities.Membership;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMSys.UI.ViewModels
{
    public class UserViewModel
    {
        public Guid? Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Location { get; set; }
        public byte[] Photo { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public List<Role> Roles { get; set; }
        public ICollection<SelectListItem> RolesSelection { get; set; } = new List<SelectListItem>();


    }
}
