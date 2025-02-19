using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment.Models
{
    public class Profile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
    }

    public class ShoppingItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public string? ImageUrl { get; set; }
        public string FullImagePath => ImageUrl != null ? $"drawable/{ImageUrl}" : "";

    }

    public class CartItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int ShoppingItemId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
