namespace InternProject.Models.PagingModels
{
    public sealed class PaginationKeySetModel<T , TCursor>
    {
        public IReadOnlyList<T> ItemsData { get; set; }
        public PaginationKeySetData Pagination { get; set; }
        public PaginationKeySetModel(IReadOnlyList<T> itemsData , TCursor cursor, bool? hasMore)
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
            public TCursor? Cursor { get; set; }
            public bool? HasMore { get; set; }
        }


    }
}