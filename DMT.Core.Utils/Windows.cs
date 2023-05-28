using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMT.Core.Utils
{
    public static class Windows
    {
        public static void AddControlToListView(ListView listview, Control control, int columnIndex, int rowIndex, float columnPercent = 1, float rowPercent = 1)
        {
            listview.Controls.Add(control);

            UpdateControlToListView(listview, control, columnIndex, rowIndex, columnPercent, rowPercent);
/*            int width = (int)(listview.Items[rowIndex].SubItems[columnIndex].Bounds.Width * columnPercent);
            int height = (int)(listview.Items[rowIndex].SubItems[columnIndex].Bounds.Height * rowPercent);
            control.Size = new Size(width, height);

            int left = listview.Items[rowIndex].SubItems[columnIndex].Bounds.Left + (int)((listview.Items[rowIndex].SubItems[columnIndex].Bounds.Width - width) / 2);
            int top = listview.Items[rowIndex].SubItems[columnIndex].Bounds.Top + (int)((listview.Items[rowIndex].SubItems[columnIndex].Bounds.Height - height) / 2);
            control.Location = new Point(left, top);*/
        }

        public static void UpdateControlToListView(ListView listview, Control control, int columnIndex, int rowIndex, float columnPercent = 1, float rowPercent = 1)
        {
            int width = (int)(listview.Items[rowIndex].SubItems[columnIndex].Bounds.Width * columnPercent);
            int height = (int)(listview.Items[rowIndex].SubItems[columnIndex].Bounds.Height * rowPercent);
            control.Size = new Size(width, height);

            int left = listview.Items[rowIndex].SubItems[columnIndex].Bounds.Left + (int)((listview.Items[rowIndex].SubItems[columnIndex].Bounds.Width - width) / 2);
            int top = listview.Items[rowIndex].SubItems[columnIndex].Bounds.Top + (int)((listview.Items[rowIndex].SubItems[columnIndex].Bounds.Height - height) / 2);
            control.Location = new Point(left, top);
        }





        public static void AddFormToContainer(Form form, Control container)
        {
            if (form != null && container != null)
            {
                form.FormBorderStyle = FormBorderStyle.None;
                form.TopLevel = false;
                form.Dock = System.Windows.Forms.DockStyle.Fill;
                container.Controls.Add(form);
                form.Show();
            }
        }
        public static void MessageBoxError(string message)
        {
            if (message.Length > 0)
            {
                MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void MessageBoxWarning(string message)
        {
            if (message.Length > 0)
            {
                MessageBox.Show(message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void DeleteDirectoryFile(string directory)
        {
            FileAttributes attr = File.GetAttributes(directory);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(directory, true);
            }
        }

        public static void MessageBoxInformation(string message)
        {
            if (message.Length > 0)
            {
                MessageBox.Show(message, "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static Boolean MessageBoxQuestion(string message)
        {
            if (message.Length > 0)
            {
                return MessageBox.Show(message, "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
            }
            return false;
        }
    }
}
