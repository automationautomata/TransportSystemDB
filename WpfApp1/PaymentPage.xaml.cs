using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
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
using static WpfApp1.RoutesPage;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для PaymentPage.xaml
    /// </summary>
    public partial class PaymentPage : Page
    {
        public PaymentPage()
        {
            InitializeComponent();
            Init();
        }
        public class PaymentCont: INotifyPropertyChanged
        {
            private string cost;
            string type;
            public string Type
            {
                get { return type; }
                set
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
            public string Cost
            {
                get { return cost; }
                set
                {
                    cost = value;
                    OnPropertyChanged("Cost");
                }
            }
            public PaymentCont(string n, string s) {
                type = n;
                cost = s;
            }
            public void OnPropertyChanged([CallerMemberName] string prop = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
        protected void Init()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
            {
                string sql = "Select Type as Тип, Cost as Название from Payment";

                SqlCommand cmd = new SqlCommand();
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                DataTable ds = new DataTable();
                adapter.Fill(ds);
                PaymentDG.ItemsSource = ds.DefaultView;
            }
            if (inserts == null)
            {
                PaymentInsertDG.ItemsSource = inserts = new ObservableCollection<PaymentCont>() { new PaymentCont("", ""), new PaymentCont("", ""), new PaymentCont("", ""), new PaymentCont("", "") };
                PaymentDeleteDG.ItemsSource = deletes = new ObservableCollection<PaymentCont>() { new PaymentCont("", ""), new PaymentCont("", ""), new PaymentCont("", ""), new PaymentCont("", "") };
                PaymentInsertDG.ItemsSource = updatesNew = new ObservableCollection<PaymentCont>() { new PaymentCont("", "") };
                PaymentDeleteDG.ItemsSource = updatesOld = new ObservableCollection<PaymentCont>() { new PaymentCont("", "") };
            }
        }
        ObservableCollection<PaymentCont> inserts;
        ObservableCollection<PaymentCont> deletes;
        ObservableCollection<PaymentCont> updatesOld;
        ObservableCollection<PaymentCont> updatesNew;
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            //PaymentInsertDG.SelectAll();
            var data = PaymentInsertDG.ItemsSource;
            string insert = "Insert into Payment (Type, Cost) values ";
            int i = 0;
            foreach (PaymentCont d in data)
            {
                insert = i > 0 ? insert + ", " : insert;
                i++;
                if ((d.Type == "" || d.Cost == "" ) && !(d.Type == "" && d.Cost == ""))
                {
                    MessageBox.Show("значения не добавлены\nошибка в" + i + "-м столбце");
                    return;
                }
                else insert += "(" + d.Type + ", " + d.Cost + ")";
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
            inserts.Add(new PaymentCont("", ""));
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = PaymentDeleteDG.ItemsSource;
            string del = "Delete from Payment where ";
            int i = 0;
            bool b = false;
            foreach (PaymentCont d in data)
            {
                if (d.Type == "")
                {
                    MessageBox.Show("Значения не добавлены\n\nОшибка в " + i + "-м столбце");
                    return;
                }
                else
                {
                    del = i > 0 ? del + " OR " : del;
                    if (d.Type != "@")
                    {
                        del += "Name = '" + d.Type + "' ";
                        b = true;
                    }
                    if (d.Type != "")
                    {
                        del += b ? "AND" : "";
                        del += " state = " + "'" + d.Cost + "'";
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
            deletes.Add(new PaymentCont("", ""));
        }
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentSearchType.Text == "" && PaymentSearchCost.Text == "")
            {
                Init();
                return;
            }
            else if (!Int32.TryParse(PaymentSearchCost.Text, out int x) && PaymentSearchType.Text == "")
            {
                MessageBox.Show("Неправильное значение Цены");
                return;
            }
            else if (!Int32.TryParse(PaymentSearchType.Text, out int y) && PaymentSearchCost.Text == "")
            {
                MessageBox.Show("Неправильное значение Типа");
                return;
            }
            if (PaymentSearchType.Text != "" || PaymentSearchCost.Text != "")
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString.connectionString))
                {
                    string sql = "Select Type as Тип, Cost as Название from Payment where ";
                    bool b = false;
                    if (PaymentSearchType.Text != "") {
                        sql += " Type = " + PaymentSearchType.Text + " ";
                        b = true;
                    }
                    if (PaymentSearchCost.Text != "")
                    {
                        sql += b ? "AND" : "";
                        sql += " Cost = " + PaymentSearchCost.Text;
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();

                    cmd.Connection = connection;
                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);

                    DataTable ds = new DataTable();
                    adapter.Fill(ds);
                    PaymentDG.ItemsSource = ds.DefaultView;
                }
            }
        }
        public void SetUserMode()
        {
            PaymentInsertItem.Visibility = Visibility.Collapsed;
            PaymentDeleteItem.Visibility = Visibility.Collapsed;
            PaymentUpdateItem.Visibility = Visibility.Collapsed;
        }
        private void AddRowUpdate(object sender, RoutedEventArgs e)
        {
            updatesOld.Add(new PaymentCont("", ""));
            updatesNew.Add(new PaymentCont("", ""));
        }
        public void Update(object sender, RoutedEventArgs e)
        {
            PaymentOldUpdateDG.SelectAll();
            PaymentNewUpdateDG.SelectAll();
            var Old = new List<PaymentCont>();
            var New = new List<PaymentCont>();
            foreach (PaymentCont old in PaymentOldUpdateDG.SelectedItems) { Old.Add(old); }
            foreach (PaymentCont n in PaymentNewUpdateDG.SelectedItems) { New.Add(n); }
            string upd = "Update Routes set ";
            List<string> updates = new List<string>();
            for (int j = 0, i = Old.Count - 1; i >= 0; i--)
            {
                if ((Old[i].Type == "" || Old[i].Cost == "") && (New[i].Type == "" || New[i].Cost == ""))
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
                        updates[i] = upd + "Route_No = " + New[i].Type + "";
                    }
                    if (New[i].Cost != "")
                    {
                        updates[i] += "', Company = '" + New[i].Cost + "'";
                    }
                    updates[i] += " Where ";
                    if (Old[i].Type != "@")
                    {
                        updates[i] = upd + "Route_No = " + New[i].Type + "' ";
                    }
                    if (Old[i].Cost != "@")
                    {
                        updates[i] += "AND Company = '" + New[i].Cost + "'";
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
