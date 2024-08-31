using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        Dictionary<string, Page> pages = new Dictionary<string, Page>();
        public bool isAdmin = true;
        public MenuWindow()
        {
            InitializeComponent();
        }
        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            switch (((TreeViewItem)e.NewValue).Header.ToString()) {
                case "Компании":
                    if (!pages.ContainsKey("Companies"))
                        pages.Add("Companies", new CompaniesPage());
                    if (!isAdmin)
                        ((CompaniesPage)pages["Companies"]).SetUserMode();
                    FrameContent.Navigate(pages["Companies"]);
                    break;
                case "Водители":
                    if (!pages.ContainsKey("Drivers"))
                        pages.Add("Drivers", new DriversPage());
                    if (!isAdmin)
                        ((DriversPage)pages["Drivers"]).SetUserMode();
                    FrameContent.Navigate(pages["Drivers"]); 
                    break;
                case "Транспорт":
                    if (!pages.ContainsKey("Transport"))
                        pages.Add("Transport", new TransportPage());
                    if (!isAdmin)
                        ((TransportPage)pages["Transport"]).SetUserMode();
                    FrameContent.Navigate(pages["Transport"]);
                    break;
                case "Дополнительный персонал":
                    if (!pages.ContainsKey("Ad_stf"))
                        pages.Add("Ad_stf", new Additional_Staff_Page());
                    if (!isAdmin)
                        ((Additional_Staff_Page)pages["Ad_stf"]).SetUserMode();
                    FrameContent.Navigate(pages["Ad_stf"]);
                    break;
                case "Маршруты":
                    if (!pages.ContainsKey("Routes"))
                            pages.Add("Routes", new RoutesPage());
                    if (!isAdmin)
                        ((RoutesPage)pages["Routes"]).SetUserMode();
                    FrameContent.Navigate(pages["Routes"]); 
                    break;
                case "Остановки":
                    if (!pages.ContainsKey("Stopps"))
                        pages.Add("Stopps", new StoppsPage());
                    if (!isAdmin)
                        ((StoppsPage)pages["Stopps"]).SetUserMode();
                    FrameContent.Navigate(pages["Stopps"]);
                    break;
                case "Остановки в маршруте":
                    if (!pages.ContainsKey("RouteStop"))
                        pages.Add("RouteStop", new RouteStopPage());
                    if (!isAdmin)
                        ((RouteStopPage)pages["RouteStop"]).SetUserMode();
                    FrameContent.Navigate(pages["RouteStop"]); 
                    break;
                case "Виды оплаты":
                    if (!pages.ContainsKey("Payment"))
                        pages.Add("Payment", new PaymentPage());
                    if (!isAdmin)
                        ((PaymentPage)pages["Payment"]).SetUserMode();
                    FrameContent.Navigate(pages["Payment"]);
                    break;
                case "Виды оплаты в маршрутах":
                    if (!pages.ContainsKey("RouteCost"))
                        pages.Add("RouteCost", new RouteCostPage());
                    if (!isAdmin)
                        ((RouteCostPage)pages["RouteCost"]).SetUserMode();
                    FrameContent.Navigate(pages["RouteCost"]);
                    break;
                case "Расписание":
                    if (!pages.ContainsKey("Work_Scheldue"))
                        pages.Add("Work_Scheldue", new Work_SchelduePage());
                    if (!isAdmin)
                        ((Work_SchelduePage)pages["Work_Scheldue"]).SetUserMode();
                    FrameContent.Navigate(pages["Work_Scheldue"]);
                    break;
                case "Журнал ремонта":
                    if (!pages.ContainsKey("Repair"))
                        pages.Add("Repair",new RepairPage());
                    if (!isAdmin)
                        ((RepairPage)pages["Repair"]).SetUserMode();
                    FrameContent.Navigate(pages["Repair"]);
                    break;
            }
        }
    }
}
