using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Function
{
    public static class IncCommon
    {

        /// <summary>
        /// Method to make sure that user's inputs are not malicious
        /// </summary>
        /// <param name="text">User's Input</param>
        /// <param name="maxLength">Maximum length of input</param>
        /// <returns>The cleaned up version of the input</returns>
        public static string InputText(string text, int maxLength)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (text.Length > maxLength)
                text = text.Substring(0, maxLength);
            text = Regex.Replace(text, "[\\s]{2,}", " ");	//two or more spaces
            text = Regex.Replace(text, "(<[b|B][r|R]/*>)+|(<[p|P](.|\\n)*?>)", "\n");	//<br>
            text = Regex.Replace(text, "(\\s*&[n|N][b|B][s|S][p|P];\\s*)+", " ");	//&nbsp;
            text = Regex.Replace(text, "<(.|\\n)*?>", string.Empty);	//any other tags
            text = text.Replace("'", "''");
            return text;
        }


        /// <summary>
        /// Method to check whether input has other characters than numbers
        /// </summary>
        public static string CleanNonWord(string text)
        {
            return Regex.Replace(text, "\\W", "");
        }

        /// <summary>
        /// path: error/error.txt
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="funname"></param>
        public static void WriteToFile(string path,string content,string funname)
        {
            //try
            //{
            //    System.IO.FileStream fs1 = new System.IO.FileStream(HttpContext.Current.Server.MapPath("~")+"/"+path, System.IO.FileMode.Append);
            //    System.IO.StreamWriter sw1 = new System.IO.StreamWriter(fs1);
            //    sw1.WriteLine("**************************************************");
            //    sw1.WriteLine("日期:" + System.DateTime.Now);
            //    sw1.WriteLine("方法名:" + funname);
            //    sw1.WriteLine("内容:" + content);
            //    sw1.WriteLine("*************************************************");
            //    sw1.Close();
            //}
            //catch
            //{
            //    throw;
            //}
        }

        /// <summary>
        /// 一行一行写到后面，path error/error.txt
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void WriteToFile(string path, string[] content)
        {
            try
            {
                System.IO.FileStream fs1 = new System.IO.FileStream(HttpContext.Current.Server.MapPath(path), System.IO.FileMode.Append);
                System.IO.StreamWriter sw1 = new System.IO.StreamWriter(fs1);

                for (int i = 0; i < content.Length; i++)
                {
                    sw1.WriteLine(content[i]);
                }
                sw1.Close();
            }
            catch
            {
                throw;
            }
        }

        public static int shiftsting(string dayofweek)
        {
            int str = 0;
            switch (dayofweek)
            {
                case "Monday":
                    return 1;

                case "Tuesday":
                    return 2;

                case "Wednesday":
                    return 3;

                case "Thursday":
                    return 4;

                case "Friday":
                    return 5;

                case "Saturday":
                    return 6;

                case "Sunday":
                    return 7;
            }
            return str;
        }

        public static string weekbig(int dayofweek)
        {
            string str = string.Empty;
            switch (dayofweek)
            {
                case 1:
                    return "一";

                case 2:
                    return "二";

                case 3:
                    return "三";

                case 4:
                    return "四";

                case 5:
                    return "五";

                case 6:
                    return "六";

                case 7:
                    return "日";
            }
            return str;
        }
    }
}