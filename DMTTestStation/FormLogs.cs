﻿using DMT.Core.Models;
using DMT.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TAI.TestDispatcher;

namespace DMTTestStation
{
    public partial class FormLogs : Form
    {
        private string Caption { get; set; }
        private bool Paused { get; set; }
        private List<string> CacheMessages { get; set; }
        public FormLogs(string caption)
        {
            InitializeComponent();
            this.Paused = false;
            this.CacheMessages = new List<string>();
            this.Caption = caption;

        }

        private void FormLogs_Load(object sender, EventArgs e)
        {
            this.richTextBoxLogs.Clear();
        }

        public void Update(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            SubjectObserver.FormInvoke update = new SubjectObserver.FormInvoke(this.ShowStatus);
            try
            {
                this.Invoke(update, notifyEvent, flag, content, result, message, sender);
            }
            catch (System.InvalidOperationException ex)
            {
                ex.ToString();
            }
            catch (System.ComponentModel.InvalidAsynchronousStateException)
            {

            }
        }




        public  void AppendTextWithColor(string content, Color color, bool IsaddNewLine)
        {
            if (IsaddNewLine)
            {
                content += Environment.NewLine;
            }
            this.richTextBoxLogs.SelectionStart = this.richTextBoxLogs.TextLength;
            this.richTextBoxLogs.SelectionLength = 0;
            this.richTextBoxLogs.SelectionColor = color;
            this.richTextBoxLogs.AppendText(content);
            this.richTextBoxLogs.SelectionColor = this.richTextBoxLogs.ForeColor;
        }


        public Color GetColorWithMessageType(MessageType messageType)
        {
            Color color = Color.Black;
            switch (messageType)
            {
                case MessageType.Normal:
                    {
                        color = Color.Black;
                        break;
                    }
                case MessageType.Error:
                    {
                        color = Color.Red;
                        break;
                    }
                case MessageType.Pass:
                    {
                        color = Color.Green;
                        break;
                    }
                case MessageType.Warning:
                    {
                        color = Color.Yellow;
                        break;
                    }

            }
            return color;

        }



        private void ShowStatus(int Event, string flag, string content, object result, string message, object sender)
        {
            try
            {
                NotifyEvents notifyEvent = (NotifyEvents)Event;
                switch (notifyEvent)
                {
                    case NotifyEvents.Message:
                    case NotifyEvents.Progress:
                        {
                            MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), content);

                            string info = "";
                            if (result is CardModule)
                            {
                               info = string.Format("{0}-{1}:{2}\r", DateTime.Now.ToString(), ((CardModule)result).Description , message);
                            
                            }
                            else  if ( result is BaseCalibrator)
                            { 
                                info = string.Format("{0}:{1}\r", DateTime.Now.ToString(), message);
                            }
                            if (!string.IsNullOrEmpty(info))
                            {
                                if (!this.Paused)
                                {
                                    Color color = GetColorWithMessageType(messageType);

                                    this.AppendTextWithColor(info, color, false);
                                }
                                else
                                {
                                    this.CacheMessages.Add(info);
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception)
            {

            }

        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            this.richTextBoxLogs.Clear();
        }

        private void toolStripButtonPause_Click(object sender, EventArgs e)
        {
            if (this.Paused)
            {
                foreach (string line in this.CacheMessages)
                {
                    this.richTextBoxLogs.AppendText(line);
                }
                this.CacheMessages.Clear();
                this.Paused = false;
                this.toolStripButtonPause.Text = "暂停";
            }
            else
            {
                this.Paused = true;
                this.toolStripButtonPause.Text = "恢复";
            }

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.saveFileDialog.FileName = string.Format("{0}-{1}.log",this.Caption,DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Files.WriteFile(this.saveFileDialog.FileName, this.richTextBoxLogs.Text);
            }
        }

        private void richTextBoxLogs_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            richTextBoxLogs.SelectionStart = richTextBoxLogs.Text.Length;                    //rtbReceive为控件的名字（自己取）
            richTextBoxLogs.ScrollToCaret();
        }
    }
}