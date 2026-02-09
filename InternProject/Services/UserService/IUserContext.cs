namespace InternProject.Services.UserService
{
    public interface IUserContext
    {
        Guid GetCurrentUserId();
        bool IsAuthenticated { get; }
    }
}
