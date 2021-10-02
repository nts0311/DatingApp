using System.Threading.Tasks;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository {get; }
        IMessageRepository MessageRepository {get; }
        ILikesRepository LikesRepository {get; }
        Task<bool> Complete();
        bool HasChanged();
    }
}