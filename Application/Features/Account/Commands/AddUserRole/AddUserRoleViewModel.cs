namespace Application.Features.Account.Commands.AddUserRole
{
    public class AddUserRoleViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}