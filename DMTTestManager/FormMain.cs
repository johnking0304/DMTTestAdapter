using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using DMT.Core.Channels;
using TAI.Constants;
using DMT.Core.Utils;
using DMTTestAdapter;

namespace DMTTestManager
{
    public partial class FormMain : MaterialForm
    {
        
        public UDPService UDPService { get; set; }
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            
            this.UDPService = new UDPService();
            this.UDPService.LoadFromFile(Contant.CONFIG);
            this.UDPService.Open();
            this.UDPService.AttachObserver(this.Update);

            Program.DMTTestAdapter = new TestAdapter();

            this.SelectTableControlPage(1);


        }


        public void SelectTableControlPage(int index)
        {
            /*            for (int i = 0; i < this.materialTabControlMain.TabCount; i++)
                        {
                            *//*                if (index == i)
                                            {
                                                materialTabControlMain.TabPages[i].Parent = this.materialTabControlMain;
                                            }
                                            else
                                            {
                                                materialTabControlMain.TabPages[i].Parent = null;
                                            }*//*


                        }*/
            tabPageAbout.Parent = null;
            tabPageDevices.Parent = null;
            tabPageSystem.Parent = null;
            tabPageLog.Parent = materialTabControlMain;

        }



        public void Update(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            SubjectObserver.FormInvoke update = new SubjectObserver.FormInvoke(this.ShowStatus);
            try
            {
                this.BeginInvoke(update, notifyEvent, flag, content, result, message, sender);
            }
            catch (System.InvalidOperationException)
            {
            }
            catch (System.ComponentModel.InvalidAsynchronousStateException)
            {

            }
        }


        private void AppLogText(string message)
        {
            this.richTextBoxLog.AppendText(string.Format("{0}",message));
        
        }

        private void ShowStatus(int Event, string flag, string content, object result, string message, object sender)
        {
            try
            {            
                switch (Event)
                {
                    case UDPClientChannel.UDP_DATA_EVENT:
                        {
                            this.AppLogText(message);
                            break;
                        }               
                }
            }
            catch
            {

            }

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.DMTTestAdapter.Dispose();
        }
    }
}
