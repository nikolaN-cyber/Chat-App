using Microsoft.AspNetCore.Http;

namespace Core.Interfaces
{
    public interface IPhotoService
    {
        Task<string> AddPhotoAsync(IFormFile file, CancellationToken cancellationToken);
        void DeletePhoto(string photoUrl);
        Task<string> UpdateUserPhotoAsync(IFormFile file, CancellationToken cancellationToken);
    }
}
