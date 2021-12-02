namespace Application.Features.Documents.Commands.AssignGroupPermission
{
    public class AssignGroupPermissionViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
}