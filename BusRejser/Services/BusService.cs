using BusRejser.Exceptions;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BusRejserLibrary.Services
{
	public class BusService
	{
		private readonly BusRepository _busRepository;
		private readonly FacilitetRepository _facilitetRepository;
		private readonly BusFacilitetRepository _busFacilitetRepository;

		public BusService(
			BusRepository busRepository,
			FacilitetRepository facilitetRepository,
			BusFacilitetRepository busFacilitetRepository)
		{
			_busRepository = busRepository;
			_facilitetRepository = facilitetRepository;
			_busFacilitetRepository = busFacilitetRepository;
		}

		public int Create(Bus bus) => _busRepository.Create(bus);

		public Bus? GetById(int id) => _busRepository.GetById(id);

		public List<Bus> GetAll() => _busRepository.GetAll();

		public bool Update(Bus bus) => _busRepository.Update(bus);

		public bool Delete(int id) => _busRepository.Delete(id);

		public List<Facilitet> GetFaciliteterForBus(int busId)
		{
			var ids = _busFacilitetRepository.GetFacilitetIdsForBus(busId);
			var list = new List<Facilitet>();

			foreach (var fid in ids)
			{
				var f = _facilitetRepository.GetById(fid);
				if (f != null) list.Add(f);
			}

			return list;
		}

		public async Task<string> UploadImageAsync(int busId, IFormFile file)
		{
			var bus = _busRepository.GetById(busId);
			if (bus == null)
				throw new NotFoundException("Bus ikke fundet.");

			if (file == null || file.Length == 0)
				throw new ValidationException("Du skal vælge en fil.");

			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(extension))
				throw new ValidationException("Kun JPG, JPEG, PNG og WEBP er tilladt.");

			if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/"))
				throw new ValidationException("Filen skal være et billede.");

			const long maxBytes = 5 * 1024 * 1024;
			if (file.Length > maxBytes)
				throw new ValidationException("Filen er for stor. Maks 5 MB.");

			await using var readStream = file.OpenReadStream();
			using var image = await Image.LoadAsync(readStream);

			if (image.Width < 800 || image.Height < 450)
				throw new ValidationException("Billedet er for lille. Minimum er 800x450.");

			const int targetWidth = 1280;
			const int targetHeight = 720;
			const double targetRatio = (double)targetWidth / targetHeight;
			var currentRatio = (double)image.Width / image.Height;

			int cropWidth;
			int cropHeight;
			int cropX;
			int cropY;

			if (currentRatio > targetRatio)
			{
				cropHeight = image.Height;
				cropWidth = (int)(cropHeight * targetRatio);
				cropX = (image.Width - cropWidth) / 2;
				cropY = 0;
			}
			else
			{
				cropWidth = image.Width;
				cropHeight = (int)(cropWidth / targetRatio);
				cropX = 0;
				cropY = (image.Height - cropHeight) / 2;
			}

			image.Mutate(x => x
				.Crop(new Rectangle(cropX, cropY, cropWidth, cropHeight))
				.Resize(targetWidth, targetHeight)
			);

			var uploadsFolder = Path.Combine(
				Directory.GetCurrentDirectory(),
				"wwwroot",
				"uploads",
				"buses"
			);

			Directory.CreateDirectory(uploadsFolder);

			var fileName = $"{Guid.NewGuid()}{extension}";
			var fullPath = Path.Combine(uploadsFolder, fileName);

			await image.SaveAsync(fullPath);

			if (!string.IsNullOrWhiteSpace(bus.ImageUrl))
			{
				var oldFileName = Path.GetFileName(bus.ImageUrl);
				var oldPath = Path.Combine(uploadsFolder, oldFileName);

				if (File.Exists(oldPath))
					File.Delete(oldPath);
			}

			bus.ImageUrl = $"/uploads/buses/{fileName}";

			var updated = _busRepository.Update(bus);
			if (!updated)
				throw new Exception("Kunne ikke gemme billedesti.");

			return bus.ImageUrl!;
		}

		public bool AddFacilitet(int busId, int facilitetId)
		{
			if (_busRepository.GetById(busId) == null) return false;
			if (_facilitetRepository.GetById(facilitetId) == null) return false;

			return _busFacilitetRepository.Add(busId, facilitetId);
		}

		public bool RemoveFacilitet(int busId, int facilitetId)
		{
			return _busFacilitetRepository.Remove(busId, facilitetId);
		}
	}
}