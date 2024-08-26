using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        }

        internal void ShowPopup(MethodCodeFunctionObject methodItem)
        {
            lbMethodName.Text = methodItem.FullName;
            txtHint.Text = methodItem.GetMethodHint();

            txtHint.Focus();

            int x = Parent.Width / 2 - this.Width / 2;
            int y = Parent.Height / 2 - this.Height / 2;

            this.Left = x;
            this.Top = y;

            this.Show();
            this.BringToFront();
        }
    }
}
