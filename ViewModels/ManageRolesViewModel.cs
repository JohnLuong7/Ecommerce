using System.Collections.Generic;

namespace Ecommerce.ViewModels
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; }
        public List<RoleSelection> Roles { get; set; }
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
}
