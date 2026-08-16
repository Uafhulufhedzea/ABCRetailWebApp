using System.ComponentModel.DataAnnotations;

namespace ABCRetailWebApp.Models
{
    public class OrderMessageModel
    {
      
        [Required(ErrorMessage = "Please type a message payload or order JSON first.")]
        public string Content { get; set; } = string.Empty;
    }
}
