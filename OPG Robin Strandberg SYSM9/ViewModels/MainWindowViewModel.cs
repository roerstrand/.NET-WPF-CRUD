using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using OPG_Robin_Strandberg_SYSM9.Managers;
using OPG_Robin_Strandberg_SYSM9.Models;
using OPG_Robin_Strandberg_SYSM9.Views;
using OPG_Robin_Strandberg_SYSM9.Commands;
using OPG_Robin_Strandberg_SYSM9.ViewModels;
using System;
using System.Collections.Generic;

namespace OPG_Robin_Strandberg_SYSM9
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly UserManager _userManager;
        private RecipeManager _recipeManager;

        private string _userNameInput;
        private string _passwordInput;
        private User _currentUser;
        private object _currentView;

        private RecipeListViewModel _recipeListViewModel;

        public string UserNameInput
        {
            get => _userNameInput;
            set
            {
                _userNameInput = value;
                OnPropertyChanged();
            }
        }

        public string PasswordInput
        {
            get => _passwordInput;
            set
            {
                _passwordInput = value;
                OnPropertyChanged();
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowRegisterCommand { get; }
        public ICommand ShowForgotPasswordCommand { get; }

        public Action ClearPassword { get; set; }

        public ICommand LoginCommand { get; }

        public MainWindowViewModel()
        {
            _userManager = App.UserManager;

            LoginCommand = new RelayCommand(_ => Login_Button());
            ShowRegisterCommand = new RelayCommand(_ =>
            {
                var register = new RegisterWindow();
                register.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            });

            ShowForgotPasswordCommand = new RelayCommand(_ =>
            {
                try
                {
                    var forgot = new ForgotPasswordWindow();
                    forgot.Show();

                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is MainWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void Login_Button()
        {
            try
            {
                if (_userManager.Login(UserNameInput, PasswordInput))
                {
                    _userManager.IsAuthenticated = true;

                    WindowState previousState = WindowState.Normal;
                    MainWindow mainWin = null;
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is MainWindow mw)
                        {
                            previousState = mw.WindowState;
                            mainWin = mw;
                            break;
                        }
                    }

                    var recipeList = new RecipeListWindow(_userManager.GetRecipeManagerForCurrentUser());
                    recipeList.WindowState = previousState;
                    recipeList.Show();

                    mainWin?.Close();
                }
                else
                {
                    UserNameInput = string.Empty;
                    PasswordInput = string.Empty;
                    ClearPassword?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
