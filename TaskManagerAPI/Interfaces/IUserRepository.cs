using TaskManagerAPI.Models;

namespace TaskManagerAPI.Interfaces
{
    public interface IUserRepository
    {
        User GetUser(string username);
        void AddUser(User user);
        void UpdateUser(User user);
        User GetUserByRefreshToken(string refreshToken);
    }
}
