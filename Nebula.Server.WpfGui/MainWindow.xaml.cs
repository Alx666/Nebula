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
using System.IO;
using Nebula.Shared;

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
            m_hService.Start(28666);
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
            Dispatcher.BeginInvoke((Action)(() => 
            {
                m_hClientList.Items.Remove(obj);
                m_hStatusLabel.Text = $"{m_hClientList.Items.Count} Clients Connected";
            }));            
        }

        private void OnClientConnected(NebulaClient obj)
        {
            Dispatcher.BeginInvoke((Action)(() => 
            {
                m_hClientList.Items.Add(obj);
                m_hStatusLabel.Text = $"{m_hClientList.Items.Count} Clients Connected";
            }));
        }

        private void OnContextMenuAddModule(object sender, RoutedEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;
            
            Microsoft.Win32.OpenFileDialog hDlg = new Microsoft.Win32.OpenFileDialog();
            hDlg.FileName = "Nebula Module";
            hDlg.DefaultExt = ".dll";
            hDlg.Filter = "Assembly Files (.dll)|*.dll";
   
            if (hDlg.ShowDialog() == true)
            {
                byte[] hData = File.ReadAllBytes(hDlg.FileName);

                List<NebulaModuleInfo> hModules = hClient.Callback.AddModule(hData);
                MessageBox.Show(hModules.First().Name + " was installed");
            }
                        
        }

        private void OnContextMenuRemoveModule(object sender, RoutedEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;

            hClient.Callback.RemoveModule(Guid.NewGuid());
        }

        private void OnContextMenuListModules(object sender, RoutedEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;

            hClient.Callback.ListModules();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
