namespace API.DTOs
{
    public class PhotoApprovalDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public bool IsApproval { get; set; }
    }
}