using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static JSFW.VS.Extensibility.Cmds.Controls.MethodList;

namespace JSFW.VS.Extensibility.Cmds.Controls
{
    public partial class MethodHintEdit : UserControl
    {
        public bool IsOK { get; private set; }
        public string MethodFullName { get { return lbMethodName.Text.Trim(); } }
        public string Comment { get { return txtHint.Text.Trim(); } }

        public event EventHandler btnOK_Clicked {
            add { btnOK.Click += value; }
            remove { btnOK.Click -= value; }    
        }

        public event EventHandler btnCancel_Clicked
        {
            add { btnCancel.Click += value; }
            remove { btnCancel.Click -= value; }
        }
         
        public MethodHintEdit()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        internal void Cancel()
        {
            IsOK = false;
            this.Hide();
            this.SendToBack();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            IsOK = true;
            this.Hide();
            this.SendToBack();
        }

        private void txtHint_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOK.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape && this.Visible)
            {
                Cancel();
            }
        }

        internal void ShowPopup(MethodCodeFunctionObject methodItem)
        {
            lbMethodName.Text = methodItem.FullName;
            txtHint.Text = methodItem.GetMethodHint();

            if (string.IsNullOrWhiteSpace(txtHint.Text))
                txtHint.Text = methodItem.Comment.Trim();

            if (string.IsNullOrWhiteSpace(txtHint.Text))
                txtHint.Text = GetSummaryText(methodItem.DocComment.Trim());

            int x = Parent.Width / 2 - this.Width / 2;
            int y = Parent.Height / 2 - this.Height / 2;

            this.Left = x;
            this.Top = y;

            this.Show();
            this.BringToFront();

            txtHint.Focus();
        }

        private string GetSummaryText(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return "";
            // <doc><summary>...</summary></doc>
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.Element("doc")?.Element("summary").Value.Trim() ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
