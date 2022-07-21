using CMSys.Core.Entities.Membership;

namespace CMSys.UI.ViewModels
{
    public class UserViewModel
    {
        public Guid? Id { get; set; }
        public string Email { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly StartDate { get; private set; }
        public DateOnly? EndDate { get; private set; }
        public string Department { get; private set; }
        public string Position { get; private set; }
        public string Location { get; private set; }
        public byte[] Photo { get; private set; }
        public string FullName => $"{FirstName} {LastName}";


    }
}
