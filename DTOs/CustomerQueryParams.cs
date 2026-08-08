namespace CustomerApi.DTOs
{
    // Query params for the new paged/sorted/filtered endpoint only.
    public class CustomerQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } = "asc";
    }
}
