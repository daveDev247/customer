namespace CustomerApi.Common
{
   
        public class PagedResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<T> Data { get; set; } = new();
            public List<string>? Errors { get; set; }
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

            public static PagedResponse<T> Create(List<T> data, int totalCount, int pageNumber, int pageSize)
            {
                return new PagedResponse<T>
                {
                    Success = true,
                    Message = "Request successful",
                    Data = data,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            // Mirrors ApiResponse<T>.FailResponse, but keeps the paging fields
            // at their defaults (0) since there's no valid page to report on failure.
            public static PagedResponse<T> Fail(string message, List<string>? errors = null)
            {
                return new PagedResponse<T>
                {
                    Success = false,
                    Message = message,
                    Errors = errors,
                    Data = new List<T>()
                };
            }
        }
    
}
