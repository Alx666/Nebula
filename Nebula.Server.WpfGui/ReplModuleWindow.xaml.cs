using Nebula.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nebula.Server.WpfGui
{
    public partial class ReplModuleWindow : Window
    {
        private NebulaClient    m_hClient;
        private StringBuilder   m_hSb;
        private Guid            m_vId;
        private bool            m_bWaitingForOutpout;

        private int             m_iReturnCount;
                                
        public ReplModuleWindow(NebulaClient hClient, Guid vModule)
        {
            m_hClient = hClient;
            m_vId = vModule;
            m_hSb = new StringBuilder();

            InitializeComponent();
        }

        public void Add(string sText)
        {            
            m_hTextBox.Text += sText + Environment.NewLine;
            m_hTextBox.CaretIndex = m_hTextBox.Text.Length - 1;
        }

        private void m_hTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChange hChange = e.Changes.First();

            if (hChange.AddedLength > 0 && hChange.RemovedLength > 0)
            {
                //Output entered Input
            }
            else if (hChange.AddedLength > 0 && hChange.RemovedLength == 0)
            {
                //User entered Input
                string sLastInput = m_hTextBox.Text.Substring(hChange.Offset, hChange.AddedLength);

                if (sLastInput == Environment.NewLine)
                {
                    m_hClient.Callback.Execute(m_vId, "SendCodeBlock", new string[] { m_hSb.ToString() });
                    m_hSb.Clear();
                }
                else
                {
                    m_hSb.Append(sLastInput);
                }
            }
            else if (hChange.AddedLength > 0)
            {                
                m_hSb.Remove(hChange.Offset, hChange.RemovedLength);
            }
        }
    }
}




