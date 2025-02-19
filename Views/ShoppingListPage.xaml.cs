using Assignment.Services;
using Assignment.Models;
namespace Assignment.Views;

public partial class ShoppingListPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Profile _currentProfile;

    public ShoppingListPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        LoadData();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadData(); // Refresh data when the page appears
    }

    private async void LoadData()
    {
        _currentProfile = await _databaseService.GetProfileAsync();
        var items = await _databaseService.GetShoppingItemsAsync();
        foreach (var item in items)
        {
            // Update the image path
            item.ImageUrl = _databaseService.GetFullImagePath(item.ImageUrl);
            System.Diagnostics.Debug.WriteLine($"Loading image for {item.Name}: {item.ImageUrl}");
        }
        ShoppingItemsCollection.ItemsSource = items;
    }


    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not ShoppingItem item)
            return;

        string result = await DisplayPromptAsync("Quantity",
            $"How many {item.Name} would you like to add?",
            keyboard: Keyboard.Numeric);

        if (!int.TryParse(result, out int quantity) || quantity <= 0)
        {
            await DisplayAlert("Error", "Please enter a valid quantity", "OK");
            return;
        }

        try
        {
            var cartItem = new CartItem
            {
                ProfileId = _currentProfile.Id,
                ShoppingItemId = item.Id,
                Quantity = quantity,
                DateAdded = DateTime.Now
            };

            await _databaseService.AddToCartAsync(cartItem);
            await DisplayAlert("Success", "Item added to cart!", "OK");
            LoadData(); // Refresh the list to show updated stock
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }


    private async void OnResetClicked(object sender, EventArgs e)
    {
        await _databaseService.ResetDatabaseAsync();
        LoadData(); // Reload data after reset
    }

    private async void OnCartClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CartPage");
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ProfilePage");
    }
}