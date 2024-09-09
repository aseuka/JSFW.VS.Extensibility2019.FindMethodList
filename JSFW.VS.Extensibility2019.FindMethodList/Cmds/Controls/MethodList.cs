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
using System.Windows.Input;
using EnvDTE;
using JSFW;
using JSFW.VS.Extensibility.FindMethodList;
using Microsoft.VisualStudio.Shell.Interop;

/*
    윈도우 글자크기 100% 일때 아이템 높이값이 12, > 125%가 되어 있으면 깨져서 보임. 
                  125% 일때 1.25 했더니 나름 괜찮게 보임. 
     
     */

namespace JSFW.VS.Extensibility.Cmds.Controls
{
    public partial class MethodList : Form
    {
        List<MethodList.MethodCodeFunctionObject> Elements { get; set; }

        TextSelection CurrentSelection;

        public MethodList()
        {
            InitializeComponent();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (Elements != null) Elements.Clear();
            Elements = null; CurrentSelection = null;
        }

        public string ProjectName { get; private set; }
        internal void SetProjectName(string prjName)
        {
            ProjectName = prjName;
        }

        internal void SetMethodList(List<MethodList.MethodCodeFunctionObject> elmts, TextSelection currentSelection)
        {
            CurrentSelection = currentSelection;
            Elements = elmts;
            MethodList.MethodCodeFunctionObject.Load_Keywords(ProjectName);
            MethodList.MethodCodeFunctionObject.Load_Hints(ProjectName);

            ListViewBind(Elements);
            SetComboBox();

            listView1.Refresh();
        }

        class GroupSort
        {
            public string Name { get; set; }
            public int FirstItemLineNumber { get; set; }
        }

        class GroupSortComarer : IComparer<GroupSort>
        {
            int IComparer<GroupSort>.Compare(GroupSort x, GroupSort y)
            {
                return x.FirstItemLineNumber.ToString("D10").CompareTo(y.FirstItemLineNumber.ToString("D10"));
            }
        }

        private void ListViewBind(List<MethodCodeFunctionObject> elements)
        {
            List<GroupSort> grpSort = new List<GroupSort>();

            Dictionary<string, List<MethodList.MethodCodeFunctionObject>> group = new Dictionary<string, List<MethodCodeFunctionObject>>();
            foreach (MethodList.MethodCodeFunctionObject item in elements)
            {
                if (group.ContainsKey(item.ClassName) == false)
                {
                    group.Add(item.ClassName, new List<MethodCodeFunctionObject>() { item });
                    grpSort.Add(new GroupSort() { Name = item.ClassName, FirstItemLineNumber = item.Line });
                }
                else
                {
                    group[item.ClassName].Add(item);
                }
            }

            for (int loop = listView1.Items.Count - 1; loop >= 0; loop--)
            {
                listView1.Items[loop].Tag = null;
            }
            listView1.Items.Clear();

            grpSort.Sort(new GroupSortComarer()); //클래스 순서대로 나타나게 하기 위해... 소팅함.

            foreach (var grp in grpSort)
            {
                var item = group[grp.Name];
                ListViewGroup lvg = new ListViewGroup(grp.Name);
                listView1.Groups.Add(lvg);

                foreach (var subitem in item)
                {
                    ListViewItem lstItem = new ListViewItem(subitem.DisplayText, lvg);
                    listView1.Items.Add(lstItem);
                    lstItem.Tag = subitem;
                }
            }

            group.Clear();
            group = null;
        }

        internal class MethodCodeFunctionObject
        {
            //string[] ImpMethodLists = new string[] {
            //    "sbFormInit",
            //    "sbGridInitSetting",
            //    "sbGridRemoveSetting",
            //    "sbGridColoring",
            //    "RibbonAddGroup",
            //    "SelectDataEvent",
            //    "SaveDataEvent",
            //    "SetButtonEnableEvent",
            //    "AfterSelectData",
            //    "AfterAddData",
            //    "AfterRowColChange",
            //    "_SetParameter",
            //    "_Loaded",
            //    "sbValidateCheck",
            //    "BeforeUserDataSetParam"
            //};

