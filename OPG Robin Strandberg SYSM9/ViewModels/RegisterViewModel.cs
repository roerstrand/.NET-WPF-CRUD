using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OPG_Robin_Strandberg_SYSM9.Commands;
using OPG_Robin_Strandberg_SYSM9.Managers;
using OPG_Robin_Strandberg_SYSM9.Views;

namespace OPG_Robin_Strandberg_SYSM9.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly UserManager _userManager;
        private string _username;
        private string _selectedCountry;
        private string _selectedSecretQuestion;
        private string _secretAnswer;

        public List<string> Countries { get; }

        public List<string> SecretQuestions { get; } = new List<string>
        {
            "What was the name of your first pet?",
            "What city were you born in?",
            "What is your mother's maiden name?",
            "What was the name of your elementary school?",
            "What is your favorite book?",
            "What was your childhood nickname?",
            "What street did you grow up on?",
            "What is your favorite movie?"
        };

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                _selectedCountry = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSecretQuestion
        {
            get => _selectedSecretQuestion;
            set
            {
                _selectedSecretQuestion = value;
                OnPropertyChanged();
            }
        }

        public string SecretAnswer
        {
            get => _secretAnswer;
            set
            {
                _secretAnswer = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public RegisterViewModel()
        {
            _userManager = App.UserManager;

            Countries = new List<string>()
            {
                "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda",
                "Argentina", "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas",
                "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize",
                "Benin", "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana",
                "Brazil", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia",
                "Cameroon", "Canada", "Chile", "China", "Colombia", "Costa Rica", "Croatia",
                "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominican Republic",
                "Ecuador", "Egypt", "El Salvador", "Estonia", "Ethiopia", "Finland",
                "France", "Germany", "Ghana", "Greece", "Greenland", "Guatemala", "Honduras",
                "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland",
                "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya",
                "Kuwait", "Latvia", "Lebanon", "Liberia", "Libya", "Lithuania", "Luxembourg",
                "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Mexico",
                "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique",
                "Namibia", "Nepal", "Netherlands", "New Zealand", "Nicaragua", "Niger",
                "Nigeria", "North Korea", "Norway", "Oman", "Pakistan", "Palestine",
                "Panama", "Paraguay", "Peru", "Philippines", "Poland", "Portugal",
                "Qatar", "Romania", "Russia", "Rwanda", "Saudi Arabia", "Senegal",
                "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia",
                "South Africa", "South Korea", "Spain", "Sri Lanka", "Sudan", "Sweden",
                "Switzerland", "Syria", "Taiwan", "Tanzania", "Thailand", "Tunisia",
                "Turkey", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom",
                "United States", "Uruguay", "Uzbekistan", "Venezuela", "Vietnam", "Yemen",
                "Zambia", "Zimbabwe"
            };
            RegisterCommand = new RelayCommand(Register);
            BackToLoginCommand = new RelayCommand(BackToLogin);
        }

        private void Register(object parameter)
        {
            if (parameter is not Window window)
                return;

            var pw1 = window.FindName("PasswordBox") as PasswordBox;
            var pw2 = window.FindName("ConfirmPasswordBox") as PasswordBox;

            if (pw1 == null || pw2 == null)
            {
                MessageBox.Show("Password fields could not be read.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string password = pw1.Password.Trim();
            string confirm = pw2.Password.Trim();

            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Please fill in both password fields.",
                    "Missing Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Passwords do not match.",
                    "Mismatch", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _userManager.Register(Username, password, SelectedCountry, SelectedSecretQuestion, SecretAnswer);

            if (success)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                CloseCurrentWindow();
            }
        }

        private void BackToLogin(object parameter)
        {
            var window = new MainWindow();
            window.Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
