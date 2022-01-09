using SvadbeniSalon.WinUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SvadbeniSalon.WinUI.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            Loaded += Login_Loaded;

        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ICloseWindow vm)
            {
                vm.Close += () =>
                {
                    this.DialogResult = true;
                    this.Close();
                };
            }
        }
    }
}
