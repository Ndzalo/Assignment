using Assignment.Views;
namespace Assignment
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // MainPage = new ShoppingListPage();
            MainPage = new AppShell();
            //MainPage = new NavigationPage(new ShoppingListPage());
            Routing.RegisterRoute("CartPage", typeof(CartPage));
            Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
        }
    }
}
