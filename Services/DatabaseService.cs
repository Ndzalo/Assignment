using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assignment.Models;

namespace Assignment.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "shopping.db");
            _database = new SQLiteAsyncConnection(dbPath);
            InitializeDatabaseAsync();
        }
        public async Task ResetDatabaseAsync()
        {
            await _database.DropTableAsync<CartItem>();
            await _database.DropTableAsync<ShoppingItem>();
            await _database.DropTableAsync<Profile>();

            await _database.CreateTableAsync<Profile>();
            await _database.CreateTableAsync<ShoppingItem>();
            await _database.CreateTableAsync<CartItem>();

            // This will trigger the initialization with new items
            await InitializeDatabaseAsync();
        }
        private async Task InitializeDatabaseAsync()
        {
            await _database.CreateTableAsync<Profile>();
            await _database.CreateTableAsync<ShoppingItem>();
            await _database.CreateTableAsync<CartItem>();

            // Seed shopping items if none exist
            if (await _database.Table<ShoppingItem>().CountAsync() == 0)
            {
                var items = new List<ShoppingItem>
                {
                    new ShoppingItem { Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, QuantityInStock = 10,ImageUrl = "laptop.png" },
                    new ShoppingItem { Name = "Smartphone", Description = "Latest model", Price = 699.99m, QuantityInStock = 15,ImageUrl = "smartphone.png" },
                    new ShoppingItem { Name = "Headphones", Description = "Wireless noise-canceling", Price = 199.99m, QuantityInStock = 20,ImageUrl = "headphones.png" },
                    new ShoppingItem { Name = "Smart Watch", Description = "Fitness tracking and notifications", Price = 299.99m, QuantityInStock = 25,ImageUrl = "smartwatch.png" },
                    new ShoppingItem { Name = "Tablet", Description = "10-inch display, perfect for multimedia", Price = 449.99m, QuantityInStock = 12,ImageUrl = "tablet.png" },
                    new ShoppingItem { Name = "Gaming Console", Description = "Next-gen gaming system", Price = 499.99m, QuantityInStock = 8 , ImageUrl = "gaming.jpg"},
                    new ShoppingItem { Name = "Wireless Mouse", Description = "Ergonomic design with precision tracking", Price = 49.99m, QuantityInStock = 30,ImageUrl = "mouse.png" },
                    new ShoppingItem { Name = "Mechanical Keyboard", Description = "RGB backlit gaming keyboard", Price = 129.99m, QuantityInStock = 18,ImageUrl = "keyboard.png" },
                    new ShoppingItem { Name = "4K Monitor", Description = "27-inch Ultra HD display", Price = 349.99m, QuantityInStock = 14 , ImageUrl = "monitor.png"},
                    new ShoppingItem { Name = "Wireless Earbuds", Description = "True wireless with charging case", Price = 159.99m, QuantityInStock = 22,ImageUrl = "earbuds.png" },
                    new ShoppingItem { Name = "VR Headset", Description = "Immersive virtual reality experience", Price = 399.99m, QuantityInStock = 10,ImageUrl = "vr.png" },
                    new ShoppingItem { Name = "Drone", Description = "4K camera drone with GPS", Price = 799.99m, QuantityInStock = 8,ImageUrl = "drone.png" },
                    new ShoppingItem { Name = "Security Camera", Description = "Smart home security camera", Price = 129.99m, QuantityInStock = 25,ImageUrl = "camera.png" },
                    new ShoppingItem { Name = "Robot Vacuum", Description = "Smart navigation and app control", Price = 299.99m, QuantityInStock = 15,ImageUrl = "robot.jpg" },
                    new ShoppingItem { Name = "Smart Thermostat", Description = "Energy-saving smart home device", Price = 179.99m, QuantityInStock = 20,ImageUrl = "thermostst.png" }
                };
                await _database.InsertAllAsync(items);
            }
        }

        // Profile operations
        public async Task<Profile> GetProfileAsync()
        {
            var profile = await _database.Table<Profile>().FirstOrDefaultAsync();
            return profile ?? new Profile();
        }

        public async Task SaveProfileAsync(Profile profile)
        {
            if (profile.Id == 0)
                await _database.InsertAsync(profile);
            else
                await _database.UpdateAsync(profile);
        }

        // Shopping Item operations
        public async Task<List<ShoppingItem>> GetShoppingItemsAsync()
        {
            return await _database.Table<ShoppingItem>().ToListAsync();
        }

        public async Task<ShoppingItem> GetShoppingItemAsync(int id)
        {
            return await _database.Table<ShoppingItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        // Cart operations
        public async Task<List<CartItem>> GetCartItemsAsync(int profileId)
        {
            return await _database.Table<CartItem>().Where(c => c.ProfileId == profileId).ToListAsync();
        }


        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            await _database.UpdateAsync(cartItem);
        }

        public async Task UpdateShoppingItemAsync(ShoppingItem shoppingItem)
        {
            await _database.UpdateAsync(shoppingItem);
        }
        public async Task AddToCartAsync(CartItem cartItem)
        {
            // First, get the shopping item to check stock
            var item = await GetShoppingItemAsync(cartItem.ShoppingItemId);
            if (item == null)
                throw new Exception("Item not found");

            // Check if the item already exists in the cart for the current profile
            var existingCartItem = await _database.Table<CartItem>()
                .Where(c => c.ProfileId == cartItem.ProfileId && c.ShoppingItemId == cartItem.ShoppingItemId)
                .FirstOrDefaultAsync();

            // Calculate the total requested quantity
            int totalRequestedQuantity = cartItem.Quantity;
            if (existingCartItem != null)
            {
                totalRequestedQuantity += existingCartItem.Quantity;
            }

            // Check if there is enough stock
            if (item.QuantityInStock < totalRequestedQuantity)
                throw new Exception($"Insufficient stock. Only {item.QuantityInStock} items available.");

            if (existingCartItem != null)
            {
                // Update the quantity of the existing cart item
                existingCartItem.Quantity += cartItem.Quantity;
                await _database.UpdateAsync(existingCartItem);
            }
            else
            {
                // Add a new cart item
                await _database.InsertAsync(cartItem);
            }

            // Update the stock
            item.QuantityInStock -= cartItem.Quantity;
            await _database.UpdateAsync(item);
        }
        public async Task<CartItem> GetCartItemAsync(int cartItemId)
        {
            return await _database.Table<CartItem>().Where(c => c.Id == cartItemId).FirstOrDefaultAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _database.Table<CartItem>().Where(c => c.Id == cartItemId).FirstOrDefaultAsync();
            if (cartItem != null)
            {
                var item = await GetShoppingItemAsync(cartItem.ShoppingItemId);
                if (item != null)
                {
                    item.QuantityInStock += cartItem.Quantity;
                    await _database.UpdateAsync(item);
                }
                await _database.DeleteAsync(cartItem);
            }
        }
        public string GetFullImagePath(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return "default_product.png";

            // If the image URL is already a full path, return it
            if (imageUrl.StartsWith("http") || Path.IsPathRooted(imageUrl))
                return imageUrl;

            // Otherwise, construct the path relative to your app's resources
            return Path.Combine("Resources", "Images", imageUrl);
        }
    }
}
