using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using OPG_Robin_Strandberg_SYSM9.Data;
using OPG_Robin_Strandberg_SYSM9.Models;

namespace OPG_Robin_Strandberg_SYSM9.Managers
{
    public class RecipeManager : INotifyPropertyChanged
    {
        private readonly CookMasterDbContext _db;
        private ObservableCollection<Recipe> _recipes;

        public ObservableCollection<Recipe> RecipeList
        {
            get => _recipes;
            private set
            {
                _recipes = value;
                OnPropertyChanged();
            }
        }

        public RecipeManager(User user, CookMasterDbContext db)
        {
            _db = db;
            RecipeList = user?.RecipeList ?? new ObservableCollection<Recipe>();
        }

        public void AddRecipe(Recipe recipe)
        {
            try
            {
                if (recipe == null)
                {
                    MessageBox.Show("Recipe could not be added because it was empty.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (RecipeList.Any(r => string.Equals(r.Title, recipe.Title, StringComparison.OrdinalIgnoreCase)
                                        && r.CreatedBy?.UserName == recipe.CreatedBy?.UserName))
                {
                    MessageBox.Show("A recipe with that title already exists for this user.", "Duplicate",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                RecipeList.Add(recipe);
                _db.Recipes.Add(recipe);
                _db.SaveChanges();
                OnPropertyChanged(nameof(RecipeList));
                MessageBox.Show($"Recipe \"{recipe.Title}\" was successfully added!", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred while adding the recipe.", ex);
            }
        }

        public void RemoveRecipe(Recipe recipe)
        {
            try
            {
                if (recipe == null)
                {
                    MessageBox.Show("No recipe selected to remove.", "Warning", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!RecipeList.Contains(recipe))
                {
                    MessageBox.Show("That recipe was not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                RecipeList.Remove(recipe);
                _db.Recipes.Remove(recipe);
                _db.SaveChanges();
                OnPropertyChanged(nameof(RecipeList));
                MessageBox.Show($"Recipe \"{recipe.Title}\" was removed.", "Removed", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred while removing the recipe.", ex);
            }
        }

        public ObservableCollection<Recipe> GetAllRecipes()
        {
            try
            {
                return new ObservableCollection<Recipe>(RecipeList);
            }
            catch (Exception ex)
            {
                ShowError("Unable to load recipes.", ex);
                return new ObservableCollection<Recipe>();
            }
        }

        public List<Recipe> GetByUser(User user)
        {
            try
            {
                if (user == null)
                {
                    MessageBox.Show("User not found. Cannot load recipes.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return new List<Recipe>();
                }

                return RecipeList
                    .Where(r => r.CreatedBy?.UserName == user.UserName)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowError("Unable to load recipes for this user.", ex);
                return new List<Recipe>();
            }
        }

        public ObservableCollection<Recipe> ViewAllRecipes(User currentUser, List<User> activeAdmins)
        {
            try
            {
                if (currentUser == null)
                {
                    MessageBox.Show("No user is logged in. Cannot load recipes.",
                        "Session", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return new ObservableCollection<Recipe>();
                }

                if (activeAdmins != null && activeAdmins.Contains(currentUser))
                {
                    return new ObservableCollection<Recipe>(RecipeList);
                }

                // Filter conditions (similar to SQL WHERE)
                return new ObservableCollection<Recipe>(
                    RecipeList.Where(r => r.CreatedBy?.UserName == currentUser.UserName)
                );
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error when loading recipes.", ex);
                return new ObservableCollection<Recipe>();
            }
        }

        public ObservableCollection<Recipe> GetAllRecipesForAdmin(List<User> allUsers)
        {
            try
            {
                var allRecipes = new ObservableCollection<Recipe>();

                if (allUsers == null || allUsers.Count == 0)
                    return allRecipes;

                foreach (var user in allUsers)
                {
                    if (user.RecipeList == null)
                        continue;

                    foreach (var recipe in user.RecipeList.ToList()) // copy current list to avoid mutation during iteration
                    {
                        if (!allRecipes.Contains(recipe))
                            allRecipes.Add(recipe);
                    }
                }

                return allRecipes;
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error while loading all recipes for admin.", ex);
                return new ObservableCollection<Recipe>();
            }
        }


        public ObservableCollection<Recipe> Filter(DateTime? selectedDate, string selectedCategory)
        {
            try
            {
                IEnumerable<Recipe> source;

                if (App.UserManager.CurrentUser is AdminUser)
                    source = App.UserManager.Users.SelectMany(u => u.RecipeList);
                else
                    source = RecipeList;

                if (selectedDate == null && string.IsNullOrWhiteSpace(selectedCategory))
                    return new ObservableCollection<Recipe>(source);

                var filtered = source.Where(r =>
                    (selectedDate == null || r.CreatedAt.Date == selectedDate.Value.Date) &&
                    (string.IsNullOrWhiteSpace(selectedCategory) ||
                     r.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                if (filtered.Count == 0)
                    MessageBox.Show("No recipes matched your filters.", "No Results",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                return new ObservableCollection<Recipe>(filtered);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering recipes: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return new ObservableCollection<Recipe>();
            }
        }

        // Method created per UML but currently the model method is used instead
        public void UpdateRecipe(Recipe recipe, string title, string instructions, string category, string ingredients)
        {
            try
            {
                if (recipe == null)
                {
                    MessageBox.Show("No recipe selected to update.", "Warning", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(instructions))
                {
                    MessageBox.Show("Title and instructions must be filled in.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                recipe.Title = title;
                recipe.Instructions = instructions;
                recipe.Category = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category;

                List<string> ingredientsList;


                if (string.IsNullOrWhiteSpace(ingredients))
                {
                    ingredientsList = new List<string>();
                }
                else
                {
                    string[] parts = ingredients.Split(',');


                    ingredientsList = new List<string>();
                    foreach (string part in parts)
                    {
                        string trimmed = part.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            ingredientsList.Add(trimmed);
                        }
                    }
                }

                recipe.Ingredients = ingredientsList;

                _db.SaveChanges();
                OnPropertyChanged(nameof(RecipeList));
                MessageBox.Show($"Recipe \"{recipe.Title}\" was updated successfully.", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while updating the recipe.", ex);
            }
        }

        private void ShowError(string message, Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] RecipeManager error: {ex.Message}");
            MessageBox.Show($"{message}\n\nDetails: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
