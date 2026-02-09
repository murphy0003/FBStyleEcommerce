namespace InternProject.Models.PagingModels
{
    public class PaginationModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public PaginationMetadata Pagination { get; set; }
        public PaginationModel(IEnumerable<T> items, int totalCount, int pageSize, int currentPage)
        {
            this.Items = items;
            Pagination = new PaginationMetadata
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = currentPage,
                TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0
            };
        }
        public class PaginationMetadata
        {
            public int TotalCount { get; set; }
            public int PageSize { get; set; }
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public bool HasPrevious => CurrentPage > 1;
            public bool HasNext => CurrentPage < TotalPages;
        }
    }
}
