using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
using static WpfApp1.CompaniesPage;
using static WpfApp1.RoutesPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для DriversPages.xaml
    /// </summary>
    public class DriversCont : INotifyPropertyChanged
    {
        private string name;
        private string company;
        private string state;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
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

        public string State
        {
            get { return state; }
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        public DriversCont(string n, string c, string s)
        {
            Name = n;
            Company = c;
            State = s;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    public partial class DriversPage : Page
    {
        public DriversPage()
        {
            InitializeComponent();
            Init();
        }
        protected void Init()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                string sql = "Select Drivers.Id as Id, Drivers.name as Имя, Companies.Name as Компания, Company as Id__Компании, isnull(Drivers.state, '') as Примечание from Drivers inner join Companies on Companies.id = Drivers.Company";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                DrvDG.ItemsSource = ds.DefaultView;
            }

            DrvInsertDG.ItemsSource = inserts = inserts == null ? new ObservableCollection<DriversCont>() { new DriversCont("", "", ""), new DriversCont("", "", ""), new DriversCont("", "", ""), new DriversCont("", "", "") } : inserts;
            DrvDeleteDG.ItemsSource = deletes = deletes == null ? new ObservableCollection<DriversCont>() { new DriversCont("", "", ""), new DriversCont("", "", ""), new DriversCont("", "", ""), new DriversCont("", "", "") } : deletes;
            DriversOldUpdateDG.ItemsSource = updatesOld = updatesOld== null? new ObservableCollection<DriversCont>() { new DriversCont("", "", "") }: updatesOld;
            DriversNewUpdateDG.ItemsSource = updatesNew = updatesNew == null ? new ObservableCollection<DriversCont>() { new DriversCont("", "", "") } : updatesNew;
        }
        ObservableCollection<DriversCont> inserts;
        ObservableCollection<DriversCont> deletes;
        ObservableCollection<DriversCont> updatesOld;
        ObservableCollection<DriversCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = DrvInsertDG.ItemsSource;
            string insert = "Insert into Drivers (name, Company, state) values ";
            int i = 0;
            foreach (DriversCont d in data)
            {
                insert = i > 0 ? insert + ", " : insert;
                i++;
                if (d.Name == "")
                {
                    MessageBox.Show("значения не добавлены\nошибка в " + i + "-м столбце");
                    return;
                }
                else insert += "( '" + d.Name + "', " + d.Company + ", "+(d.State == "" ? d.State : "null") + ")";
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
            inserts.Add(new DriversCont("", "", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DrvInsertDG.SelectAll();
            var data = DrvInsertDG.SelectedItems;
            string del = "Delete from Drivers where ";
            int i = 0;
            bool b = false;
            foreach (DriversCont d in data)
            {
                if (d.Name == "" && d.Company == "")
                {
                    MessageBox.Show("Значения не добавлены\n\nОшибка в" + i + "-м столбце");
                    return;
                }
                else
                {
                    del = i > 0 ? del + " OR " : del;
                    if (d.Name != "")
                    {
                        b = true;
                        del += "Name = '" + d.Name + "' ";
                    }
                    if (d.State != "")
                    {
                        del += b ? "AND" : del;
                        b = true;
                        del += " state = " + "'" + d.State + "' ";
                    }
                    if (d.Company != "")
                    {
                        del += b ? "AND" : del;
                        b = true;
                        del += " AND Company = " + "" + d.Company + "";
                    }
                }
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
            deletes.Add(new DriversCont("", "", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(DrvSearchId.Text, out int x) && DrvSearchId.Text != "")
            {
                MessageBox.Show("Неправильное значение Id");
                return;
            }
            if (DrvSearchId.Text != "" || DrvSearchName.Text != "" || DrvSearchState.Text != "" || DrvSearchCompany.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select Id as Id, name as Название, isnull(state, '') as Примечание from Drivers where ";
                    if (DrvSearchId.Text != "")
                    {
                        sql += " Id = " + DrvSearchId.Text;
                    }
                    if (DrvSearchName.Text != "")
                    {
                        sql += " Name = '" + DrvSearchName.Text + "'";
                    }
                    if (DrvSearchState.Text != "")
                    {
                        sql += " state = '" + DrvSearchState.Text + "'";
                    }
                    if (DrvSearchCompany.Text != "")
                    {
                        sql += " Company = '" + DrvSearchCompany.Text + "'";
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    DrvDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        public void SetUserMode()
        {
            DriversInsertItem.Visibility = Visibility.Collapsed;
            DriversDeleteItem.Visibility = Visibility.Collapsed;
            DriversUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new DriversCont("", "", ""));
            updatesNew.Add(new DriversCont("", "", ""));
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<DriversCont>();
            var New = new List<DriversCont>();
            foreach (DriversCont old in DriversOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (DriversCont n in DriversNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update Routes set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Company == "" || Old[i].Name == "" || Old[i].State == "") && 
                    (New[i].Name == "" || New[i].Company == "" || New[i].State == ""))
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
                    if (New[i].Name != "")
                    {
                        updates[i] += upd + " Name = '" + New[i].Name + "' ";
                    }
                    if (New[i].Company != "")
                    {
                        updates[i] += b ? "," : "";
                        b = false;
                        updates[i] += " Company = '" + New[i].Company + "'";
                    }
                    if (New[i].State != "")
                    {
                        updates[i] += b ? "," : "";
                        b = false;
                        updates[i] += " state = '" + New[i].State + "'";
                    }
                    updates[i] += " Where ";
                    b = false;
                    if (Old[i].Name != "@")
                    {
                        b = true;
                        updates[i] = upd + "Name = " + New[i].Name + "' ";
                    }
                    if (Old[i].Company != "")
                    {
                        updates[i] += b ? "AND" : "";
                        b = false;
                        updates[i] += " Company = '" + New[i].Company + "' ";
                    }
                    if (Old[i].State != "")
                    {
                        updates[i] += b ? "AND" : "";
                        b = false;
                        updates[i] += " state = '" + New[i].State + "'";
                    }
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
