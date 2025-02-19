using Assignment.Models;
using Assignment.Services;
using Assignment.ViewModels;

namespace Assignment.Views;

public partial class CartPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Profile _currentProfile;
    private List<CartItemViewModel> _cartItems;
    public CartPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }
    public CartPage(DatabaseService databaseService = null)
    {
        InitializeComponent();
        _databaseService = databaseService ?? new DatabaseService();
        //LoadCart();
    }

    private async Task LoadCartAsync()
    {
        try
        {
            _currentProfile = await _databaseService.GetProfileAsync();
            var cartItems = await _databaseService.GetCartItemsAsync(_currentProfile.Id);
            _cartItems = new List<CartItemViewModel>();

            foreach (var cartItem in cartItems)
            {
                var item = await _databaseService.GetShoppingItemAsync(cartItem.ShoppingItemId);
                if (item != null)
                {
                    _cartItems.Add(new CartItemViewModel
                    {
                        Id = cartItem.Id,
                        ItemName = item.Name,
                        Price = item.Price,
                        ItemImageUrl = _databaseService.GetFullImagePath(item.ImageUrl),
                        Quantity = cartItem.Quantity
                    });
                }
            }
            CartItemsCollection.ItemsSource = null;
            CartItemsCollection.ItemsSource = _cartItems;
            UpdateCartSummary();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load cart: " + ex.Message, "OK");
        }
    }

    private void UpdateCartSummary()
    {
        if (_cartItems != null)
        {
            int totalItems = _cartItems.Sum(i => i.Quantity);
            decimal totalAmount = _cartItems.Sum(i => i.TotalPrice);

            TotalItemsLabel.Text = totalItems.ToString();
            TotalAmountLabel.Text = $"${totalAmount:N2}";
        }
        else
        {
            TotalItemsLabel.Text = "0";
            TotalAmountLabel.Text = "$0.00";
        }
    }

    private async void OnRemoveItemClicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            var cartItem = button?.CommandParameter as CartItemViewModel;

            if (cartItem != null)
            {
                bool answer = await DisplayAlert("Remove Item",
                    $"Remove {cartItem.ItemName} from cart?",
                    "Yes", "No");

                if (answer)
                {
                    await _databaseService.RemoveFromCartAsync(cartItem.Id);
                    await DisplayAlert("Success", "Item removed from cart", "OK");
                    await LoadCartAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to remove item: " + ex.Message, "OK");
        }
    }

    private async void OnBackToShoppingClicked(object sender, EventArgs e)
    {
        //await Navigation.PopAsync();
        await Shell.Current.GoToAsync("//ShoppingListPage");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCartAsync(); // Changed to async method
    }

    private async void OnIncreaseQuantityClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var cartItemViewModel = button?.CommandParameter as CartItemViewModel;

        if (cartItemViewModel != null)
        {
            try
            {
                // Get the corresponding cart item from the database
                var cartItem = await _databaseService.GetCartItemAsync(cartItemViewModel.Id);

                // Get the shopping item to check stock
                var shoppingItem = await _databaseService.GetShoppingItemAsync(cartItem.ShoppingItemId);

                if (shoppingItem != null && shoppingItem.QuantityInStock > 0)
                {
                    // Increase the quantity
                    cartItem.Quantity++;
                    shoppingItem.QuantityInStock--;

                    // Update the database
                    await _databaseService.UpdateCartItemAsync(cartItem);
                    await _databaseService.UpdateShoppingItemAsync(shoppingItem);

                    // Refresh the cart
                    await LoadCartAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Insufficient stock", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnDecreaseQuantityClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var cartItemViewModel = button?.CommandParameter as CartItemViewModel;

        if (cartItemViewModel != null)
        {
            try
            {
                // Get the corresponding cart item from the database
                var cartItem = await _databaseService.GetCartItemAsync(cartItemViewModel.Id);

                // Get the shopping item to update stock
                var shoppingItem = await _databaseService.GetShoppingItemAsync(cartItem.ShoppingItemId);

                if (cartItem.Quantity > 1)
                {
                    // Decrease the quantity
                    cartItem.Quantity--;
                    shoppingItem.QuantityInStock++;

                    // Update the database
                    await _databaseService.UpdateCartItemAsync(cartItem);
                    await _databaseService.UpdateShoppingItemAsync(shoppingItem);

                    // Refresh the cart
                    await LoadCartAsync();
                }
                else
                {
                    // If quantity is 1, remove the item from the cart
                    await _databaseService.RemoveFromCartAsync(cartItem.Id);
                    shoppingItem.QuantityInStock++;
                    await _databaseService.UpdateShoppingItemAsync(shoppingItem);

                    // Refresh the cart
                    await LoadCartAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}