            //public bool IsNameContains
            //{
            //    get
            //    {
            //        bool isContains = false;
            //        foreach (var item in Keywords)
            //        {
            //            if (Name.ToUpper().Contains(item.ToUpper()))
            //            {
            //                isContains = true;
            //                break;
            //            }
            //        }
            //        return isContains;
            //    }
            //}

            internal KeywordClass GetKeyword()
            {
                if (Keywords != null)
                    return Keywords.Find(f => Name.ToUpper().Contains(f.Name.ToUpper()));
                return null;
            }

            internal string GetMethodHint()
            {
                string hint = null;
                bool hasFullName = Hints.Any(a => a.Key.ToUpper() == FullName.ToUpper());
                if (hasFullName) {
                    hint = Hints[FullName.ToUpper()];
                }
                return hint;
            }

            public string Name { get; set; }
            public int Line { get; set; }
            public CodeFunction Element { get; set; }

            public string DisplayText
            {
                get
                {
                    string display = Name;
                    if (Element != null)
                    {
                        List<string> prms = new List<string>();

                        foreach (CodeElement prm in Element.Parameters)
                        {
                            prms.Add(prm.Name);
                        }
                        display += string.Format("({0})", string.Join(", ", prms.ToArray()));

                        // 리턴타입
                        //string retType = this.Element.Type.AsFullName; 
                    }
                    return display;
                }
            }

            public string FullName { get; internal set; }
            public vsCMFunction FunctionKind { get; internal set; }

            public string ClassName
            {
                get
                {

                    int lastIndex = FullName.LastIndexOf('.');

                    if (0 <= lastIndex && lastIndex < FullName.Length)
                    {
                        return FullName.Substring(0, lastIndex);
                    }
                    else
                    {
                        return FullName;
                    }
                }

            }

            public override string ToString()
            {
                return Name;
            }

            internal static List<KeywordClass> Keywords { get; set; }
            internal static bool IsDataBinding = false;

            internal static void Load_Keywords(string prjName)
            {
                try
                {
                    string keywordsJson = "";
                    string keywordsFilePath = Path.Combine(VSPackage._DIR_JSFW + "MethodList", $"{prjName}.keywords.json");
                    if (File.Exists(keywordsFilePath))
                    {
                        keywordsJson = File.ReadAllText(keywordsFilePath, Encoding.UTF8);
                    }

                    IsDataBinding = true;
                    if (Keywords == null) Keywords = new List<KeywordClass>();

                    Keywords.Clear();
                    List<KeywordClass> data = ("" + keywordsJson).DeSerialize<List<KeywordClass>>();
                    if (data != null)
                    {
                        Keywords.AddRange(data.ToArray());
                    }
                    data = null;
                }
                catch (Exception ex)
                { }
                finally
                {
                    IsDataBinding = false;
                }
            }

            internal static void Save_Keywords(string prjName, List<KeywordClass> newKeywords)
            {
                try
                {
                    IsDataBinding = true;
                    if (Keywords == null) Keywords = new List<KeywordClass>();

                    Keywords.Clear();
                    if (newKeywords != null)
                        Keywords.AddRange(newKeywords.ToArray());
                     
                    string data = Keywords.Serialize();
                    string keywordsFilePath = Path.Combine(VSPackage._DIR_JSFW + "MethodList", $"{prjName}.keywords.json");
                    string root = Path.GetDirectoryName(keywordsFilePath);
                    if (!Directory.Exists(root)) {
                        Directory.CreateDirectory(root);
                    }
                    File.WriteAllText(keywordsFilePath, data, Encoding.UTF8); 
                }
                finally
                {
                    IsDataBinding = false;
                }
            }

            internal static Dictionary<string, string> Hints { get; set; }
            public string Comment { get; internal set; }
            public string DocComment { get; internal set; }

