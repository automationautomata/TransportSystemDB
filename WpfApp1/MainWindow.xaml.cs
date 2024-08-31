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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для StartWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MenuWindow mainWindow = new MenuWindow();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (Username.Text == "admin")
                mainWindow.isAdmin = true;
            else
                mainWindow.isAdmin = false;

                mainWindow.ShowDialog();

        }

        //private void Btn_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //    mainWindow.ShowDialog();
        //}

        //private void tb_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //}
    }
}
