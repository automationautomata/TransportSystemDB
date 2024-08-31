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
using static WpfApp1.CompaniesPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для CompanyPage.xaml
    /// </summary>
    public partial class Additional_Staff_Page : Page
    {
        public Additional_Staff_Page()
        {
            InitializeComponent();
            Init();
        }
        public class Additional_Staff_Cont : INotifyPropertyChanged
        {
            private string name;
            private string id;

            public string Name
            {
                get { return name; }
                set
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }

            public string Id
            {
                get { return id; }
                set
                {
                    id = value;
                    OnPropertyChanged("State");
                }
            }

            public Additional_Staff_Cont(string n, string s)
            {
                Name = n;
                Id = s;
            }
            public Additional_Staff_Cont() { }
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
                string sql = "Select Id as Id, name as Имя from Additional_Staff";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                AdsDG.ItemsSource = ds.DefaultView;
            }
            if (inserts == null)
            {
                AdsInsertDG.ItemsSource = inserts = new ObservableCollection<Additional_Staff_Cont>() { new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", "") };
                AdsDeleteDG.ItemsSource = deletes = new ObservableCollection<Additional_Staff_Cont>() { new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", "") };
                AdsOldUpdateDG.ItemsSource = updatesOld = new ObservableCollection<Additional_Staff_Cont>() { new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", "") };
                AdsNewUpdateDG.ItemsSource = updatesNew = new ObservableCollection<Additional_Staff_Cont>() { new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", ""), new Additional_Staff_Cont("", "") };
            }
        }
        ObservableCollection<Additional_Staff_Cont> inserts;
        ObservableCollection<Additional_Staff_Cont> deletes;
        ObservableCollection<Additional_Staff_Cont> updatesOld;
        ObservableCollection<Additional_Staff_Cont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            AdsInsertDG.SelectAll();
            var data = AdsInsertDG.SelectedItems;
            string insert = "Insert into Additional_Staff (name) values ";
            int i = 0;
            foreach (Additional_Staff_Cont d in data)
            {
                insert = i > 0 ? insert + ", " : insert;
                i++;
                if (d.Name == "")
                {
                    MessageBox.Show("Значения не добавлены\n\nОшибка в " + i + "-м столбце");
                    return;
                }
                else insert += "('" + d.Name+ "')";
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
            inserts.Add(new Additional_Staff_Cont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            AdsInsertDG.SelectAll();
            var data = AdsInsertDG.SelectedItems;
            string del = "Delete from Additional_Staff where ";
            int i = 0;
            bool b = false;
            foreach (Additional_Staff_Cont d in data)
            {
                if (d.Name == "")
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
                        del += "Name = '" + d.Name + "'";
                    }
                    if (d.Name != "")
                    {
                        del += b ? "AND" : "";
                        del += " Id = " + d.Id + "";
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
            deletes.Add(new Additional_Staff_Cont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if(!Int32.TryParse(AdsSearchId.Text, out int x) && AdsSearchId.Text != "")
            {
                MessageBox.Show("Неправильное значение Id");
                return;
            }
            if (AdsSearchId.Text != "" || CmpSearchName.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select Id, name as Имя from Additional_Staff where ";
                    bool b = false;
                    if (CmpSearchName.Text != "")
                    {
                        b = true;
                        sql += " Name = '" + CmpSearchName.Text + "'";
                    }
                    if (AdsSearchId.Text != "")
                    {
                        sql += b ? "," : "";
                        sql += " id = " + AdsSearchId.Text + "";
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    AdsDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new Additional_Staff_Cont("", ""));
            updatesNew.Add(new Additional_Staff_Cont("", ""));
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<Additional_Staff_Cont>();
            var New = new List<Additional_Staff_Cont>();
            foreach (Additional_Staff_Cont old in AdsOldUpdateDG.SelectedItems) { Old.Add(old); }
            foreach (Additional_Staff_Cont n in AdsNewUpdateDG.SelectedItems) { New.Add(n); }
            string upd = "Update Additional_Staff set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Name == "") && (New[i].Name == ""))
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
                    updates[i] += " Where ";
                    b = false;
                    if (Old[i].Name != "")
                    {
                        b = true;
                        updates[i] = upd + "Name = '" + New[i].Name + "' ";
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
            AdsInsertItem.Visibility = Visibility.Collapsed;
            AdsDeleteItem.Visibility = Visibility.Collapsed;
            AdsUpdateItem.Visibility = Visibility.Collapsed;
        }
    }
}
