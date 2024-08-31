using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Data.Entity.Infrastructure.Design.Executor;
using static WpfApp1.PaymentPage;
using static WpfApp1.RepairPage;
using static WpfApp1.RoutesPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RepairPage.xaml
    /// </summary>
    public partial class RepairPage: Page
    {
        public RepairPage()
        {
            InitializeComponent();
            Init();
            SearchCompany.Visibility = Visibility.Collapsed;
        }
        public class RepairCont : INotifyPropertyChanged
        {
            private string transport;
            private string request_Date;
            private string state;
            private string eng;
            private string end_Time;

            private string type;
            public string Type
            {
                get { return type; }
                set
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
            public string Transport
            {
                get { return transport; }
                set
                {
                    transport = value;
                    OnPropertyChanged("Transport");
                }
            }
            public string Request_Date
            {
                get { return request_Date; }
                set
                {
                    request_Date = value;
                    OnPropertyChanged("Request_Date");
                }
            }
            public string State
            {
                get { return state; }
                set
                {
                    state = value;
                    OnPropertyChanged("State");
                }
            }
            public string Eng
            {
                get { return eng; }
                set
                {
                    eng = value;
                    OnPropertyChanged("Eng");
                }
            }
            public string End_Time
            {
                get { return end_Time; }
                set
                {
                    end_Time = value;
                    OnPropertyChanged("End_Time");
                }
            }
            public RepairCont()
            {
                Type = "";
                Transport = "";
                Request_Date = "";
                State = "";
                Eng = "";
                End_Time = "";
            }
            public RepairCont(string t, string tr, string rd, string s, string e, string et)
            {
                Type = t;
                Transport = tr;
                Request_Date = rd;
                State = s;
                Eng = e;
                End_Time = et;
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
                //string sql = "Select Repair.Id, Route_No as Маршрут, Transport as Транспорт," +
                //            " Drivers.Name as Водитель, Addictional_Staff.Name as Кондуктор, Start_Time as Время начала работы " +
                //            "from Repair inner join Drivers on Drivers.Id = Repair.Driver inner join " +
                //            "Addictional_Staff on Repair.Addictional_Staff = Addictional_Staff.Id";
                string sql = "SELECT TOP (1000) Id, Type AS Тип, Transport AS Транспорт, Request_Date AS Дата_Запроса, Status AS Состояние, Engineer AS Инженер, ISNULL(End_Date, '') AS Дата_Окончания\r\nFROM" +
                                "            dbo.Repair\r\n" +
                                "WHERE        (Request_Date > DATEADD(year, - 10, GETDATE()))";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                RepDG.ItemsSource = ds.DefaultView;
            }
            RepInsertDG.ItemsSource = inserts = new ObservableCollection<RepairCont>() { new RepairCont(), new RepairCont(), new RepairCont(), new RepairCont() };
            RepDeleteDG.ItemsSource = deletes = new ObservableCollection<RepairCont>() { new RepairCont(), new RepairCont(), new RepairCont(), new RepairCont() };
            RepairOldUpdateDG.ItemsSource = updatesNew = new ObservableCollection<RepairCont>() { new RepairCont(), new RepairCont(), new RepairCont(), new RepairCont() };
            RepairNewUpdateDG.ItemsSource = updatesOld = new ObservableCollection<RepairCont>() { new RepairCont(), new RepairCont(), new RepairCont(), new RepairCont() };
        }
        ObservableCollection<RepairCont> inserts;
        ObservableCollection<RepairCont> deletes;
        ObservableCollection<RepairCont> updatesNew;
        ObservableCollection<RepairCont> updatesOld;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = RepInsertDG.ItemsSource;
            string insert = "INSERT INTO dbo.Repair ( Type, Transport, Request_Date, Status, Engineer, End_Date) VALUES";
            int i = 0;
            string tmp;
            foreach (RepairCont d in data)
            {
                tmp = insert;
                i++;
                  if ( d.Transport == " " || d.Request_Date == " " || d.State == " " || d.Eng == " ")
                {
                    MessageBox.Show("Значения не добавлены\n\nОшибка в " + i + "-м столбце");
                    return;
                }
                else tmp += " ('" + d.Type + "', '" + d.Transport + "', '" + d.Request_Date + "', " + d.State + ", " + d.Eng + ", " + (d.End_Time!=" " || d.End_Time != " " ? "'"+d.End_Time+"'":"Null" ) + ") ";
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(tmp, connection);
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
            inserts.Add(new RepairCont("", "", "", "", "", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = RepDeleteDG.ItemsSource;
            string del = "Delete from Repair where ";
            int i = 0;
            bool b = false;
            string sql;
            foreach (RepairCont d in data)
            {
                i++;
                sql = del;
                if (d.Type != "")
                {
                    sql += " Type = '" + d.Type + "' ";
                    b = true;
                }
                if (d.Transport != "")
                {
                    sql += b ? "And" : "";
                    sql += " Transport = '" + d.Transport + "' ";
                    b = true;
                }
                if (d.Request_Date != "")
                {
                    sql += b ? "And" : "";
                    sql += " Request_Date = '" + d.Request_Date.Replace("/", "") + "' ";
                    b = true;
                }
                if (d.State != "")
                {
                    sql += b ? "And" : "";
                    sql += " Status = " + d.State + " ";
                    b = true;
                }
                if (d.Eng != "")
                {
                    sql += b ? "And" : "";
                    sql += " Engineer = " + d.Eng + " ";
                    b = true;
                }
                if (d.End_Time != "")
                {
                    sql += b ? "And" : "";
                    sql += " End_Date = '" + d.End_Time + "'";
                    b = true;
                }
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        Init();
                        MessageBox.Show("Значения добавлены");
                    }
                    catch { MessageBox.Show("Значения не добавлены"); }

                }
                b = false;
            }
        }
        private void AddRowDelete(object sender, RoutedEventArgs e)
        {
            deletes.Add(new RepairCont("", "", "", "", "", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            bool b = false;
            if (RepSearchType.Text != "" || RepSearchTransport.Text != "" || RepSearchRequest_Date.Text != "" ||
                RepSearchStatus.Text != "" || RepSearchEng.Text != "" || RepSearchED.Text != "" )
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select * from Repair Where ";
                    if (RepSearchType.Text != "")
                    {
                        sql += "Type Like '%" + RepSearchType.Text + "%' ";
                        b = true;
                    }
                    if (RepSearchTransport.Text != "")
                    {
                        sql += b ? "And" : "";
                        sql += " Transport = '" + RepSearchTransport.Text + "' ";
                        b = true;
                    }
                    if (RepSearchRequest_Date.Text != "")
                    {
                        sql += b ? "And" : "";
                        sql += " Request_Date = '" + RepSearchRequest_Date.Text + "' ";
                        b = true;
                    }
                    if (RepSearchStatus.Text != "")
                    {
                        sql += b ? "And" : "";
                        sql += " Status Like '%" + RepSearchStatus.Text + "%' ";
                        b = true;
                    }
                    if (RepSearchEng.Text != "")
                    {
                        sql += b ? "And" : "";
                        sql += " Engineer = " + RepSearchEng.Text + " ";
                        b = true;
                    }
                    if (RepSearchED.Text != "")
                    {
                        sql += b ? "And" : "";
                        sql += " End_Date = '" + RepSearchED.Text + "'";
                        b = true;
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    RepDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<RepairCont>();
            var New = new List<RepairCont>();
            foreach (RepairCont old in RepairNewUpdateDG.SelectedItems) { Old.Add(old); }
            foreach (RepairCont n in RepairOldUpdateDG.SelectedItems) { New.Add(n); }
            string upd = "Update Repair set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--, b = false)
            {
                if ((New[i].Type == "" && New[i].Transport == "" && New[i].Request_Date == "" && New[i].State == "" && New[i].Eng == "" && New[i].End_Time == "") &&
                    (Old[i].Type == "" && Old[i].Transport == "" && Old[i].Request_Date == "" && Old[i].State == "" && Old[i].Eng == "" && Old[i].End_Time == ""))
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
                    if (New[i].Type != "")
                    {
                        updates[i] += "Type = '" +  New[i].Type + "' ";
                        b = true;
                    }
                    if (New[i].Transport != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " Transport = '" + New[i].Transport + "' ";
                        b = true;
                    }
                    if (New[i].Request_Date != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " Request_Date = '" + New[i].Request_Date + "' ";
                        b = true;
                    }
                    if (New[i].State != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " Status = '" + New[i].State + "' ";
                        b = true;
                    }
                    if (New[i].Eng != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " Engineer = " + New[i].Eng + " ";
                        b = true;
                    }
                    if (New[i].End_Time != "")
                    {
                        updates[i] += b ? "," : "";
                        updates[i] += " End_Date = '" + RepSearchED.Text + "'";
                        b = true;
                    }

                    b = false;
                    updates[i] += " Where ";
                    if (Old[i].Type != "")
                    {
                        updates[i] += "Type = '" + Old[i].Type + "' ";
                        b = true;
                    }
                    if (Old[i].Transport != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Transport = '" + Old[i].Transport + "' ";
                        b = true;
                    }
                    if (Old[i].Request_Date != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Request_Date = '" + Old[i].Request_Date + "' ";
                        b = true;
                    }
                    if (Old[i].State != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Status = " + Old[i].State + " ";
                        b = true;
                    }
                    if (Old[i].Eng != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " Engineer = " + Old[i].Eng + " ";
                        b = true;
                    }
                    if (Old[i].End_Time != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " End_Date = '" + Old[i].End_Time + "'";
                        b = true;
                    }
                }
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                connection.Open();
                foreach (string str in updates)
                {
                    SqlCommand cmd = new SqlCommand(upd+str, connection);
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
        private void Report(object sender, RoutedEventArgs e)
        {
            var Rp = new ReportWindow();
            Rp.Show();
            using (SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=TransportSystemDB;Integrated Security=True"))
            {
                var d1 = Date1.SelectedDate.ToString().Replace(".", "-").Replace("0:00:00", "");//DATEADD(YEAR, -10, getdate())
                var d2 = Date2.SelectedDate.ToString().Replace(".", "-").Replace("0:00:00", "");//DATEADD(YEAR, -10, getdate())

                string select = "DECLARE @d1 AS DATE = DATEADD(YEAR, -10, '" + d1 + "'), @d2 AS DATE = '" + d2 + "', @n AS INT = 0\r\n" +
                                "EXEC RepairStatistics @Begin = @d1, @End = @d2\n";
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
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SearchCompany.Visibility = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                //string sql = "Select Repair.Id, Route_No as Маршрут, Transport as Транспорт," +
                //            " Drivers.Name as Водитель, Addictional_Staff.Name as Кондуктор, Start_Time as Время начала работы " +
                //            "from Repair inner join Drivers on Drivers.Id = Repair.Driver inner join " +
                //            "Addictional_Staff on Repair.Addictional_Staff = Addictional_Staff.Id";
                string sql = "SELECT TOP (1000) Id, Type AS Тип, Transport AS Транспорт, Request_Date AS Дата_Запроса, Status AS Состояние, " +
                                "Engineer AS Инженер, ISNULL(End_Date, '') AS Дата_Окончания, Cmp.Company as Компанияы " +
                                    "FROM Repair inner join " +
                                        "(SELECT dbo.Transport.Number as Number, dbo.Companies.Name as Company " +
                                            "FROM dbo.Companies INNER JOIN dbo.Transport ON dbo.Companies.Id = dbo.Transport.Company) as Cmp " +
                                                "ON Cmp.Number = dbo.Repair.Transport;";
                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                RepDG.ItemsSource = ds.DefaultView;
            }
        }
        public void SetUserMode()
        {
            RepairInsertItem.Visibility = Visibility.Collapsed;
            RepairDeleteItem.Visibility = Visibility.Collapsed;
            RepairUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new RepairCont());
            updatesNew.Add(new RepairCont());
        }
    }
}
