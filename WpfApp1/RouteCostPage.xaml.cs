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
using static WpfApp1.RouteCostPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RouteStopPage.xaml
    /// </summary>
    public partial class RouteCostPage : Page
    {
        public RouteCostPage()
        {
            InitializeComponent();
            Init();
        }
        public class RouteCostCont : INotifyPropertyChanged
        {
            private string id;
            private string route;
	        private string payment;

 
            public string Id
            {
                get { return id; }
                set
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }

            public string Route 
            {
                get { return route; }
                set
                {
                    route = value;
                    OnPropertyChanged("Route");
                }
            }
     	    public string Payment
            {
                get { return payment; }
                set
                {
                    payment = value;
                    OnPropertyChanged("Payment");
                }
            }
            public RouteCostCont(string r, string c)
            {
                Id = r;
                Route = c;
            }
            public RouteCostCont(string r, string c, string s)
            {
                Id = r;
                Route = c;		
		Payment = s;
            }
            public RouteCostCont(){}
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
                string sql = "Select Id, Route_No as Адрес, Payment as Тип_Оплаты from Cost_Route";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                RouteCostDG.ItemsSource = ds.DefaultView;
            }
            if (inserts == null)
            {
                RouteCostInsertDG.ItemsSource = inserts = new ObservableCollection<RouteCostCont>() { new RouteCostCont("", ""), new RouteCostCont("", ""), new RouteCostCont("", ""), new RouteCostCont("", "") };
                RouteCostDeleteDG.ItemsSource = deletes = new ObservableCollection<RouteCostCont>() { new RouteCostCont("", ""), new RouteCostCont("", ""), new RouteCostCont("", ""), new RouteCostCont("", "") };
                RouteCostOldUpdateDG.ItemsSource = updatesOld = new ObservableCollection<RouteCostCont>() { new RouteCostCont("", "") };
                RouteCostNewUpdateDG.ItemsSource = updatesNew = new ObservableCollection<RouteCostCont>() { new RouteCostCont("", "") };
            }
        }
        ObservableCollection<RouteCostCont> inserts;
        ObservableCollection<RouteCostCont> deletes;
        ObservableCollection<RouteCostCont> updatesOld;
        ObservableCollection<RouteCostCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = RouteCostInsertDG.ItemsSource;
            string insert = "Insert into Cost_Route (Route, Payment) values ";
            int i = 0;
            foreach (RouteCostCont d in data)
            {
                if (d.Route != "")
                {
                    insert = i > 0 ? insert + ", " : insert;
                    insert += "( '" + d.Route + "'";
                    if (d.Payment != "")
                    {
                        insert += ", '" + d.Payment + "')";
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
            inserts.Add(new RouteCostCont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = RouteCostDeleteDG.ItemsSource;
            string del = "Delete from Cost_Route where ";
            int i = 0;
            bool b = false;
            foreach (RouteCostCont d in data)
            {
                if (d.Id != "")
                {
                    del = i > 0 ? del + " OR " : del;
                    del += " Id = " + d.Id + " ";
                    b = true;
                }
                if (d.Route != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Route = " + d.Route;
                    b = true;
                }
                if (d.Route != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Payment = " + d.Route;
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
            deletes.Add(new RouteCostCont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (RouteCostSearchPayment.Text != "" || RouteCostSearchRoute.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select Id, Route as Адрес, Payment as Тип_Оплаты from Route_Cost Where";
                    bool b = false;
                    if (RouteCostSearchPayment.Text != "") {
                        sql = " Payment = '" + RouteCostSearchPayment.Text + "'";
                        b = true;
                    }
                    if (RouteCostSearchRoute.Text != "")
                    {
                        sql += b ? "," : "";
                        sql += " Route = '" + RouteCostSearchRoute.Text + "'";
                    }

                    SqlCommand cmd = new SqlCommand();
                    connection.Open();
                    try
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = sql;
                        SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                        DataTable ds = new DataTable();
                        adapter.Fill(ds);

                        RouteCostDG.ItemsSource = ds.DefaultView;
                    }
                    catch { }
                }
            }
        }
        public void SetUserMode() {
            RouteCostInsertItem.Visibility = Visibility.Collapsed;
            RouteCostDeleteItem.Visibility = Visibility.Collapsed;
            RouteCostUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new RouteCostCont("", ""));
            updatesNew.Add(new RouteCostCont("", ""));
        }
        private void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<RouteCostCont>();
            var New = new List<RouteCostCont>();
            foreach (RouteCostCont old in RouteCostOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (RouteCostCont n in RouteCostNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update Cost_Route set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Payment == "" || Old[i].Route == "") && (New[i].Payment == "" || New[i].Route == "" || New[i].Id == ""))
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
                    if (New[i].Id != "")
                    {
                        updates[i] = upd + "Payment = " + New[i].Id + "";
                        b = true;
                    }
                    if (New[i].Route != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " Route = '" + New[i].Route + "'";
                        b = true;
                    }
                    updates[i] += " Where ";
                    b = false;
                    if (Old[i].Id != "")
                    {
                        updates[i] = upd + "Payment = " + Old[i].Id + "";
                        b = true;
                    }
                    if (Old[i].Route != "@")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Route = '" + Old[i].Route + "'";
                    }
                    b = false;
                }
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                connection.Open();
                foreach (string str in updates)
                {
                    SqlCommand cmd = new SqlCommand(upd + str, connection);
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
