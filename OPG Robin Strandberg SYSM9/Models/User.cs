using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OPG_Robin_Strandberg_SYSM9.Models
{
    public class User : INotifyPropertyChanged
    {
        private string _userName;
        private string _password;
        private string _country;

        public string UserName
        {
            get => _userName;
            private set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            private set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string Country
        {
            get => _country;
            private set
            {
                _country = value;
                OnPropertyChanged();
            }
        }

        public int Id { get; private set; }

        public ObservableCollection<Recipe> RecipeList { get; set; }

        public string SecretQuestion { get; private set; }
        public string SecretAnswer { get; private set; }

        public virtual bool IsAdmin => false;

        // Required by EF Core to instantiate objects when loading from the database
        protected User()
        {
            RecipeList = new ObservableCollection<Recipe>();
        }

        public User(string userName, string password, string country)
        {
            _userName = userName;
            _password = password;
            _country = country;
            RecipeList = new ObservableCollection<Recipe>();
        }

        public void SetSecretQuestion(string question, string answer)
        {
            SecretQuestion = question?.Trim();
            SecretAnswer = answer?.Trim();
        }

        public virtual bool ValidateLogin(string userName, string password)
        {
            return _userName == userName && _password == password;
        }

        public virtual bool ChangeUserName(string newUserName)
        {
            if (string.IsNullOrWhiteSpace(newUserName) || newUserName.Length < 3)
                return false;

            UserName = newUserName.Trim();
            return true;
        }

        public virtual bool ChangePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 5)
                return false;

            Password = newPassword.Trim();
            return true;
        }

        public virtual bool UpdateDetails(string newCountry)
        {
            if (string.IsNullOrWhiteSpace(newCountry))
                return false;

            Country = newCountry.Trim();
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
