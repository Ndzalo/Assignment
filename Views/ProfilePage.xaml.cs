using Microsoft.Maui;
using Assignment.Services;
using Assignment.Models;


namespace Assignment.Views;

public partial class ProfilePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Profile _currentProfile;

    public ProfilePage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        LoadProfile();
    }

    private async void LoadProfile()
    {
        try
        {
            _currentProfile = await _databaseService.GetProfileAsync();

            NameEntry.Text = _currentProfile.Name;
            SurnameEntry.Text = _currentProfile.Surname;
            EmailEntry.Text = _currentProfile.Email;
            BioEditor.Text = _currentProfile.Bio;

            if (!string.IsNullOrEmpty(_currentProfile.ProfilePicturePath))
            {
                ProfileImage.Source = _currentProfile.ProfilePicturePath;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load profile: " + ex.Message, "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(SurnameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Validation Error", "Name, Surname and Email are required.", "OK");
            return;
        }

        try
        {
            _currentProfile.Name = NameEntry.Text;
            _currentProfile.Surname = SurnameEntry.Text;
            _currentProfile.Email = EmailEntry.Text;
            _currentProfile.Bio = BioEditor.Text ?? string.Empty;

            await _databaseService.SaveProfileAsync(_currentProfile);
            await DisplayAlert("Success", "Profile saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to save profile: " + ex.Message, "OK");
        }
    }

    private async void OnProfileImageTapped(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                var newFile = Path.Combine(FileSystem.AppDataDirectory, "profile_picture.jpg");
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                {
                    await stream.CopyToAsync(newStream);
                }

                _currentProfile.ProfilePicturePath = newFile;
                ProfileImage.Source = newFile;
                await _databaseService.SaveProfileAsync(_currentProfile);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to update profile picture: " + ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        //await Navigation.PopAsync();
        await Shell.Current.GoToAsync("//ShoppingListPage");
    }
}