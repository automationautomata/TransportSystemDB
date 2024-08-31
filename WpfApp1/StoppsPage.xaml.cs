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
using static WpfApp1.StoppsPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RouteStopPage.xaml
    /// </summary>
    public partial class StoppsPage : Page
    {
        public StoppsPage()
        {
            InitializeComponent();
            Init();
        }
        public class StoppsCont : INotifyPropertyChanged
        {
            private string id;
            private string stop_addres;
	    private string stop_time;

 
            public string Id
            {
                get { return id; }
                set
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }

            public string Stop_Addres 
            {
                get { return stop_addres; }
                set
                {
                    stop_addres = value;
                    OnPropertyChanged("Stop_Addres");
                }
            }
     	    public string Stop_Time
            {
                get { return stop_time; }
                set
                {
                    stop_time = value;
                    OnPropertyChanged("Stop_Time");
                }
            }
            public StoppsCont(string r, string c)
            {
                Id = r;
                Stop_Addres = c;
            }
            public StoppsCont(string r, string c, string s)
            {
                Id = r;
                Stop_Addres = c;		
		Stop_Time = s;
            }
            public StoppsCont(){}
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
                string sql = "Select Id, Stop_Addres as Адрес, Stop_Time as Время_Остановки from Stopps";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                StoppsDG.ItemsSource = ds.DefaultView;
            }
            if (inserts == null)
            {
                StoppsInsertDG.ItemsSource = inserts = new ObservableCollection<StoppsCont>() { new StoppsCont("", ""), new StoppsCont("", ""), new StoppsCont("", ""), new StoppsCont("", "") };
                StoppsDeleteDG.ItemsSource = deletes = new ObservableCollection<StoppsCont>() { new StoppsCont("", ""), new StoppsCont("", ""), new StoppsCont("", ""), new StoppsCont("", "") };
                StoppsOldUpdateDG.ItemsSource = updatesOld = new ObservableCollection<StoppsCont>() { new StoppsCont("", "") };
                StoppsNewUpdateDG.ItemsSource = updatesNew = new ObservableCollection<StoppsCont>() { new StoppsCont("", "") };
            }
        }
        ObservableCollection<StoppsCont> inserts;
        ObservableCollection<StoppsCont> deletes;
        ObservableCollection<StoppsCont> updatesOld;
        ObservableCollection<StoppsCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = StoppsInsertDG.ItemsSource;
            string insert = "Insert into Stopps (Stop_Addres, Stop_Time) values ";
            int i = 0;
            foreach (StoppsCont d in data)
            {
                if (d.Stop_Addres != "")
                {
                    insert = i > 0 ? insert + ", " : insert;
                    insert += "( '" + d.Stop_Addres + "'";
                    if (d.Stop_Time != "")
                    {
                        insert += ", '" + d.Stop_Time + "')";
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
            inserts.Add(new StoppsCont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = StoppsDeleteDG.ItemsSource;
            string del = "Delete from Stopps where ";
            int i = 0;
            bool b = false;
            foreach (StoppsCont d in data)
            {
                if (d.Id != "")
                {
                    del = i > 0 ? del + " OR " : del;
                    del += " Id = " + d.Id + " ";
                    b = true;
                }
                if (d.Stop_Addres != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Stop_Addres = " + d.Stop_Addres;
                    b = true;
                }
                if (d.Stop_Addres != "")
                {
                    del = i > 0 && !b ? del + " OR " : del;
                    del += b ? "AND" : "";
                    del += " Stop_Time = " + d.Stop_Addres;
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
            deletes.Add(new StoppsCont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (StoppsSearchStopTime.Text != "" || StoppsSearchStopAdr.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select Id, Stop_Addres as Адрес, Stop_Time as Время_Остановки from Stopps Where";
                    bool b = false;
                    if (StoppsSearchStopTime.Text != "") {
                        sql = " Stop_Time = '" + StoppsSearchStopTime.Text + "'";
                        b = true;
                    }
                    if (StoppsSearchStopAdr.Text != "")
                    {
                        sql += b ? "," : "";
                        sql += " Stop_Addres = '" + StoppsSearchStopAdr.Text + "'";
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

                        StoppsDG.ItemsSource = ds.DefaultView;
                    }
                    catch { }
                }
            }
        }
        public void SetUserMode() {
            StoppsInsertItem.Visibility = Visibility.Collapsed;
            StoppsDeleteItem.Visibility = Visibility.Collapsed;
            StoppsUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new StoppsCont("", ""));
            updatesNew.Add(new StoppsCont("", ""));
        }
        private void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<StoppsCont>();
            var New = new List<StoppsCont>();
            foreach (StoppsCont old in StoppsOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (StoppsCont n in StoppsNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update Stopps set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Stop_Time == "" || Old[i].Stop_Addres == "") && (New[i].Stop_Time == "" || New[i].Stop_Addres == "" || New[i].Id == ""))
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
                        updates[i] = upd + "Stop_Time = " + New[i].Id + "";
                        b = true;
                    }
                    if (New[i].Stop_Addres != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " Stop_Addres = '" + New[i].Stop_Addres + "'";
                        b = true;
                    }
                    updates[i] += " Where ";
                    b = false;
                    if (Old[i].Id != "")
                    {
                        updates[i] = upd + "Stop_Time = " + Old[i].Id + "";
                        b = true;
                    }
                    if (Old[i].Id != "@")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] = upd + "Stop_Time = " + Old[i].Stop_Time + "' ";
                        b = true;
                    }
                    if (Old[i].Stop_Addres != "@")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Stop_Addres = '" + Old[i].Stop_Addres + "'";
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
