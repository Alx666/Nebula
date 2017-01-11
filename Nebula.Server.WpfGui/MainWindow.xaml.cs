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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nebula.Server;

namespace Nebula.Server.WpfGui
{    
    public partial class MainWindow : Window
    {
        private NebulaMasterService m_hService;

        public MainWindow()
        {
            InitializeComponent();

            MenuItemStartClick(null, null);
        }

        private void MenuItemStartClick(object sender, RoutedEventArgs e)
        {
            m_hService                  = new NebulaMasterService();
            m_hService.ClientConnected += OnClientConnected;
            m_hService.ClientFaulted   += OnClientDisconnected;
            m_hService.Start(28000);
            m_hMenuItemStart.IsEnabled  = false;
            m_hMenuItemStop.IsEnabled   = true;
        }

        private void MenuItemStopClick(object sender, RoutedEventArgs e)
        {
            m_hService.ClientConnected -= OnClientConnected;
            m_hService.ClientFaulted   -= OnClientDisconnected;
            m_hService.Stop();
            m_hService.Dispose();
            m_hService                  = null;
            m_hMenuItemStart.IsEnabled  = true;
            m_hMenuItemStop.IsEnabled   = false;
        }

        private void OnClientDisconnected(NebulaClient obj)
        {            
            Dispatcher.BeginInvoke((Action)(() => m_hClientList.Items.Remove(obj)));
        }

        private void OnClientConnected(NebulaClient obj)
        {
            Dispatcher.BeginInvoke((Action)(() => m_hClientList.Items.Add(obj)));
        }
    }
}
