using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace Function
{
    public static class IncWeb
    {
        public static void AddNodes(TreeView tv, DataTable dt, string text,
            string value, string url, string rootselect, string rootselectvalue,
            int rootindex, string childselect, string childselectvalue, string ordervalue, string ovalue)
        {

            DataRow[] root = dt.Select(rootselect + "=" + rootselectvalue, ordervalue + " " + ovalue);
            for (int i = 0; i < root.Length; i++)
            {
                TreeNode tn = new TreeNode();
                tn.Text = root[i][text].ToString();
                tn.SelectAction = TreeNodeSelectAction.None;
                tn.Value = root[i][value].ToString();
                tv.Nodes.AddAt(rootindex, tn);
                if (i != root.Length - 1)
                {
                    tn.Expanded = false;
                }

                addchilid(dt, text, value, url, childselect, childselectvalue, tn, root[i], ordervalue, ovalue);
            }
        }
        private static void addchilid(DataTable dt, string text,
            string value, string url, string childselect,
            string childselectvalue, TreeNode ftn, DataRow fdr, string ordervalue, string ovalue)
        {
            DataRow[] child = dt.Select(childselect + "=" + fdr[childselectvalue].ToString(), ordervalue);
            for (int j = 0; j < child.Length; j++)
            {
                TreeNode ctn = new TreeNode();
                ctn.Text = child[j][text].ToString();
                ctn.NavigateUrl = child[j][url].ToString();
                ctn.Value = child[j][value].ToString();
                ftn.ChildNodes.Add(ctn);

                addchilid(dt, text, value, url, childselect, childselectvalue, ctn, child[j], ordervalue, ovalue);
            }
        }

        public static System.Drawing.Color readonlycolor()
        {
            return System.Drawing.Color.FromArgb(212, 208, 200);
        }

        /// <summary>
        /// 组合搜索sql语句
        /// </summary>
        /// <param name="wc"></param>
        /// <param name="cs">1为 加''"     *#null#* 代表除text之外的不做查询的约定,text不查询就为空</param>
        /// <param name="searchwhat">select [u2_id] from [lx_user2] where 1=1</param>
        /// <returns></returns>
        public static string searend(WebControl[] wc, string[,] cs, string searchwhat, bool[] likebool)
        {
            string str = "";
            str = searenda(wc, cs, likebool);
            if (str != "")
            {
                str = searchwhat + str;
            }
            return str;
        }

        private static string searenda(WebControl[] wc, string[,] cs, bool[] likebool)
        {
            string str = "";
            for (int i = 0; i < wc.Length; i++)
            {
                string test = wc[i].GetType().ToString().ToLower();
                switch (wc[i].GetType().ToString().ToLower())
                {
                    case "system.web.ui.webcontrols.textbox":
                        {
                            if (((TextBox)wc[i]).Text != "")
                            {
                                if (cs[i, 1] == "0")
                                {
                                    if (likebool[i])
                                    {

                                        str = str + " and " + cs[i, 0] + " like '%" + ((TextBox)wc[i]).Text + "%' ";
                                    }
                                    else
                                    {

                                        str = str + " and " + cs[i, 0] + "=" + ((TextBox)wc[i]).Text;
                                    }

                                }
                                else
                                {
                                    if (likebool[i])
                                    {

                                        str = str + " and " + cs[i, 0] + " like '%" + ((TextBox)wc[i]).Text + "%' ";
                                    }
                                    else
                                    {

                                        str = str + " and " + cs[i, 0] + "='" + ((TextBox)wc[i]).Text + "'";
                                    }

                                }
                            }
                            break;
                        }
                    case "system.web.ui.webcontrols.dropdownlist":
                        {
                            if (((DropDownList)wc[i]).SelectedValue != "*#null#*")
                            {
                                if (cs[i, 1] == "0")
                                {
                                    if (likebool[i])
                                    {
                                        str = str + " and " + cs[i, 0] + " like '%" + ((DropDownList)wc[i]).SelectedValue + "%' ";
                                    }
                                    else
                                    {
                                        str = str + " and " + cs[i, 0] + "=" + ((DropDownList)wc[i]).SelectedValue;
                                    }

                                }
                                else
                                {
                                    if (likebool[i])
                                    {
                                        str = str + " and " + cs[i, 0] + " like '%" + ((DropDownList)wc[i]).SelectedValue + "%' ";
                                    }
                                    else
                                    {
                                        str = str + " and " + cs[i, 0] + "='" + ((DropDownList)wc[i]).SelectedValue + "'";
                                    }
                                }
                            }
                            break;
                        }
                    case "system.web.ui.webcontrols.radiobuttonlist":
                        {

                            if (((RadioButtonList)wc[i]).SelectedValue != "*#null#*")
                            {
                                if (cs[i, 1] == "0")
                                {
                                    if (likebool[i])
                                    {
                                        str = str + " and " + cs[i, 0] + " like '%" + ((RadioButtonList)wc[i]).SelectedValue + "%' ";
                                    }
                                    else
                                    {

                                        str = str + " and " + cs[i, 0] + "=" + ((RadioButtonList)wc[i]).SelectedValue;
                                    }

                                }
                                else
                                {
                                    if (likebool[i])
                                    {
                                        str = str + " and " + cs[i, 0] + " like '%" + ((RadioButtonList)wc[i]).SelectedValue + "%' ";
                                    }
                                    else
                                    {
                                        str = str + " and " + cs[i, 0] + "='" + ((RadioButtonList)wc[i]).SelectedValue + "'";
                                    }

                                }
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return str;
        }

        /// <summary>
        /// *#null#*代表不做查询
        /// </summary>
        /// <param name="wc"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private static string searenda(string[] wc, string[,] cs)
        {
            string str = "";
            for (int i = 0; i < wc.Length; i++)
            {
                if (wc[i] != "*#null#*")
                {
                    if (cs[i, 1] == "0")
                    {
                        str = str + " and " + cs[i, 0] + "=" + wc[i];
                    }
                    else
                    {
                        str = str + " and " + cs[i, 0] + "='" + wc[i] + "'";
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// 检查Request.QueryString的参数是否都存在,否则转到错误页面
        /// </summary>
        /// <param name="qs"></param>
        /// <param name="errorurl"></param>
        public static void checkqstring(string[] qs, string errorurl)
        {

            foreach (string qs1 in qs)
            {
                if (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[qs1]))
                {
                    HttpContext.Current.Response.Redirect(errorurl, true);
                }
                break;
            }
        }

        /// <summary>
        /// 检查Request.QueryString的参数是否都存在
        /// </summary>
        /// <param name="qs"></param>
        /// <param name="errorurl"></param>
        public static bool checkqstring(string[] qs)
        {
            bool full = true;
            foreach (string qs1 in qs)
            {
                if (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[qs1]))
                {
                    full = false;
                }
                break;
            }
            return full;
        }

        /// <summary>
        /// 如果选择把value加上，如str += it.Value + ",";
        /// </summary>
        /// <param name="str"></param>
        /// <param name="cbl"></param>
        /// <returns></returns>
        public static string checklisttostring(CheckBoxList cbl)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (ListItem it in cbl.Items)
            {
                if (it.Selected == true)
                {
                    sb.Append(it.Value + ",");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 如果含有则选中(和前配合)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="cbl"></param>
        /// <returns></returns>
        public static void checklistfromstringok(string str, CheckBoxList cbl)
        {

            foreach (ListItem it in cbl.Items)
            {
                if (("," + str).Contains("," + it.Value + ","))
                {
                    it.Selected = true;
                }
            }
        }

        public static void checklistclear( CheckBoxList cbl)
        {

            foreach (ListItem it in cbl.Items)
            {
               
                    it.Selected =false;
            }
        }

        /// <summary>
        /// 如果含有则选中
        /// </summary>
        /// <param name="str"></param>
        /// <param name="cbl"></param>
        /// <returns></returns>
        public static void checklistfromstring(string str, CheckBoxList cbl, string valuea)
        {

            foreach (ListItem it in cbl.Items)
            {
                if (("," + str).Contains("," + valuea + ","))
                {
                    it.Selected = true;
                }
            }
        }

        /// <summary>
        /// 如果含有则选中
        /// </summary>
        /// <param name="str"></param>
        /// <param name="cbl"></param>
        /// <returns></returns>
        public static void checklistfromstring(string str, CheckBoxList cbl)
        {

            foreach (ListItem it in cbl.Items)
            {

                if (("," + str).Contains("," + it.Value + ","))
                {
                    it.Selected = true;
                }
                else if (("," + str + ",").Contains("," + it.Value + "-1,"))
                {
                    it.Selected = true;
                }
            }
        }
    }
}