namespace TmsApi.Dtos;

public class PagedRequest
{
    private const int MaxPageSize = 50;
    private int _pageSize = 20;

    public int Page { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value < 1) ? 20 : (value > MaxPageSize ? MaxPageSize : value);
    }

    public string? Search { get; set; }
    public string OrderBy { get; set; } = "Title";
    public bool Descending { get; set; }
}