using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfApp1.RoutesPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RoutesPage.xaml
    /// </summary>
    public partial class RoutesPage : Page
    {
        public RoutesPage()
        {
            InitializeComponent();
            Init();
        }
        public class RoutesCont : INotifyPropertyChanged
        {
            private string route;
            private string company;

            public string Route
            {
                get { return route; }
                set
                {
                    route = value;
                    OnPropertyChanged("Route");
                }
            }

            public string Company
            {
                get { return company; }
                set
                {
                    company = value;
                    OnPropertyChanged("Company");
                }
            }

            public RoutesCont(string r, string c)
            {
                Route = r;
                Company = c;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged([CallerMemberName] string prop = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        protected void Init()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                string sql = "Select Route_No as Маршрут, Company as Компания from Routes";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                RoutesDG.ItemsSource = ds.DefaultView;
            }
            RoutesInsertDG.ItemsSource = inserts = new ObservableCollection<RoutesCont>() { new RoutesCont("",""), new RoutesCont("", "") , new RoutesCont("", "") , new RoutesCont("", "") };
            RoutesDeleteDG.ItemsSource = deletes = new ObservableCollection<RoutesCont>() { new RoutesCont("", ""), new RoutesCont("", ""), new RoutesCont("", ""), new RoutesCont("", "") };
            RoutesOldUpdateDG.ItemsSource = updatesOld = new ObservableCollection<RoutesCont>() { new RoutesCont("", "") };
            RoutesNewUpdateDG.ItemsSource = updatesNew = new ObservableCollection<RoutesCont>() { new RoutesCont("", "") };
        }
        ObservableCollection<RoutesCont> inserts;
        ObservableCollection<RoutesCont> deletes;
        ObservableCollection<RoutesCont> updatesOld;
        ObservableCollection<RoutesCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = RoutesInsertDG.ItemsSource;
            string insert = "Insert into Routes (Route_No, Company) values ";
            int i = 0;
            foreach (RoutesCont d in data)
            {
                if (d.Route != "")
                {
                    insert = i > 0 ? insert + ", " : insert;
                    insert += "( " + d.Route;
                    if (d.Company != "")
                    {
                        insert += ", " + d.Company + ")";
                    }
                    else
                    {
                        MessageBox.Show("Значения не добавлены\nОшибка в " + i + "-м столбце");
                        return;
                    }
                }
                i++;
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(insert, connection);
                try
                {
                    cmd.ExecuteNonQuery();
                    Init();
                    MessageBox.Show("Значения добавлены");
                }
                catch { MessageBox.Show("Значения не добавлены"); }
            }
        }
        private void AddRowInsert(object sender, RoutedEventArgs e)
        {
            inserts.Add(new RoutesCont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = RoutesDeleteDG.ItemsSource;
            string del = "Delete from Routes where ";
            int i = 0;
            bool b = false;
            foreach (RoutesCont d in data)
            {
                if (d.Route != "")
                {
                    del = i > 0 ? del + " OR " : del;
                    del += " Route_No = " + d.Route + " ";
                    b = true;
                }
                if (d.Company != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Company = " + d.Company;
                    b = true;
                }
                b = false;
                i++;
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(del, connection);
                try
                {
                    cmd.ExecuteNonQuery();
                    Init();
                    MessageBox.Show("Значения удалены");
                }
                catch { MessageBox.Show("Значения не удалены"); }
            }
        }
        private void AddRowDelete(object sender, RoutedEventArgs e)
        {
            deletes.Add(new RoutesCont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if(!Int32.TryParse(RoutesSearchCost.Text, out int x) && RoutesSearchCost.Text != "")
            {
                MessageBox.Show("Неправильное значение");
                return;
            }
            if (RoutesSearchType.Text != "" || RoutesSearchCost.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select Route_No as Маршрут, Company as Компания from Routes Where";
                    if (RoutesSearchType.Text != "") {
                        sql += " Route_No = " + RoutesSearchType.Text;
                    }
                    if (RoutesSearchCost.Text != "")
                    {
                        sql += " Company = '" + RoutesSearchCost.Text + "'";
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    RoutesDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        public void SetUserMode() {
            RoutesInsertItem.Visibility = Visibility.Collapsed;
            RoutesDeleteItem.Visibility = Visibility.Collapsed;
            RoutesUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new RoutesCont("", ""));
            updatesNew.Add(new RoutesCont("", ""));
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<RoutesCont>();
            var New = new List<RoutesCont>();
            foreach (RoutesCont old in RoutesOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (RoutesCont n in RoutesNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update Routes set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Route == "" && Old[i].Company == "") || (New[i].Route == "" && New[i].Company == ""))
                {
                    if (j != 0) {
                        MessageBox.Show("Значения не добавлены\n\nОшибка в " + i + "-м столбце");
                        return;
                    }
                }
                else
                {
                    j = 1;
                    updates.Add("");
                    if (New[i].Route != "")
                    {
                        updates[i] = upd + "Route_No = " + New[i].Route + "";
                        b = true;
                    }
                    if (New[i].Company != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " Company = '" + New[i].Company + "'";
                        b = true;
                    }
                    b = false;
                    updates[i] += " Where ";
                    if (Old[i].Route != "@")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] = upd + "Route_No = " + Old[i].Route + "' ";
                        b = true;
                    }
                    if (Old[i].Company != "@")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Company = '" + Old[i].Company + "'";
                    }
                    b = false;
                }
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                connection.Open();
                foreach (string str in updates)
                {
                    SqlCommand cmd = new SqlCommand(str, connection);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        Init();
                        MessageBox.Show("Значения изменены");
                    }
                    catch { MessageBox.Show("Значения не изменены"); }
                }
            }
        }
    }
}
