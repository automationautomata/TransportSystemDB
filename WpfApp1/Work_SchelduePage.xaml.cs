using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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
using static WpfApp1.RoutesPage;
using static WpfApp1.Work_SchelduePage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Work_SchelduePage.xaml
    /// </summary>
    public partial class Work_SchelduePage : Page
    {
        public Work_SchelduePage()
        {
            InitializeComponent();
            Init();
            SearchCompany.Visibility = Visibility.Collapsed;
        }
        public class Work_ScheldueCont: INotifyPropertyChanged
        {
            private string _driver;
            private string _transport;
            private string _additionalStaff;
            private string _routeNumber;
            private string _startTime;

            public string Driver
            {
                get { return _driver; }
                set
                {
                    _driver = value;
                    OnPropertyChanged("Driver");
                }
            }

            public string Transport
            {
                get { return _transport; }
                set
                {
                    _transport = value;
                    OnPropertyChanged("Transport");
                }
            }

            public string Ad_Stf
            {
                get { return _additionalStaff; }
                set
                {
                    _additionalStaff = value;
                    OnPropertyChanged("Ad_Stf");
                }
            }

            public string Route_No
            {
                get { return _routeNumber; }
                set
                {
                    _routeNumber = value;
                    OnPropertyChanged("Route_No");
                }
            }

            public string Start_Time
            {
                get { return _startTime; }
                set
                {
                    _startTime = value;
                    OnPropertyChanged("Start_Time");
                }
            }

            public Work_ScheldueCont(string driver, string transport, string additionalStaff, string routeNumber, string startTime)
            {
                Driver = driver;
                Transport = transport;
                Ad_Stf = additionalStaff;
                Route_No = routeNumber;
                Start_Time = startTime;
            }
            public Work_ScheldueCont() { }
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected void Init()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                //string sql = "Select Work_Scheldue.Id, Route_No as Маршрут, Transport as Транспорт," +
                //            " Drivers.Name as Водитель, Addictional_Staff.Name as Кондуктор, Start_Time as Время начала работы " +
                //            "from Work_Scheldue inner join Drivers on Drivers.Id = Work_Scheldue.Driver inner join " +
                //            "Addictional_Staff on Work_Scheldue.Addictional_Staff = Addictional_Staff.Id";
                string sql = "select * from View_WS";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                WSDG.ItemsSource = ds.DefaultView;
            }
            WSInsertDG.ItemsSource = inserts = new ObservableCollection<Work_ScheldueCont>() { new Work_ScheldueCont("", "", "", "", ""), new Work_ScheldueCont("", "", "", "", ""), new Work_ScheldueCont("", "", "", "", ""), new Work_ScheldueCont("", "", "", "", "") };
            WSDeleteDG.ItemsSource = deletes = new ObservableCollection<Work_ScheldueCont>() { new Work_ScheldueCont("", "", "", "", ""), new Work_ScheldueCont("", "", "", "", ""), new Work_ScheldueCont("", "", "", "", ""), new Work_ScheldueCont("", "", "", "", "") };
            WSDeleteDG.ItemsSource = updatesOld = new ObservableCollection<Work_ScheldueCont>() { new Work_ScheldueCont("", "", "", "", "") };
            WSDeleteDG.ItemsSource = updatesNew = new ObservableCollection<Work_ScheldueCont>() { new Work_ScheldueCont("", "", "", "", "") };
        }
        ObservableCollection<Work_ScheldueCont> inserts;
        ObservableCollection<Work_ScheldueCont> deletes;
        ObservableCollection<Work_ScheldueCont> updatesOld;
        ObservableCollection<Work_ScheldueCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            WSInsertDG.SelectAll();
            var data = WSInsertDG.SelectedItems;
            string insert = "EXEC Insert_in_WS";
            int i = 0;
            string tmp;
            foreach (Work_ScheldueCont d in data)
            {
                tmp = insert;
                if (d.Driver == "" || d.Transport != "" || d.Ad_Stf != "" || d.Route_No != "" || d.Start_Time != "")
                {
                    MessageBox.Show("Значения не добавлены\n\nОшибка в" + i + "-м столбце");
                    return;
                }
                else tmp += " '" + d.Start_Time + "', '" + d.Driver + "', '" + d.Ad_Stf + "', '" + d.Transport + "', " + d.Route_No;

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
        }
        private void AddRowInsert(object sender, RoutedEventArgs e)
        {
            inserts.Add(new Work_ScheldueCont("", "", "", "", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            WSInsertDG.SelectAll();
            var data = WSInsertDG.SelectedItems;
            string del = "Delete from Work_Scheldue where ";
            int i = 0;
            bool b = false;
            foreach (Work_ScheldueCont d in data)
            {
                if (d.Route_No != "")
                {
                    del = i > 0 ? del + " OR " : del;
                    del += " Route_No = " + d.Route_No + " ";
                    b = true;
                }
                if (d.Transport != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Transport = '" + d.Transport + "'";
                    b = true;
                }
                if (d.Driver != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Driver = '" + d.Driver + "'";
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
                    MessageBox.Show("Значения добавлены");
                }
                catch { MessageBox.Show("Значения не добавлены"); }
            }
        }
        private void AddRowDelete(object sender, RoutedEventArgs e)
        {
            deletes.Add(new Work_ScheldueCont("", "", "", "", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            //if(!Int32.TryParse("", out int x))
            //{
            //    MessageBox.Show("Неправильное значение Id");
            //    return;
            //}
            bool b = false;
            if (WSSearchRoute.Text != "" || WSSearchTransport.Text != "" || WSSearchDriver.Text != "" || 
                WSSearchAd_Stf.Text != "" || WSSearchAd_Stf.Text != "" || WSSearchST.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select * from View_WS_Cmpn Where ";
                    if (WSSearchRoute.Text != "")
                    {
                        sql += " Маршрут = " + WSSearchRoute.Text + " ";
                    }
                    if (WSSearchTransport.Text != "")
                    {
                        sql += b ? "And" : "";
                        b = true;
                        sql += " Транспорт = '" + WSSearchTransport.Text + "' ";
                    }
                    if (WSSearchDriver.Text != "")
                    {
                        sql += b ? "And" : "";
                        b = true;
                        sql += " Водитель = '" + WSSearchDriver.Text + "' ";
                    }
                    if (WSSearchAd_Stf.Text != "")
                    {
                        sql += b ? "And" : "";
                        b = true;
                        sql += " Кондуктор = '" + WSSearchAd_Stf.Text + "' ";
                    }
                    if (WSSearchST.Text != "")
                    {
                        sql += b ? "And" : "";
                        b = true;
                        sql += " Время_начала_работы = '" + WSSearchST.Text + "' ";
                    }
                    if (WSSearchCompany.Text != "")
                    {
                        sql += b ? "And" : "";
                        b = true;
                        sql += " Компания = '" + WSSearchCompany.Text + "'";
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    WSDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SearchCompany.Visibility = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                //string sql = "Select Work_Scheldue.Id, Route_No as Маршрут, Transport as Транспорт," +
                //            " Drivers.Name as Водитель, Addictional_Staff.Name as Кондуктор, Start_Time as Время начала работы " +
                //            "from Work_Scheldue inner join Drivers on Drivers.Id = Work_Scheldue.Driver inner join " +
                //            "Addictional_Staff on Work_Scheldue.Addictional_Staff = Addictional_Staff.Id";
                string sql = "select * from View_WS_Cmpn";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                WSDG.ItemsSource = ds.DefaultView;
            }
        }
        private void Report(object sender, RoutedEventArgs e)
        {
            var Rp = new ReportWindow();
            Rp.Show();
            using (SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TransportSystemDB;Integrated Security=True"))
            {
                var tmp = ReportDate.SelectedDate.ToString().Replace(".", "-").Replace("0:00:00", "");//DATEADD(YEAR, -10, getdate())
                string select = "DECLARE @d1 AS DATE = '" + tmp + "'\n" +
                                "EXEC WorkerStatistics @Begin = @d1";
                conn.Open();

                SqlCommand cmd = new SqlCommand();

                cmd.Connection = conn;
                cmd.CommandText = select;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Rp.ReportText.Text += reader.GetValue(0) + " " + reader.GetValue(1) + " " + reader.GetValue(2) + "\n";
                        }
                    }
                }
            }
        }
        public void SetUserMode()
        {
            WSInsertItem.Visibility = Visibility.Collapsed;
            WSDeleteItem.Visibility = Visibility.Collapsed;
            WSUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new Work_ScheldueCont("", "", "", "", ""));
            updatesNew.Add(new Work_ScheldueCont("", "", "", "", ""));
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<Work_ScheldueCont>();
            var New = new List<Work_ScheldueCont>();
            foreach (Work_ScheldueCont old in WSOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (Work_ScheldueCont n in WSNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update Routes set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Route_No == "" || Old[i].Driver == "" || Old[i].Start_Time == "" || Old[i].Ad_Stf == "" || Old[i].Transport == "") &&
                    (New[i].Route_No == "" || New[i].Driver == "" || New[i].Start_Time == "" || New[i].Ad_Stf == "" || New[i].Transport == ""))
                {
                    if (j != 0)
                    {
                        MessageBox.Show("Значения не добавлены\n\nОшибка в " + i + "-м столбце");
                        return;
                    }
                }
                else
                {
                    j = 1;
                    updates.Add("");
                    if (New[i].Route_No != "")
                    {
                        b = true;
                        updates[i] += " Маршрут = " + New[i].Route_No + " ";
                    }
                    if (New[i].Transport != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Транспорт = '" + New[i].Transport + "' ";
                    }
                    if (New[i].Driver != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Водитель = '" + New[i].Driver + "' ";
                    }
                    if (New[i].Ad_Stf != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Кондуктор = '" + New[i].Ad_Stf + "' ";
                    }
                    if (New[i].Start_Time != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Время_начала_работы = '" + New[i].Start_Time + "' ";
                    }
                    updates[i] += " Where ";
                    b = false;
                    if (Old[i].Route_No != "")
                    {
                        updates[i] += " Маршрут = " + Old[i].Route_No + " ";
                        b = true;
                    }
                    if (Old[i].Transport != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Транспорт = '" + Old[i].Transport + "' ";
                    }
                    if (Old[i].Driver != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Водитель = '" + Old[i].Driver + "' ";
                    }
                    if (Old[i].Ad_Stf != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Кондуктор = '" + Old[i].Ad_Stf + "' ";
                    }
                    if (Old[i].Start_Time != "")
                    {
                        updates[i] = b ? "And" : "";
                        b = true;
                        updates[i] += " Время_начала_работы = '" + Old[i].Start_Time + "' ";
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
