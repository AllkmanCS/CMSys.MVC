﻿namespace CMSys.UI.ViewModels
{
    public class RoleViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserViewModel> Users { get; set; }

    }
}
