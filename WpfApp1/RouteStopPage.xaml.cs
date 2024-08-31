using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using static WpfApp1.RouteStopPage;
using System.Security.Cryptography;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RouteStopPage.xaml
    /// </summary>
    public partial class RouteStopPage : Page
    {
        public RouteStopPage()
        {
            InitializeComponent();
            Init();
        }
        public class RouteStopCont : INotifyPropertyChanged
        {
            private string stop;
            private string route_no;
            private string travel_time;
            private string prev_stop;
            public string Stop
            {
                get { return stop; }
                set
                {
                    stop = value;
                    OnPropertyChanged("Stop");
                }
            }
            public string Route_No
            {
                get { return route_no; }
                set
                {
                    route_no = value;
                    OnPropertyChanged("Route_No");
                }
            }
            public string Travel_time
            {
                get { return travel_time; }
                set
                {
                    travel_time = value;
                    OnPropertyChanged("Travel_time");
                }
            }
            public string Prev_stop
            {
                get { return prev_stop; }
                set
                {
                    prev_stop = value;
                    OnPropertyChanged("Prev_stop");
                }
            }
            public RouteStopCont(string r, string c)
            {
                Stop = r;
                Route_No = c;
            }
            public RouteStopCont(string r, string c, string s, string st)
            {
                Stop = r;
                Route_No = c;
                Travel_time = s;
                Prev_stop = st;
            }
            public RouteStopCont() { }
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
                string sql = "select Route_No, (select Stop_Addres from Stopps where Id = stop) AS Stop, Stop as StopId, Travel_time," +
                             " (select Stop_Addres from Stopps where Id = Previous_Stop) as Prev_Stop, Previous_Stop as Previous_Stop_Id from Route_Stop";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                RouteStopDG.ItemsSource = ds.DefaultView;
            }
            if (inserts == null)
            {
                RouteStopInsertDG.ItemsSource = inserts = new ObservableCollection<RouteStopCont>() { new RouteStopCont("", ""), new RouteStopCont("", ""), new RouteStopCont("", ""), new RouteStopCont("", "") };
                RouteStopDeleteDG.ItemsSource = deletes = new ObservableCollection<RouteStopCont>() { new RouteStopCont("", ""), new RouteStopCont("", ""), new RouteStopCont("", ""), new RouteStopCont("", "") };
                RouteStopOldUpdateDG.ItemsSource = updatesOld = new ObservableCollection<RouteStopCont>() { new RouteStopCont("", "") };
                RouteStopNewUpdateDG.ItemsSource = updatesNew = new ObservableCollection<RouteStopCont>() { new RouteStopCont("", "") };
            }
        }
        ObservableCollection<RouteStopCont> inserts;
        ObservableCollection<RouteStopCont> deletes;
        ObservableCollection<RouteStopCont> updatesOld;
        ObservableCollection<RouteStopCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = RouteStopInsertDG.ItemsSource;
            string insert = "Insert into RouteStop (Stop, Route_No, Travel_time, Previous_Stop, Previous_Route) values ";
            int i = 0;
            foreach (RouteStopCont d in data)
            {
                if (d.Stop != "")
                {
                    insert = i > 0 ? insert + ", " : insert;
                    insert += "( " + d.Stop + "";
                    if (d.Route_No != "")
                    {
                        insert += ", " + d.Route_No + "";
                        if (d.Travel_time != "") 
                        {
                            insert += ", " + d.Travel_time + "";
                            insert += ", " + (d.Prev_stop == "" ? "Null" : d.Prev_stop) + "" + ", " + d.Route_No + ")";
                        }
                        else
                        {
                            MessageBox.Show("Значения не добавлены\n\nОшибка в " + i + "-м столбце");
                            return;
                        }
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
            inserts.Add(new RouteStopCont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = RouteStopDeleteDG.ItemsSource;
            string del = "Delete from Route_Stop where ";
            int i = 0;
            bool b = false;
            foreach (RouteStopCont d in data)
            {
                if (d.Stop != "")
                {
                    del = i > 0 ? del + " OR " : del;
                    del += " Stop = " + d.Stop + " ";
                    b = true;
                }
                if (d.Route_No != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Route_No = " + d.Route_No;
                    b = true;
                }
                if (d.Travel_time != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Travel_time = " + d.Travel_time;
                    b = true;
                }
                if (d.Prev_stop != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Previous_Stop = " + d.Prev_stop;
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
            deletes.Add(new RouteStopCont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (RouteStopSearchRoute.Text != "" || RouteStopSearchStop.Text != "" || RouteStopSearchTrvl.Text != "" || RouteStopSearchPrevSt.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "select Route_No, (select Stop_Addres from Stopps where Id = stop) AS Stop, Stop as StopId, Travel_time," +
                             " (select Stop_Addres from Stopps where Id = Previous_Stop) as Prev_Stop, Previous_Stop as Previous_Stop_Id from Route_Stop Where ";
                    bool b = false;
                    if (RouteStopSearchRoute.Text != "")
                    {
                        sql += " Route_No = " + RouteStopSearchRoute.Text;
                        b = true;
                    }
                    if (RouteStopSearchStop.Text != "")
                    {
                        sql += b ? "," : "";
                        b = true;
                        sql += " Stop = " + RouteStopSearchStop.Text;
                    }
                    if (RouteStopSearchTrvl.Text != "")
                    {
                        sql += b ? "," : "";
                        b = true;
                        sql += " Travel_Time = '" + RouteStopSearchTrvl.Text + "'";
                    }
                    if (RouteStopSearchPrevSt.Text != "")
                    {
                        sql += b ? "," : "";
                        b = true;
                        sql += " Previous_Stop = " + RouteStopSearchPrevSt.Text;
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

                        RouteStopDG.ItemsSource = ds.DefaultView;
                    }
                    catch { }
                }
            }
        }
        public void SetUserMode()
        {
            RouteStopInsertItem.Visibility = Visibility.Collapsed;
            RouteStopDeleteItem.Visibility = Visibility.Collapsed;
            RouteStopUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new RouteStopCont("", ""));
            updatesNew.Add(new RouteStopCont("", ""));
        }
        private void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<RouteStopCont>();
            var New = new List<RouteStopCont>();
            foreach (RouteStopCont old in RouteStopOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (RouteStopCont n in RouteStopNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update RouteStopPage set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Travel_time == "" || Old[i].Route_No == "") && (New[i].Travel_time == "" || New[i].Route_No == "" || New[i].Stop == ""))
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
                    var d = New[i];
                    if (d.Stop != "")
                    {
                        updates[i] += " Stop = " + d.Stop + " ";
                        b = true;
                    }
                    if (d.Route_No != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Route_No = " + d.Route_No;
                        b = true;
                    }
                    if (d.Travel_time != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Travel_time = " + d.Travel_time;
                        b = true;
                    }
                    if (d.Prev_stop != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Previous_Stop = " + d.Prev_stop;
                        b = true;
                    }
                    updates[i] += " Where ";
                    b = false;
                    d = Old[i];
                    if (d.Stop != "")
                    {
                        updates[i] += " Stop = " + d.Stop + " ";
                        b = true;
                    }
                    if (d.Route_No != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Route_No = " + d.Route_No;
                        b = true;
                    }
                    if (d.Travel_time != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Travel_time = " + d.Travel_time;
                        b = true;
                    }
                    if (d.Prev_stop != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Previous_Stop = " + d.Prev_stop;
                        b = true;
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