using Core.Interfaces;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public PhotoService(IWebHostEnvironment environment, AppDbContext context, ICurrentUserService currentUserService)
        {
            _environment = environment;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<string> AddPhotoAsync(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            return "/uploads/images/" + fileName;
        }
        public void DeletePhoto(string photoUrl)
        {
            if (string.IsNullOrEmpty(photoUrl)) return;

            var fileName = Path.GetFileName(photoUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", "images", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task<string> UpdateUserPhotoAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == 0) throw new Exception("Unauthorized");

            var user = await _context._users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null) throw new Exception("Korisnik nije pronađen");

            if (!string.IsNullOrEmpty(user.PhotoUrl))
            {
                DeletePhoto(user.PhotoUrl);
            }

            var photoUrl = await AddPhotoAsync(file, cancellationToken);

            user.PhotoUrl = photoUrl;
            await _context.SaveChangesAsync(cancellationToken);

            return photoUrl;
        }
    }
}
