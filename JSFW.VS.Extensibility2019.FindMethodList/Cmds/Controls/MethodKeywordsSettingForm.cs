using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSFW.VS.Extensibility.Cmds.Controls
{
    public partial class MethodKeywordsSettingForm : Form
    {
        public bool IsDataModified { get; set; } = false;

        public MethodKeywordsSettingForm()
        {
            InitializeComponent();
        }

        private void keywordEditControl1_EditCommit(KeywordEditControl obj)
        {
            // 추가! 아이템. 
            bool hasKeyword = false;
            foreach (KeywordEditControl kwEdit in flowLayoutPanel1.Controls)
            {
                if (kwEdit.KeyWordText.ToUpper().Trim().Contains( obj.KeyWordText.ToUpper().Trim())) {
                    hasKeyword = true;
                    break;
                }
            }
            if (hasKeyword == false)
            {
                KeywordEditControl edit = new KeywordEditControl();
                edit.SetKeywordInfo(new KeywordClass() { 
                        Name = obj.KeyWordText, 
                        HTMLColor = KeywordClass.ConvertHTMLColor(obj.KeywordForeColor), 
                        Comment = obj.Comment
                });
                flowLayoutPanel1.Controls.Add(edit);
            }
            else
            {
                // 중복!
            }
            keywordEditControl1.ClearValues();

        }
         
        private void btnAdd_Click(object sender, EventArgs e)
        {
            keywordEditControl1.DoEditCommit();
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            foreach (KeywordEditControl kwEdit in flowLayoutPanel1.Controls)
            {
                kwEdit.ShowCheck();
            }

            btnDelOK.BringToFront();
            btnDelCancel.BringToFront();
        }

        private void btnDelCancel_Click(object sender, EventArgs e)
        {
            foreach (KeywordEditControl kwEdit in flowLayoutPanel1.Controls)
            {
                kwEdit.HideCheck();
            } 
            btnDelOK.SendToBack();
            btnDelCancel.SendToBack();
        }

        private void btnDelOK_Click(object sender, EventArgs e)
        {
            List<KeywordEditControl> dels = new List<KeywordEditControl>(); 
            foreach (KeywordEditControl kwEdit in flowLayoutPanel1.Controls)
            {
                if (kwEdit.IsSelected)
                {
                    dels.Add(kwEdit);
                } 
                kwEdit.HideCheck();
            }

            for (int loop = 0; loop < dels.Count; loop++)
            {
                using (dels[loop])
                {
                    flowLayoutPanel1.Controls.Remove(dels[loop]);
                }
            } 
            btnDelOK.SendToBack();
            btnDelCancel.SendToBack(); 
        }

        private void Save()
        {
            List<KeywordClass> newKeywords = new List<KeywordClass>();
            foreach (KeywordEditControl kwEdit in flowLayoutPanel1.Controls)
            {
                newKeywords.Add(new KeywordClass() { 
                    Name = kwEdit.KeyWordText, 
                    HTMLColor = KeywordClass.ConvertHTMLColor(kwEdit.KeywordForeColor), 
                    Comment = kwEdit.Comment,    
                });
            }
            MethodList.MethodCodeFunctionObject.Save_Keywords(newKeywords);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Save();
            this.Close(); 
        }

        internal void Load_Keywords()
        {
            MethodList.MethodCodeFunctionObject.Load_Keywords();

            // 바인딩.
            DataBind();
        }

        private void DataBind()
        {
            try
            {
                DataClear();
                foreach (var kw in MethodList.MethodCodeFunctionObject.Keywords)
                {
                    KeywordEditControl edit = new KeywordEditControl();
                    edit.SetKeywordInfo(kw);
                    flowLayoutPanel1.Controls.Add(edit);
                }
            }
            catch ( Exception ex) {
                MessageBox.Show(ex.Message, "확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DataClear()
        {
            for (int loop = flowLayoutPanel1.Controls.Count - 1; loop >= 0; loop--)
            {
                using (flowLayoutPanel1.Controls[loop]) { }
            }
        }

        internal void SetMethodName(string methodName)
        {
            int idx = methodName.IndexOf('(');
            if (0 < idx)
            {
                methodName = methodName.Substring(0, idx);
            }

            KeywordClass mth = MethodList.MethodCodeFunctionObject.Keywords.Find(m => m.Name == methodName);
            if (mth != null)
            {
                keywordEditControl1.SetKeywordInfo(mth);
            }
            else
            {
                keywordEditControl1.SetKeywordText(methodName);
            }
            

        }
    }
}
