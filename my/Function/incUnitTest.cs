using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Common
{
    public abstract class incUnitTest
    {
        public static void SetupColumm(DataTable databable_books)
        {
            DataColumn tempDataColumn = null;
            tempDataColumn = databable_books.Columns.Add("ID", Type.GetType("System.Int32"));
            tempDataColumn.AutoIncrement = true;//自动增加
            tempDataColumn.AutoIncrementSeed = 1;//起始为1
            tempDataColumn.AutoIncrementStep = 1;//步长为1
            tempDataColumn.AllowDBNull = false;//


            databable_books.Columns.Add("Product", Type.GetType("System.String"));
            databable_books.Columns.Add("Date", Type.GetType("System.DateTime"));
            databable_books.Columns.Add("BookID", Type.GetType("System.Int32"));
            databable_books.Columns.Add("Description", Type.GetType("System.String"));
        }

        public static DataTable GetOneValueDatatable(string cellvalue)
        {
            DataTable databable_books = new DataTable("Books");
            databable_books.Columns.Add("Product", Type.GetType("System.String"));
            DataRow newRow;
            newRow = databable_books.NewRow();
            newRow["Product"] = cellvalue;
            databable_books.Rows.Add(newRow);


            return databable_books;
        }

        public static DataTable GetOnetimeValueDatatable()
        {
            DataTable databable_books = new DataTable("Books");
            databable_books.Columns.Add("time", Type.GetType("System.DateTime"));
            DataRow newRow;
            newRow = databable_books.NewRow();
            newRow["time"] = DateTime.Now;
            databable_books.Rows.Add(newRow);


            return databable_books;
        }

        public static DataTable GetDatatable()
        {
            DataTable databable_books = new DataTable("Books");

            SetupColumm(databable_books);

            DataRow newRow;
            newRow = databable_books.NewRow();
            newRow["Product"] = "c++";
            newRow["BookID"] = 1;
            newRow["Date"] = new DateTime(1922, 11, 15);
            newRow["Description"] = "我很喜欢";
            databable_books.Rows.Add(newRow);

            newRow = databable_books.NewRow();
            newRow["Product"] = "c#";
            newRow["BookID"] = 2;
            newRow["Date"] = new DateTime(1922, 11, 16);
            newRow["Description"] = "very nice";
            databable_books.Rows.Add(newRow);

            newRow = databable_books.NewRow();
            newRow["Product"] = "java";
            newRow["BookID"] = 3;
            newRow["Date"] = new DateTime(1922, 11, 17);
            newRow["Description"] = "good id";
            databable_books.Rows.Add(newRow);

            return databable_books;
        }

        public static DataTable GetDatatableCustomCount(int count, int offset = 0)
        {
            if (count >= 0)
            {
                DataTable databable_books = new DataTable("Books");

                SetupColumm(databable_books);

                for (int i = 0; i < count; i++)
                {
                    DataRow newRow;
                    newRow = databable_books.NewRow();
                    newRow["Product"] = "book" + (i + 1).ToString();
                    newRow["BookID"] = i + 1 + offset;
                    newRow["Date"] = DateTime.Now;
                    newRow["Description"] = "我很喜欢" + (i + 1).ToString();
                    databable_books.Rows.Add(newRow);
                }

                return databable_books;
            }
            else
            {
                return null;
            }
        }

        public static DataTable GetDatatableSingleValue(string value)
        {
            DataTable databable_books = new DataTable("SingleValue");
            databable_books.Columns.Add("value", Type.GetType("System.String"));
            DataRow newRow;
            newRow = databable_books.NewRow();
            newRow["value"] = value;
            databable_books.Rows.Add(newRow);
            return databable_books;
        }


        public static DataTable GetDatatable_10000Record()
        {
            DataTable databable_books = new DataTable("Books");

            SetupColumm(databable_books);

            for (int i = 0; i < 100; i++)
            {
                DataRow newRow;
                newRow = databable_books.NewRow();
                newRow["Product"] = "c++" + i.ToString();
                newRow["BookID"] = i;
                newRow["Date"] = new DateTime(1922, 11, 15);
                newRow["Description"] = "我很喜欢" + i.ToString();
                databable_books.Rows.Add(newRow);
            }

            return databable_books;
        }

        public static DataTable GetDatatable2()
        {
            DataTable databable_books = new DataTable("Books");


            SetupColumm(databable_books);

            DataRow newRow;
            newRow = databable_books.NewRow();
            newRow["Product"] = "c++";
            newRow["BookID"] = 1;
            newRow["Date"] = new DateTime(1922, 11, 17);
            newRow["Description"] = "我很喜欢1";
            databable_books.Rows.Add(newRow);

            newRow = databable_books.NewRow();
            newRow["Product"] = "c#";
            newRow["BookID"] = 2;
            newRow["Date"] = new DateTime(1922, 11, 17);
            newRow["Description"] = "very nice1";
            databable_books.Rows.Add(newRow);

            newRow = databable_books.NewRow();
            newRow["Product"] = "java";
            newRow["BookID"] = 3;
            newRow["Date"] = new DateTime(1922, 11, 17);
            newRow["Description"] = "good id1";
            databable_books.Rows.Add(newRow);

            newRow = databable_books.NewRow();
            newRow["Product"] = "js";
            newRow["BookID"] = 4;
            newRow["Date"] = new DateTime(1922, 11, 17);
            newRow["Description"] = "good id1";
            databable_books.Rows.Add(newRow);

            return databable_books;
        }

        public static DataTable GetStringDatatableCustomCount(int columnCount)
        {
            DataTable databable_books = null;
            if (columnCount > 0)
            {
                databable_books = new DataTable("TemplateDatatable");

                for (int i = 0; i < columnCount; i++)
                {
                    databable_books.Columns.Add("column" + i, typeof(string));
                }
                return databable_books;
            }
            return databable_books;
        }

    }
}