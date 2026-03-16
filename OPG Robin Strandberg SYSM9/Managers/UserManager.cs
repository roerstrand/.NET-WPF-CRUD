using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using OPG_Robin_Strandberg_SYSM9.Data;
using OPG_Robin_Strandberg_SYSM9.Models;

namespace OPG_Robin_Strandberg_SYSM9.Managers
{
    public class UserManager : INotifyPropertyChanged
    {
        private readonly CookMasterDbContext _db;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        private User _searchedUser;

        public User SearchedUser
        {
            get => _searchedUser;
            set
            {
                _searchedUser = value;
                OnPropertyChanged();
            }
        }

        private readonly Dictionary<User, RecipeManager> _userRecipeManagers = new();

        public List<User> Users { get; set; }

        public List<AdminUser> ActiveAdmins { get; private set; } = new();

        private bool _isAuthenticated;

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                if (_isAuthenticated != value)
                {
                    _isAuthenticated = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserManager(CookMasterDbContext db)
        {
            _db = db;

            // Load all users including their recipes from the database
            Users = _db.Users.Include(u => u.RecipeList).ToList();

            // Create default users if the database is empty
            if (!Users.Any())
                CreateDefaultUsers();
        }

        private void CreateDefaultUsers()
        {
            var adminUser = new AdminUser("admin", "password", "Sweden");
            adminUser.SetSecretQuestion("What was the name of your first pet?", "Kiruna");
            _db.Users.Add(adminUser);

            var normalUser = new User("user", "password", "Norway");
            normalUser.SetSecretQuestion("What city were you born in?", "Oslo");

            normalUser.RecipeList.Add(new Recipe(
                "Classic Pancakes",
                "Mix flour, milk, eggs, and butter. Fry in pan until golden.",
                "Breakfast",
                DateTime.Now,
                normalUser,
                "Flour, Milk, Eggs, Butter, Salt"
            ));

            normalUser.RecipeList.Add(new Recipe(
                "Spaghetti Bolognese",
                "Cook pasta. Prepare sauce with minced meat, tomatoes, and herbs. Combine and serve.",
                "Dinner",
                DateTime.Now,
                normalUser,
                "Spaghetti, Minced Meat, Tomato Sauce, Garlic, Onion, Herbs"
            ));

            _db.Users.Add(normalUser);
            _db.SaveChanges();

            // Reload with IDs assigned by the database
            Users = _db.Users.Include(u => u.RecipeList).ToList();
        }

        public bool Login(string username, string password)
        {
            try
            {
                foreach (User u in Users)
                {
                    if (u.UserName == username && u.Password == password)
                    {
                        if (!PerformTwoFactorAuthentication(u))
                            return false;

                        CurrentUser = u;
                        IsAuthenticated = true;
                        GetRecipeManagerForCurrentUser();

                        if (u is AdminUser admin && !ActiveAdmins.Contains(admin))
                        {
                            ActiveAdmins.Add(admin);
                            MessageBox.Show($"Welcome administrator {u.UserName}!",
                                "Login successful", MessageBoxButton.OK, MessageBoxImage.Information);
                            OnPropertyChanged(nameof(IsAuthenticated));
                            return true;
                        }

                        MessageBox.Show($"Welcome {u.UserName}!",
                            "Login successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        OnPropertyChanged(nameof(IsAuthenticated));
                        return true;
                    }
                }

                IsAuthenticated = false;
                OnPropertyChanged(nameof(IsAuthenticated));
                MessageBox.Show("Incorrect username or password.",
                    "Login failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show("Unexpected error occurred while trying to log in.",
                    "System error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public RecipeManager GetRecipeManagerForCurrentUser()
        {
            try
            {
                if (CurrentUser == null)
                    return null;

                if (!_userRecipeManagers.ContainsKey(CurrentUser))
                    _userRecipeManagers[CurrentUser] = new RecipeManager(CurrentUser, _db);

                return _userRecipeManagers[CurrentUser];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public void Logout()
        {
            if (CurrentUser is AdminUser admin && ActiveAdmins.Contains(admin))
                ActiveAdmins.Remove(admin);

            CurrentUser = null;
            IsAuthenticated = false;
            OnPropertyChanged(nameof(IsAuthenticated));
        }

        public bool ChangePassword(User user, string newPassword)
        {
            try
            {
                bool result = user.ChangePassword(newPassword);
                if (result)
                    _db.SaveChanges();
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while changing password:\n" + ex.Message,
                    "System error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool Register(string username, string password, string country, string secretQuestion, string secretAnswer)
        {
            try
            {
                if (Users.Any(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Username already taken. Please choose another.",
                        "Username taken", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                string pattern = @"^(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>])[A-Za-z\d!@#$%^&*(),.?""':{}|<>]{8,}$";
                if (!Regex.IsMatch(password, pattern))
                {
                    MessageBox.Show(
                        "Password must be 8 symbols long, contain at least one digit and one special character.",
                        "Not allowed password", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(secretQuestion) || string.IsNullOrWhiteSpace(secretAnswer))
                {
                    MessageBox.Show("Please select a secret question and provide an answer.",
                        "Secret question required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var newUser = new User(username, password, country);
                newUser.SetSecretQuestion(secretQuestion, secretAnswer);

                _db.Users.Add(newUser);
                _db.SaveChanges();
                Users.Add(newUser);

                MessageBox.Show("Registration succeeded!!", "Done",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured during registration:\n" + ex.Message,
                    "System error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool IsUsernameTaken(string newUserName)
        {
            try
            {
                return Users.Any(u => u.UserName.Equals(newUserName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show("Unexpected error when checking username availability.");
                return false;
            }
        }

        public User FindUser(string username)
        {
            try
            {
                foreach (User u in Users)
                {
                    if (u.UserName == username)
                        return SearchedUser = u;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private string _lastGeneratedCode;

        private bool PerformTwoFactorAuthentication(User user)
        {
            try
            {
                var random = new Random();
                _lastGeneratedCode = random.Next(100000, 999999).ToString();

                MessageBox.Show(
                    $"Simulated email sent to {user.UserName}@example.com\nVerification code: {_lastGeneratedCode}",
                    "Two-Factor Authentication", MessageBoxButton.OK, MessageBoxImage.Information);

                var twoFactorWindow = new Views.TwoFactorWindow(_lastGeneratedCode);
                bool? result = twoFactorWindow.ShowDialog();
                return result == true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during two-factor authentication:\n{ex.Message}",
                    "System error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
