namespace ApparelPro.Shared.Extensions
{
    public class PaginationResult<T> where T:class
    {
        public PaginationResult()
        {
            Items = new List<T>();
        }
        public PaginationResult(int pageSize, int currentPage, int totalItems, IList<T> items, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery )
        {
            PageSize = pageSize;
            CurrentPage = currentPage;
            Items = items;
            TotalItems = totalItems;
            _ = TotalPages;
            SortColumn = sortColumn;
            SortOrder = sortOrder;
            FilterColumn = filterColumn;
            FilterQuery = filterQuery;
        }

        public IList<T> Items { get; set; }      
        public int PageSize { get; set; } 
        public int CurrentPage { get; set; }    
        public int TotalItems { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public string? FilterColumn { get; set; }
        public string? FilterQuery { get; set; }

        public int TotalPages
        {
            get
            {
                var total = ((double)TotalItems / PageSize);
                var pageTotals = (int)Math.Ceiling(total);
                return pageTotals;
            }
        }

        //public T First
        //{
        //    get
        //    {
        //        var item = Items.Skip(ItemsPerPage * CurrentPage - 1).Take(ItemsPerPage).First();
        //        return item;
        //    }
        //}

        //public T Last
        //{
        //    get
        //    {
        //        var lastItem = Items.Skip(ItemsPerPage * CurrentPage - 1).Take(ItemsPerPage).Last();
        //        return lastItem;
        //    }
        //}
    }
}
