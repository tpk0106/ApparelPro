namespace ApparelPro.WebApi.APIModels
{
    public class PaginationAPIModel<T> where T : class
    {
        public PaginationAPIModel()
        {
            Items = new List<T>();
        }
        public PaginationAPIModel(int pageSize, int currentPage, IList<T> items, 
            int totalPages, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            PageSize = pageSize;
            CurrentPage = currentPage;
            Items = items;
            TotalPages = totalPages;
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
        public int TotalPages{ get; set; }       

        //public T First
        //{
        //    get {
        //       var item = Items.Skip(ItemsPerPage * CurrentPage - 1).Take(ItemsPerPage).First();
        //        return item;
        //    }
        //}

        //public T Last
        //{
        //    get
        //    {
        //        var lastItem = Items.Skip(ItemsPerPage * CurrentPage-1).Take(ItemsPerPage).Last();
        //        return lastItem;
        //    }
        //}

    }
}
