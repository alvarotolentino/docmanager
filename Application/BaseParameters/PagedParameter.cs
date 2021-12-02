namespace Application.BaseParameters
{
    public class PagedParameter
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }


        public PagedParameter()
        {
            this.PageNumber = 1;
            this.PageSize = 10;
        }

        public PagedParameter(int pageNumber, int pageSize)
        {
            this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.PageSize = pageSize > 10 ? 10 : pageSize;
        }
    }
}