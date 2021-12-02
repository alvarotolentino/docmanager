namespace Application.Features.Documents.Commands.AssignUserPermission
{
    public class AssignUserPermissionViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
}