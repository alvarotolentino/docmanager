namespace Application.Features.Account.Commands.AddUserRole
{
    public class AddUserRoleViewModel
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; }
    }
}