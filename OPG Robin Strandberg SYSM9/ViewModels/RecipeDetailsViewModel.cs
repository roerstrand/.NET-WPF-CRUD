using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using OPG_Robin_Strandberg_SYSM9.Commands;
using OPG_Robin_Strandberg_SYSM9.Managers;
using OPG_Robin_Strandberg_SYSM9.Models;
using OPG_Robin_Strandberg_SYSM9.Views;

namespace OPG_Robin_Strandberg_SYSM9.ViewModels
{
    public class RecipeDetailsViewModel : INotifyPropertyChanged
    {
        private readonly RecipeManager _recipeManager;
        private Recipe _recipe;
        private bool _isEditing;
        private string _editButtonText = "Edit";

        public Recipe Recipe
        {
            get => _recipe;
            private set
            {
                _recipe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Ingredients));
                OnPropertyChanged(nameof(Instructions));
                OnPropertyChanged(nameof(Category));
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReadOnly));
                EditButtonText = _isEditing ? "Lock" : "Edit";
            }
        }

        // Inverted IsEditing — controls whether input fields are read-only
        public bool IsReadOnly => !IsEditing;

        public string EditButtonText
        {
            get => _editButtonText;
            private set
            {
                _editButtonText = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => Recipe.Title;
            set
            {
                Recipe.Title = value;
                OnPropertyChanged();
            }
        }

        public string Ingredients
        {
            get => string.Join(", ", Recipe.Ingredients);
            set
            {
                // Split by comma and trim each element, then convert to list (similar to Array.map in JavaScript)
                Recipe.Ingredients = value.Split(',').Select(i => i.Trim()).ToList();
                OnPropertyChanged();
            }
        }

        public string Instructions
        {
            get => Recipe.Instructions;
            set
            {
                Recipe.Instructions = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => Recipe.Category;
            set
            {
                Recipe.Category = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand CancelCommand { get; }

        public RecipeDetailsViewModel(Recipe recipe, RecipeManager recipeManager)
        {
            _recipeManager = recipeManager;
            Recipe = recipe;

            EditCommand = new RelayCommand(_ => ToggleEdit());
            SaveCommand = new RelayCommand(_ => SaveRecipe());
            CopyCommand = new RelayCommand(_ => CopyRecipe());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void ToggleEdit() => IsEditing = !IsEditing;

        private void SaveRecipe()
        {
            try
            {
                Recipe.EditRecipe(Title, Instructions, Category, Ingredients);

                var listWindow = new RecipeListWindow(_recipeManager);
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
                MessageBox.Show($"Error while saving recipe: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CopyRecipe()
        {
            try
            {
                var copy = Recipe.CopyRecipe(App.UserManager.CurrentUser);

                if (copy == null)
                    return;

                _recipeManager.AddRecipe(copy);

                var listWindow = new RecipeListWindow(_recipeManager);
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
                MessageBox.Show($"An error occurred while copying the recipe: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            var listWindow = new RecipeListWindow(_recipeManager);
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
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
