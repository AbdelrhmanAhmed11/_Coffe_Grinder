using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// Interaction logic for Admin_login.xaml
    /// </summary>
    public partial class Admin_login : Page
    {
       Coffe_Grinder_DBEntities db = new Coffe_Grinder_DBEntities();
        public Admin_login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            var user = db.Users.Where(u => u.Username == username && u.Password == password).FirstOrDefault();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please Enter Data!!");
                return;
            }

            if (user != null)
            {
                if(user.Role=="Admin")
                {
                    MessageBox.Show("Welcome Admin!");
                    Adminhomepage adpg = new Adminhomepage();
                    this.NavigationService.Navigate(adpg);
                }
                else if(user.Role=="User")
                {
                    MessageBox.Show("Please Enter Admin Data Only!!");
                }
            }
            else
            {
                MessageBox.Show("Invalid Username Or Password!!!");
            }
        }
    }
}
