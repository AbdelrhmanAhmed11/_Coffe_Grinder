using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Coffe_Grinder
{
    /// <summary>
    /// Interaction logic for welcome.xaml
    /// </summary>
    public partial class welcome : Page
    {
        public welcome()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Admin_login admin_Login = new Admin_login();
            this.NavigationService.Navigate(admin_Login);
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CreateOrderPage());
        }
    }
}
