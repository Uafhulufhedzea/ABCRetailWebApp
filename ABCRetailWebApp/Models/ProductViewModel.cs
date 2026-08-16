using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ABCRetailWebApp.Models
{
    public class ProductViewModel : ITableEntity
    {
        // Required Azure Table Layout Columns
        public string PartitionKey { get; set; } = "RetailProduct";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Product Catalog Form Attributes
        [Required(ErrorMessage = "Please provide a product title name.")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

      
        [Required(ErrorMessage = "Please state the unit price.")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be a valid amount greater than 0.")]
        [IgnoreDataMember] 
        public decimal Price { get; set; }

       
        public double AzurePrice
        {
            get => (double)Price;
            set => Price = (decimal)value;
        }

        [Required(ErrorMessage = "Please choose a retail department category.")]
        public string Category { get; set; } = string.Empty;

        // Image Management Fields
        [Display(Name = "Product Feature Image")]
        [IgnoreDataMember]
        public IFormFile? ProductImageFile { get; set; }

        public string? FinalImageUrl { get; set; }
    }
}
