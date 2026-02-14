namespace InternProject.Models.PagingModels
{
    public class PaginationKeySetModel<T>
    {
        public IEnumerable<T> ItemsData { get; set; }
        public PaginationKeySetData Pagination { get; set; }
        public PaginationKeySetModel(IEnumerable<T> itemsData , DateTime? cursor, bool? hasMore)
        {
            this.ItemsData = itemsData;
            Pagination = new PaginationKeySetData
            {
                Cursor = cursor,
                HasMore = hasMore
            };
        }
        public class PaginationKeySetData
        {
            public DateTime? Cursor { get; set; }
            public bool? HasMore { get; set; }
        }


    }
}

    

