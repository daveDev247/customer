namespace CustomerApi.DTOs
{
    public class BulkCreateResultDto
    {
        public List<CustomerResponseDto> Created { get; set; } = new();
        public List<BulkCreateErrorDto> Errors { get; set; } = new();
        public int SuccessCount => Created.Count;
        public int FailureCount => Errors.Count;
    }

    public class BulkCreateErrorDto
    {
        public int Index { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}
