namespace Application.BaseParameters
{
    public class PagedParameter
    {
        public int pagenumber { get; set; }
        public int pagesize { get; set; }

        public PagedParameter()
        {
            this.pagenumber = 1;
            this.pagesize = 10;
        }

        public PagedParameter(int pageNumber, int pageSize)
        {
            this.pagenumber = pageNumber < 1 ? 1 : pageNumber;
            this.pagesize = pageSize > 10 ? 10 : pageSize;
        }
    }
}