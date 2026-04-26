namespace MediApp.Models;

public class PagedResult<T>
{
    public List<T> Items {get;set;} = [];
    public int PageSize{get;set;}
    public int PageNumber {get;set;}
    public int TotalItemCount {get;set;}
    public int TotalPageCount => (int)Math.Ceiling(TotalItemCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPageCount;
}