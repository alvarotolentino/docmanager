namespace Application.Features.Documents.Commands.AssignGroupPermission
{
    public class AssignGroupPermissionViewModel
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public long DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
}