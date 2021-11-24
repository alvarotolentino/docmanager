namespace Application.Common
{
    public class PagedResponse<T> : Response<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public PagedResponse(T data, int pageNumber, int pageSize)
        : base(data: data)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;

        }

    }
}