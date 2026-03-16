using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace OPG_Robin_Strandberg_SYSM9.Models
{
    public class AdminUser : User
    {
        public override bool IsAdmin => true;

        // Required by EF Core (TPH)
        protected AdminUser() { }

        public AdminUser(string username, string password, string country)
            : base(username, password, country) { }

        public void RemoveAnyRecipe(Recipe recipe)
        {
            try
            {
                if (recipe == null)
                {
                    MessageBox.Show("No recipe selected to remove.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Remove from database
                App.DbContext.Recipes.Remove(recipe);
                App.DbContext.SaveChanges();

                // Update in-memory RecipeList for the affected user
                foreach (var user in App.UserManager.Users)
                {
                    var r = user.RecipeList.FirstOrDefault(x => x.Id == recipe.Id);
                    if (r != null)
                    {
                        user.RecipeList.Remove(r);
                        break;
                    }
                }

                OnPropertyChanged(nameof(RecipeList));
                MessageBox.Show($"Recipe \"{recipe.Title}\" was removed by administrator.",
                    "Removed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while removing recipe: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ObservableCollection<Recipe> ViewAllRecipes()
        {
            var all = new ObservableCollection<Recipe>();
            foreach (var user in App.UserManager.Users)
                foreach (var recipe in user.RecipeList)
                    if (!all.Contains(recipe))
                        all.Add(recipe);
            return all;
        }
    }
}
