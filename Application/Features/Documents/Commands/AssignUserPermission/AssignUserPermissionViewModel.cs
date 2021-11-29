namespace Application.Features.Documents.Commands.AssignUserPermission
{
    public class AssignUserPermissionViewModel
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
}