namespace RaceDay.Api.Services
{
    public static class ImageValidator
    {
        private static readonly string[] AllowedTypes =
            { "image/jpeg", "image/png", "image/webp" };

        private const long MaxBytes = 2 * 1024 * 1024; 

        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "Please choose an image file.";

            if (file.Length > MaxBytes)
                return "Image must be 2 MB or smaller.";

            if (!AllowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                return "Only JPEG, PNG or WebP images are allowed.";

            return null; 
        }
    }
}