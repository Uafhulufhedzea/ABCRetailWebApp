using Microsoft.AspNetCore.Http;

namespace ABCRetailWebApp.Models
{
    public class BlobStorageModel
    {
        //capturing the actual file uploaded by the user
        public IFormFile? ImageFile { get; set; }

        //Displaying the direct URL link of the image back to the user after uploading
        public string? ImageUrl { get; set; }
    }
}
