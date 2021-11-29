namespace Domain.Entities
{
    public class UserDocument
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long DocumentId { get; set; }
        public string DocumentName { get; set; }
    }

}