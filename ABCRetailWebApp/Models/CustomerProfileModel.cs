using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailWebApp.Models
{
    public class CustomerProfileModel : ITableEntity
    { 
        public string PartitionKey { get; set; } = "Customer"; 
        public string RowKey { get; set; } = Guid.NewGuid().ToString(); 
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

    
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
