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
using static WpfApp1.PaymentPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для CompanyPage.xaml
    /// </summary>
    public struct ConnectionString
    {
       public const String connectionString = "Data Source=localhost;Initial Catalog=TransportSystemDB;Integrated Security=True";
    }
    public partial class CompaniesPage : Page
    {
        public CompaniesPage()
        {
            InitializeComponent();
            Init();
        }
        public class CompaniesCont : INotifyPropertyChanged
        {
            private string name;
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

            public string State
            {
                get { return state; }
                set
                {
                    state = value;
                    OnPropertyChanged("State");
                }
            }

            public CompaniesCont(string n, string s)
            {
                Name = n;
                State = s;
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
                string sql = "Select Id as Id, name as Название, isnull(state, '') as Примечание from Companies";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                CmpDG.ItemsSource = ds.DefaultView;
            }
            CmpInsertDG.ItemsSource = inserts = new ObservableCollection<CompaniesCont>() { new CompaniesCont("",""), new CompaniesCont("", "") , new CompaniesCont("", "") , new CompaniesCont("", "") };
            CmpDeleteDG.ItemsSource = deletes = new ObservableCollection<CompaniesCont>() { new CompaniesCont("", ""), new CompaniesCont("", ""), new CompaniesCont("", ""), new CompaniesCont("", "") };
            CmpInsertDG.ItemsSource = updatesOld = new ObservableCollection<CompaniesCont>() { new CompaniesCont("", ""), new CompaniesCont("", ""), new CompaniesCont("", ""), new CompaniesCont("", "") };
            CmpDeleteDG.ItemsSource = updatesNew = new ObservableCollection<CompaniesCont>() { new CompaniesCont("", ""), new CompaniesCont("", ""), new CompaniesCont("", ""), new CompaniesCont("", "") };
        }
        ObservableCollection<CompaniesCont> inserts;
        ObservableCollection<CompaniesCont> deletes;
        ObservableCollection<CompaniesCont> updatesOld;
        ObservableCollection<CompaniesCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = CmpInsertDG.ItemsSource;
            string insert = "Insert into Сompanies (name, state) values ";
            int i = 0;
            foreach (CompaniesCont d in data)
            {
                insert = i > 0 ? insert + ", " : insert;
                i++;
                if (d.Name == "")
                {
                    MessageBox.Show("значения не добавлены\nошибка в" + i + "-м столбце");
                    return;
                }
                else insert += "('" + d.Name+ "', " + (d.State == "" ? d.State : "null") + ")";
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
            inserts.Add(new CompaniesCont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            CmpInsertDG.SelectAll();
            var data = CmpInsertDG.SelectedItems;
            string del = "Delete from Сompanies where ";
            int i = 0;
            bool b = false;
            foreach (CompaniesCont d in data)
            {
                if (d.Name == "")
                {
                    MessageBox.Show("Значения не добавлены\n\nОшибка в" + i + "-м столбце");
                    return;
                }
                else
                {
                    del = i > 0 ? del + " OR " : del;
                    if (d.Name != "@")
                    {
                        b = true;
                        del += "Name = '" + d.Name + "'";
                    }
                    if (d.Name != "")
                    {
                        del += b ? "AND" : "";
                        del += " state = " + "'" + d.State + "'";
                    }
                    b = false;
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
            deletes.Add(new CompaniesCont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if(!Int32.TryParse(CmpSearchId.Text, out int x) && CmpSearchId.Text != "")
            {
                MessageBox.Show("Неправильное значение Id");
                return;
            }
            if (CmpSearchId.Text != "" || CmpSearchName.Text != "" || CmpSearchState.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select Id as Id, name as Название, isnull(state, '') as Примечание from Companies where ";
                    if (CmpSearchId.Text != "") {
                        sql += " Id = " + CmpSearchId.Text;
                    }
                    if (CmpSearchName.Text != "")
                    {
                        sql += " Name = '" + CmpSearchName.Text + "'";
                    }
                    if (CmpSearchState.Text != "")
                    {
                        sql += " state = '" + CmpSearchState.Text + "'";
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    CmpDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new CompaniesCont("", ""));
            updatesNew.Add(new CompaniesCont("", ""));
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<CompaniesCont>();
            var New = new List<CompaniesCont>();
            foreach (CompaniesCont old in CompaniesOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (CompaniesCont n in CompaniesNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update Routes set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Name == "" || Old[i].State == "") && (New[i].Name == "" || New[i].State == ""))
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
                        b = true;
                        updates[i] = upd + "Name = '" + New[i].Name + "'";
                    }
                    if (New[i].State != "")
                    {
                        updates[i] += b ? "," : "";
                        b = true;
                        updates[i] += " state = '" + New[i].State + "'";
                    }
                    updates[i] += " Where ";
                    b = false;
                    if (Old[i].Name != "@")
                    {
                        b = true;
                        updates[i] = upd + "Name = '" + New[i].Name + "' ";
                    }
                    if (Old[i].State != "@")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " state = '" + New[i].State + "'";
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

        public void SetUserMode()
        {
            CompaniesInsertItem.Visibility = Visibility.Collapsed;
            CompaniesDeleteItem.Visibility = Visibility.Collapsed;
            CompaniesUpdateItem.Visibility = Visibility.Collapsed;
        }
    }
}
