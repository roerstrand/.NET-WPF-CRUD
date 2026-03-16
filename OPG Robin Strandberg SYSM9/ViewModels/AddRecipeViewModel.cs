using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using OPG_Robin_Strandberg_SYSM9.Commands;
using OPG_Robin_Strandberg_SYSM9.Managers;
using OPG_Robin_Strandberg_SYSM9.Models;
using OPG_Robin_Strandberg_SYSM9.Views;

namespace OPG_Robin_Strandberg_SYSM9.ViewModels
{
    public class AddRecipeViewModel : INotifyPropertyChanged
    {
        private readonly RecipeManager _recipeManager;

        private string _title;
        private string _ingredients;
        private string _instructions;
        private string _category;
        private DateTime _createdDate = DateTime.Now;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Ingredients
        {
            get => _ingredients;
            set
            {
                _ingredients = value;
                OnPropertyChanged();
            }
        }

        public string Instructions
        {
            get => _instructions;
            set
            {
                _instructions = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged();
            }
        }

        public List<string> Categories { get; } = new List<string>
        {
            "Breakfast",
            "Lunch",
            "Dinner",
            "Dessert",
            "Snack",
            "Drink",
            "Soup",
            "Salad",
            "Appetizer",
            "Side Dish",
            "Baking",
            "Vegetarian",
            "Seafood"
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddRecipeViewModel(RecipeManager recipeManager)
        {
            _recipeManager = recipeManager;

            SaveCommand = new RelayCommand(_ => SaveRecipe());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void SaveRecipe()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title) ||
                    string.IsNullOrWhiteSpace(Instructions) ||
                    string.IsNullOrWhiteSpace(Category) ||
                    string.IsNullOrWhiteSpace(Ingredients))
                {
                    MessageBox.Show("All fields must be filled out.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var recipe = new Recipe(
                    Title,
                    Instructions,
                    Category,
                    CreatedDate,
                    App.UserManager.CurrentUser,
                    Ingredients
                );

                _recipeManager.AddRecipe(recipe);

                var listWindow = new RecipeListWindow(App.UserManager.GetRecipeManagerForCurrentUser());
                listWindow.Show();

                foreach (Window w in Application.Current.Windows)
                {
                    if (w.DataContext == this)
                    {
                        w.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            var listWindow = new RecipeListWindow(App.UserManager.GetRecipeManagerForCurrentUser());
            listWindow.Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w.DataContext == this)
                {
                    w.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
