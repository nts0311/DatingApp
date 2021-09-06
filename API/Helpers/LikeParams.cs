namespace API.Helpers
{
    public class LikeParams : PaginationParams
    {
        public string Predicate { get; set; }
        public int UserId { get; set; }
    }
}