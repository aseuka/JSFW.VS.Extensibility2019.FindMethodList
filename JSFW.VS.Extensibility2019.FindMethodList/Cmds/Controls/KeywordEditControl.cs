using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSFW.VS.Extensibility.Cmds.Controls
{
    public partial class KeywordEditControl : UserControl
    { 
        public string KeyWordText { get { return textBox1.Text.Trim(); } }

        public string Comment { get { return txtComment.Text.Trim(); } }

        public Color KeywordForeColor { get { return textBox1.ForeColor; } }

        public bool IsNEW { get; set; } = false;

        public bool IsSelected { get { return chkSelect.Checked; } }

        public KeywordEditControl()
        {
            InitializeComponent();
        }

        public void SetKeywordInfo(KeywordClass keyword)
        {
            textBox1.Text = keyword.Name;
            textBox1.ForeColor = keyword.ForeColor;
            txtComment.Text = keyword.Comment;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.AnyColor = true;
                dlg.Color = textBox1.ForeColor;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox1.ForeColor = dlg.Color;
                }
            }
        }

        internal void ClearValues()
        {
            // 초기화.
            textBox1.Clear();
            textBox1.ForeColor = Color.Black;
        }

        internal void DoEditCommit()
        {
            textBox1.Focus();
            SendKeys.SendWait("{ENTER}");
        }

        internal void ShowCheck()
        {
            chkSelect.Checked = false;
            chkSelect.Visible = true;
        }

        internal void HideCheck()
        {
            chkSelect.Checked = false;
            chkSelect.Visible = false;
        }

        public event Action<KeywordEditControl> EditCommit = null;

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (IsNEW) {
                    OnEditCommit(); 
                }
            }
        }

        private void OnEditCommit()
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                if (EditCommit != null)
                    EditCommit(this);
            }
        }

        internal void SetKeywordText(string methodName)
        {
            textBox1.Text = methodName;
        }
    }
}
