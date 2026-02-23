namespace InternProject.Models.PagingModels
{
    public readonly record struct CompositeCursor<TPrimary, TSecondary>(
    TPrimary Primary,
    TSecondary Secondary
)
    where TPrimary : IComparable
    where TSecondary : IComparable;
}
