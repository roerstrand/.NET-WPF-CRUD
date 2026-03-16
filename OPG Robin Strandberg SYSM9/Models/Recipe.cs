using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace OPG_Robin_Strandberg_SYSM9.Models
{
    public class Recipe : INotifyPropertyChanged
    {
        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _instructions;

        public string Instructions
        {
            get => _instructions;
            set
            {
                if (_instructions != value)
                {
                    _instructions = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _category;

        public string Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _createdAt = DateTime.Now;

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged();
                }
            }
        }

        private User _createdBy;

        public User CreatedBy
        {
            get => _createdBy;
            set
            {
                if (_createdBy != value)
                {
                    _createdBy = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<string> _ingredients = new List<string>();

        public List<string> Ingredients
        {
            get => _ingredients;
            set
            {
                if (value != null)
                {
                    _ingredients = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Id { get; private set; }

        // Required by EF Core to instantiate objects when loading from the database
        protected Recipe()
        {
            Ingredients = new List<string>();
        }

        public Recipe(string title, string instructions, string category, DateTime createdAt, User createdBy,
            string ingredients)
        {
            Title = title;
            Instructions = instructions;
            Category = category;
            CreatedAt = createdAt;
            CreatedBy = createdBy;
            // Split the ingredients string and trim each element (similar to map in JavaScript)
            Ingredients = ingredients.Split(',').Select(i => i.Trim()).ToList();
        }

        public void EditRecipe(string title, string instructions, string category, string ingredients)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) ||
                    string.IsNullOrWhiteSpace(instructions) ||
                    string.IsNullOrWhiteSpace(category) ||
                    string.IsNullOrWhiteSpace(ingredients))
                {
                    MessageBox.Show("All fields must be filled in before saving.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Filter out blank entries (equivalent to SQL WHERE)
                var ingredientList = ingredients.Split(',')
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .ToList();

                if (ingredientList.Count == 0)
                {
                    MessageBox.Show("At least one ingredient must be specified.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Title = title.Trim();
                Instructions = instructions.Trim();
                Category = category.Trim();
                Ingredients = ingredientList;

                MessageBox.Show("Recipe updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Instructions));
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(Ingredients));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while editing the recipe.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Recipe CopyRecipe(User newOwner = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title) ||
                    string.IsNullOrWhiteSpace(Instructions) ||
                    string.IsNullOrWhiteSpace(Category) ||
                    Ingredients == null || Ingredients.Count == 0)
                {
                    MessageBox.Show("Cannot copy recipe — some fields are empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                var copy = new Recipe(
                    $"{Title} (Copy)",
                    Instructions,
                    Category,
                    DateTime.Now,
                    newOwner ?? CreatedBy,
                    string.Join(", ", Ingredients)
                );

                return copy;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while copying the recipe.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
