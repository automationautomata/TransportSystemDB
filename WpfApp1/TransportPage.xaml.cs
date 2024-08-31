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
using static WpfApp1.TransportPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для TransportPage.xaml
    /// </summary>
    public partial class TransportPage : Page
    {
        public TransportPage()
        {
            InitializeComponent();
            Init();
        }
        public class TransportCont : INotifyPropertyChanged
        {
            private string number;
            private string company;
            private string state;

            public string Number
            {
                get { return number; }
                set
                {
                    number = value;
                    OnPropertyChanged("Number");
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

            public TransportCont(string n, string s)
            {
                Number = n;
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
                string sql = "Select Number as Номер, Company as Id__Компании, (select name from Companies where id = Company) as Компания, isnull(state, '') as Примечание from Transport";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                TransportDG.ItemsSource = ds.DefaultView;
            }
            if (inserts == null)
            {
                TransportInsertDG.ItemsSource = inserts = new ObservableCollection<TransportCont>() { new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", "") };
                TransportDeleteDG.ItemsSource = deletes = new ObservableCollection<TransportCont>() { new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", "") };
                TransportInsertDG.ItemsSource = updatesOld = new ObservableCollection<TransportCont>() { new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", "") };
                TransportDeleteDG.ItemsSource = updatesNew = new ObservableCollection<TransportCont>() { new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", ""), new TransportCont("", "") };
            }
        }
        ObservableCollection<TransportCont> inserts;
        ObservableCollection<TransportCont> deletes;
        ObservableCollection<TransportCont> updatesOld;
        ObservableCollection<TransportCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            var data = TransportInsertDG.ItemsSource;
            string insert = "Insert into Transport (Number, Company, state) values ";
            int i = 0;
            foreach (TransportCont d in data)
            {
                insert = i > 0 ? insert + ", " : insert;
                i++;
                if (d.Number == "")
                {
                    MessageBox.Show("Значения не добавлены\nОшибка в " + i + "-м столбце");
                    return;
                }
                else insert += "('" + d.Number+ "', " + d.Company + ", " + (d.State == "" ? d.State : "null") + ")";
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
            inserts.Add(new TransportCont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            TransportInsertDG.SelectAll();
            var data = TransportInsertDG.SelectedItems;
            string del = "Delete from Transport where ";
            int i = 0;
            bool b = false;
            foreach (TransportCont d in data)
            {
                if (d.Number == "")
                {
                    MessageBox.Show("Значения не добавлены\n\nОшибка в" + i + "-м столбце");
                    return;
                }
                else
                {
                    del = i > 0 ? del + " OR " : del;
                    if (d.Number != "@")
                    {
                        b = true;
                        del += "Number = '" + d.Number + "'";
                    }
                    if (d.Company != "")
                    {
                        del += b ? "AND" : "";
                        del += " Company = " + "'" + d.Company + "'";
                        b = true;
                    }
                    if (d.State != "")
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
            deletes.Add(new TransportCont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            //if(!Int32.TryParse(TransportSearchId.Text, out int x) && TransportSearchId.Text != "")
            //{
            //    MessageBox.Show("Неправильное значение Id");
            //    return;
            //}
            if (TransportSearchCompany.Text != "" || TransportSearchNumber.Text != "" || TransportSearchState.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string s = "Select Number as Название, Company as Id__Компании, isnull(state, '') as Примечание from Transport ";
                    string sql = "";
                    bool b = false;
                    if (TransportSearchCompany.Text != "") {
                        b = true;
                        sql += " Company = " + TransportSearchCompany.Text + " ";
                    }
                    if (TransportSearchNumber.Text != "")
                    {
                        sql += b ? "AND" : "";
                        b = true;
                        sql += " Number = '" + TransportSearchNumber.Text + "' ";
                    }
                    if (TransportSearchState.Text != "")
                    {
                        sql += b ? "AND" : "";
                        b = true;
                        sql += " state = '" + TransportSearchState.Text + "'";
                    }
                    if (sql != "")
                    {
                        sql = s + " where " + sql;
                    }
                    else sql = s;
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    TransportDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new TransportCont("", ""));
            updatesNew.Add(new TransportCont("", ""));
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            var Old = new List<TransportCont>();
            var New = new List<TransportCont>();
            foreach (TransportCont old in TransportOldUpdateDG.ItemsSource) { Old.Add(old); }
            foreach (TransportCont n in TransportNewUpdateDG.ItemsSource) { New.Add(n); }
            string upd = "Update Transport set ";
            List<string> updates = new List<string>();
            bool b = false;
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Number == "" || Old[i].State == "") && (New[i].Number == "" || New[i].State == ""))
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
                    if (New[i].Number != "")
                    {
                        b = true;
                        updates[i] = upd + "Number = '" + New[i].Number + "'";
                    }
                    if (New[i].State != "")
                    {
                        updates[i] += b ? "," : "";
                        b = true;
                        updates[i] += " Company = " + New[i].Company + " ";
                    }
                    if (New[i].State != "")
                    {
                        updates[i] += b ? "," : "";
                        b = true;
                        updates[i] += " state = '" + New[i].State + "'";
                    }
                    updates[i] += " Where ";
                    b = false;
                    if (Old[i].Number != "")
                    {
                        b = true;
                        updates[i] = upd + "Number = '" + Old[i].Number + "' ";
                    }
                    if (Old[i].Number != "")
                    {
                        updates[i] += b ? "AND" : "";
                        b = true;
                        updates[i] = upd + "Company = " + Old[i].Number + " ";
                    }
                    if (Old[i].State != "")
                    {
                        updates[i] += b ? "AND" : "";
                        updates[i] += " state = '" + Old[i].State + "'";
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
            TransportInsertItem.Visibility = Visibility.Collapsed;
            TransportDeleteItem.Visibility = Visibility.Collapsed;
            TransportUpdateItem.Visibility = Visibility.Collapsed;
        }
    }
}