            internal static void Load_Hints(string prjName)
            {
                try
                {
                    string hintsJson = "";
                    string hintFilePath = Path.Combine( VSPackage._DIR_JSFW + "MethodList", $"{prjName}.hint.json");
                    if (File.Exists(hintFilePath))
                    {
                        hintsJson = File.ReadAllText(hintFilePath, Encoding.UTF8);
                    }                    
                    IsDataBinding = true;
                    if (Hints == null) Hints = new Dictionary<string, string>();

                    Hints.Clear();
                    Dictionary<string, string> data = ("" + hintsJson).DeSerialize<Dictionary<string, string>>();
                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            Hints.Add(item.Key.ToUpper(), item.Value);
                        }
                    }
                    data = null;
                }
                catch (Exception ex)
                { }
                finally
                {
                    IsDataBinding = false;
                }
            }
             
            internal static void Save_Hints(string prjName, Dictionary<string, string> newHints)
            {
                try
                {
                    IsDataBinding = true;
                    if (Hints == null) Hints = new Dictionary<string, string>();

                    Hints.Clear();
                    if (newHints != null)
                    {
                        foreach (var item in newHints)
                        {
                            Hints.Add(item.Key.ToUpper(), item.Value);
                        }
                    }
                    string data = Hints.Serialize();
                    string hintFilePath = Path.Combine(VSPackage._DIR_JSFW + "MethodList", $"{prjName}.hint.json");
                    string root = Path.GetDirectoryName(hintFilePath);
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }
                    File.WriteAllText(hintFilePath, data, Encoding.UTF8);
                }
                finally
                {
                    IsDataBinding = false;
                }
            }

            internal static void Save_Hints(string prjName, string methodFullName, string comment)
            {
                try
                {
                    IsDataBinding = true;
                    if (Hints == null) Hints = new Dictionary<string, string>();

                    if (Hints.ContainsKey(methodFullName.ToUpper()))
                    {
                        Hints[methodFullName.ToUpper()] = comment;
                    }
                    else
                    {
                        Hints.Add(methodFullName.ToUpper(), comment);
                    } 
                    string data = Hints.Serialize();
                    string hintFilePath = Path.Combine(VSPackage._DIR_JSFW + "MethodList", $"{prjName}.hint.json");
                    string root = Path.GetDirectoryName(hintFilePath);
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }
                    File.WriteAllText(hintFilePath, data, Encoding.UTF8);
                }
                finally
                {
                    IsDataBinding = false;
                }
            }
        }



        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            if (e.Index < 0) return;
            if (Elements == null) return;

            MethodCodeFunctionObject item = Elements[e.Index] as MethodCodeFunctionObject;
            if (item == null) return;

            KeywordClass kw = item.GetKeyword();
            if (kw != null)
            {
                TextRenderer.DrawText(e.Graphics, item.DisplayText, new Font(e.Font.FontFamily, e.Font.Size, FontStyle.Bold), e.Bounds.Location, kw.ForeColor);
            }
            else if (item.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
            {
                if (e.State == (DrawItemState.Selected | DrawItemState.Focus))
                {
                    // vs가 어둡게일때 - 글씨를 흰색으로 배경을 ... 
                    TextRenderer.DrawText(e.Graphics, item.DisplayText, new Font(e.Font.FontFamily, e.Font.Size, FontStyle.Bold), e.Bounds.Location, Color.White, Color.DodgerBlue);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, item.DisplayText, new Font(e.Font.FontFamily, e.Font.Size, FontStyle.Bold), e.Bounds.Location, Color.DodgerBlue);
                }
            }
            else
            {
                TextRenderer.DrawText(e.Graphics, item.DisplayText, e.Font, e.Bounds.Location, e.ForeColor);
            }
        }

        MethodKeywordsSettingForm settingForm { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            // 키워드 설정 팝업.
            if (settingForm == null)
            {
                settingForm = new MethodKeywordsSettingForm();
                settingForm.FormClosed += (fs, fe) =>
                {
                    settingForm = null;
                };
            }

            settingForm.Load_Keywords(ProjectName);

            if ((ModifierKeys & Keys.Control) == Keys.Control &&
                0 < listView1.SelectedItems.Count)
            {
                settingForm.SetMethodName(listView1.SelectedItems[0].Text);
            }

            settingForm.ShowDialog(this);

            listView1.Refresh();

            SetComboBox();
        }

        bool IsComboBoxSetting = false;
        private void SetComboBox()
        {
            try
            {
                IsComboBoxSetting = true;
                comboBox1.SelectedIndex = -1;
                comboBox1.Items.Clear();
                comboBox1.Items.Add("");
                foreach (var item in MethodCodeFunctionObject.Keywords)
                {
                    comboBox1.Items.Add(item.Name);
                }
            }
            finally
            {
                IsComboBoxSetting = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
            {
                listView1.FocusedItem.Focused = false;
            }
            if (listView1.SelectedIndices != null)
            {
                for (int loop = listView1.SelectedIndices.Count - 1; loop >= 0; loop--)
                {
                    listView1.SelectedItems[loop].Selected = false;
                }
                listView1.SelectedIndices.Clear();
            }

            //// 찾기. 
            //if (IsComboBoxSetting) return;

            //string item = ("" + comboBox1.SelectedItem).Trim();
            //if (string.IsNullOrEmpty(item) == false)
            //{
            //    for (int loop = 0; loop < listView1.Items.Count; loop++)
            //    {
            //        if (listView1.Items[loop].Text.ToUpper().Trim().Contains(item.ToUpper().Trim()))
            //        {
            //            listView1.SelectedIndices.Add(loop);
            //            listView1.SelectedItems[listView1.SelectedItems.Count - 1].Selected = true;
            //            listView1.SelectedItems[listView1.SelectedItems.Count - 1].Focused = true;
            //            listView1.FocusedItem = listView1.SelectedItems[listView1.SelectedItems.Count - 1];
            //        }
            //    }
            //}
            //if(listView1.FocusedItem != null) listView1.Select();
            listView1.Refresh();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems == null || listView1.SelectedItems.Count <= 0) return;

            // 선택
            MethodCodeFunctionObject item = listView1.SelectedItems[0].Tag as MethodCodeFunctionObject;
            if (item == null) return;
            int line = item.Line;
            if (0 <= line)
            {
                // 이동! 
                if (CurrentSelection != null) CurrentSelection.GotoLine(line, false);
                this.Close();
            }
        }

        private void MethodList_Resize(object sender, EventArgs e)
        {
            if (0 < listView1.Columns.Count)
                listView1.Columns[0].Width = listView1.Width - 8;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            if (e.ItemIndex < 0 || e.Item == null) return;
            if (Elements == null) return;

            MethodCodeFunctionObject item = e.Item.Tag as MethodCodeFunctionObject;
            if (item == null) return;

            Color ItemForeColor = e.Item.ForeColor;
            KeywordClass kw = item.GetKeyword();

            string hint = item.GetMethodHint();

            string displayText = $"{item.DisplayText,-45} {hint}".TrimEnd();

            Point pt = e.Bounds.Location; 
            pt.Y += 2;

            if (kw != null)
            {
                if (e.State == (ListViewItemStates.Selected | ListViewItemStates.Focused))
                {
                    // vs가 어둡게일때 - 글씨를 흰색으로 배경을 ... 
                    using (var fnt = new Font(e.Item.Font.FontFamily, e.Item.Font.Size, FontStyle.Bold))
                        TextRenderer.DrawText(e.Graphics, displayText, fnt, pt, e.Item.BackColor, kw.ForeColor);
                    ItemForeColor = e.Item.BackColor;
                }
                else
                {
                    using (var fnt = new Font(e.Item.Font.FontFamily, e.Item.Font.Size, FontStyle.Bold))
                        TextRenderer.DrawText(e.Graphics, displayText, fnt, pt, kw.ForeColor, e.Item.BackColor);
                    ItemForeColor = kw.ForeColor;
                }
            }
            else if (item.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
            {
                if (e.State == (ListViewItemStates.Selected | ListViewItemStates.Focused))
                {
                    // vs가 어둡게일때 - 글씨를 흰색으로 배경을 ... 
                    using (var fnt = new Font(e.Item.Font.FontFamily, e.Item.Font.Size, FontStyle.Bold))
                        TextRenderer.DrawText(e.Graphics, displayText, fnt, pt, Color.White, Color.DodgerBlue);
                    ItemForeColor = Color.White;
                }
                else
                {
                    using (var fnt = new Font(e.Item.Font.FontFamily, e.Item.Font.Size, FontStyle.Bold))
                        TextRenderer.DrawText(e.Graphics, displayText, fnt, pt, Color.DodgerBlue);
                    ItemForeColor = Color.DodgerBlue;
                }
            }
            else
            {
                if (e.State == (ListViewItemStates.Selected | ListViewItemStates.Focused))
                {
                    // vs가 어둡게일때 - 글씨를 흰색으로 배경을 ... 
                    using (var fnt = new Font(e.Item.Font.FontFamily, e.Item.Font.Size, FontStyle.Bold))
                        TextRenderer.DrawText(e.Graphics, displayText, fnt, pt, e.Item.BackColor, e.Item.ForeColor);
                    ItemForeColor = e.Item.BackColor;
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, displayText, e.Item.Font, pt, e.Item.ForeColor, e.Item.BackColor);
                    ItemForeColor = e.Item.ForeColor;
                }
            }

            string searchText = ("" + comboBox1.SelectedItem).Trim();
            if (string.IsNullOrWhiteSpace(searchText) == false
                && e.Item.Text.ToUpper().Contains(searchText.ToUpper()))
            {
                SizeF szText = e.Graphics.MeasureString(e.Item.Text, e.Item.Font);
                using (Pen pen = new Pen(ItemForeColor, 2f))
                    e.Graphics.DrawLine(pen, e.Bounds.X, e.Bounds.Y + e.Bounds.Height - 3f, e.Bounds.X + szText.Width + 10f, e.Bounds.Y + e.Bounds.Height - 3f);
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }

                // Draw the standard header background.
                e.DrawBackground();

                // Draw the header text.
                using (Font headerFont =
                            new Font(listView1.Font.FontFamily, 11f, FontStyle.Bold))
                {
                    e.Graphics.DrawString(e.Header.Text, headerFont,
                        Brushes.Black, e.Bounds, sf);
                }
            }

        }

        private void MethodList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                listView1.SelectedItems.Clear();
                listView1.Update();
            }
            else if (e.KeyCode == Keys.F2)
            {
                //수정! 
                if (0 < listView1.SelectedItems.Count)
                {
                    MethodCodeFunctionObject methodItem = listView1.SelectedItems[0].Tag as MethodCodeFunctionObject;
                    methodHintEdit1.ShowPopup(methodItem);
                }
            }
            else if (e.KeyCode == Keys.Escape) {
                if (methodHintEdit1.Visible) {
                    methodHintEdit1.Cancel();
                }
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            MethodList_KeyDown(sender, e);
        }

        private void methodHintEdit1_btnOK_Clicked(object sender, EventArgs e)
        {
            if (methodHintEdit1.IsOK)
            { 
                MethodCodeFunctionObject.Save_Hints(ProjectName, methodHintEdit1.MethodFullName, methodHintEdit1.Comment); 
                listView1.Refresh();
            }
        }

       
    }

    public class KeywordClass
    {
        public string Name { get; set; }

        public string HTMLColor { get; set; } = System.Drawing.ColorTranslator.ToHtml(Color.Black);

        public Color ForeColor
        {
            get
            {
                return System.Drawing.ColorTranslator.FromHtml(HTMLColor);
            }
        }

        public KeywordClass()
        {

        }

        internal static string ConvertHTMLColor(Color color)
        {
            return System.Drawing.ColorTranslator.ToHtml(color);
        }
    }
}
