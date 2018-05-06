using System;
using System.Collections.Generic;
//using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace MedicBot.Dialogs
{
    [Serializable]
    public class UseDatebase
    {
        #region private variance
        [NonSerialized]
        public SqlConnection connection;//连接的变量,用于建立连接
        public SqlCommand command = new SqlCommand();//创建一个SQL命令对象
        public SqlDataReader reader;//用于读取数据
        public string[] column;//记录查询结果的每一列

        /// <summary>
        /// 查询结果，每个元素代表表格中的一行。每个元素为数组，数组按列存储。
        /// </summary>
        public List<string[]> queryResul = new List<string[]>();
        #endregion

        #region method
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串，visual studio 中连接属性查看</param>
        public UseDatebase(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            command.Connection = connection;//设置命令所使用的数据库连接
            command.CommandType = CommandType.Text;//设置命令类型
        }

        /// <summary>
        /// 用于查询数据库，查询结果记录在qureyResul
        /// </summary>
        /// <param name="sql">查询语句</param>
        public void Query(string sql)
        {
            connection.Open();//打开数据库连接
            command.CommandText = sql;//命令的查询语句
            reader = command.ExecuteReader();//执行查询语句并返回reader
            column = new string[reader.FieldCount];//定义column，用于存储每一行记录
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    column[i] = reader[i].ToString();
                }
                queryResul.Add(column);
            }
            connection.Close();//关闭数据库连接
        }

        /// <summary>
        /// 用于插入记录
        /// </summary>
        /// <param name="sql">插入数据的语句</param>
        public void InsertDate(string sql)
        {
            connection.Open();//打开数据库连接
            command.CommandText = sql;//插入数据
            //Console.WriteLine(sql);
            command.ExecuteNonQuery();//执行语句
            connection.Close();//关闭数据库连接
        }

        /// <summary>
        /// 用来生成标准化查询
        /// </summary>
        /// <param name="fieldName">表头数组，表示表头的限制</param>
        /// <param name="fieldValue">与表头数组对应位置的限制</param>
        /// <param name="selectField">查询的字段</param>
        /// <param name="tableName">表格的名称</param>
        /// <returns></returns>
        public static string GenerateQuery(string tableName, string[] fieldName, string[] fieldValue, string[] selectField = null)
        {
            string returnResult = null;
            string constrainCondiction = null;//记录限制条件
            string findColumn = "*";//记录查找对象

            for (int i = 0; i < fieldName.Length - 1; i++)
            {
                constrainCondiction += tableName + '.' + fieldName[i].ToString() + " = \'" + fieldValue[i].ToString() + "\' and ";
            }
            if (fieldName.Length - 1 > 0)//如果多于一个限制查询条件
            {
                constrainCondiction += tableName + '.' + fieldName[fieldName.Length - 1].ToString() + " = \'" + fieldValue[fieldName.Length - 1].ToString() + "\'";
            }
            else //只有一个限制条件
            {
                constrainCondiction += tableName + '.' + fieldName[0].ToString() + " = \'" + fieldValue[0].ToString() + "\'";
            }
            

            if (selectField != null)
            {
                findColumn = "";
                for (int i = 0; i < selectField.Length - 1; i++)
                {
                    findColumn += selectField[i] + ", ";
                }
                findColumn += selectField[selectField.Length - 1];
            }

            returnResult = "select " + findColumn + " from " + tableName + " where " + constrainCondiction + ";";
            return returnResult;
        }

        /// <summary>
        /// 用来生成多重条件查询
        /// </summary>
        /// <param name="fieldName">表头数组，表示表头的限制</param>
        /// <param name="fieldValue">与表头数组对应位置的限制</param>
        /// <param name="selectField">查询的字段</param>
        /// <param name="tableName">表格的名称</param>
        /// <returns></returns>
        public static string GenerateQueryPlus(string tableName, string[] fieldName, List<string[]> fieldValue, string[] selectField = null)
        {
            string returnResult = null;
            string constrainCondiction = null;//记录限制条件
            string findColumn = "*";//记录查找对象

            for (int j = 0; j < fieldValue.Count; j++)
            {
                for (int i = 0; i < fieldName.Length - 1; i++)
                {
                    constrainCondiction += tableName + '.' + fieldName[i].ToString() + " = \'" + fieldValue[j][i].ToString() + "\' and ";
                }
                constrainCondiction += tableName + '.' + fieldName[fieldName.Length - 1].ToString() + " = \'" + fieldValue[j][fieldName.Length - 1].ToString() + "\'";
                constrainCondiction += " OR ";
            }
            constrainCondiction = constrainCondiction.TrimEnd(' ');
            constrainCondiction = constrainCondiction.TrimEnd('R');
            constrainCondiction = constrainCondiction.TrimEnd('O');
            constrainCondiction = constrainCondiction.TrimEnd(' ');

            if (selectField != null)
            {
                findColumn = "";
                for (int i = 0; i < selectField.Length - 1; i++)
                {
                    findColumn += selectField[i] + ", ";
                }
                findColumn += selectField[selectField.Length - 1];
            }

            returnResult = "select " + findColumn + " from " + tableName + " where " + constrainCondiction + ";";
            return returnResult;
        }

        /// <summary>
        /// 导入数据到表格中
        /// </summary>
        /// <param name="tableName">表格名</param>
        /// <param name="fieldName">字段名，表头</param>
        /// <param name="fieldValue">对应的插入值</param>
        /// <returns></returns>
        public static string GenerateInsert(string tableName, string[] fieldName, string[] fieldValue)
        {
            string returnResult = null;
            string fieldValueStr = "(";
            string fieldStr = "(";
            for (int i = 0; i < fieldName.Length - 1; i++)
            {
                fieldStr += fieldName[i] + ", ";
            }
            fieldStr += fieldName[fieldName.Length - 1] + ")";
            for (int i = 0; i < fieldValue.Length - 1; i++)
            {
                fieldValueStr += "'" + fieldValue[i] + "'" + ", ";
            }
            fieldValueStr += "'" + fieldValue[fieldValue.Length - 1] + "'" + ")";
            returnResult += "insert into " + tableName + " " + fieldStr + " values" + fieldValueStr + ";";
            return returnResult;
        }

        /// <summary>
        /// 按照txt中的表头,给对应数据库中的表插入数据。
        /// </summary>
        /// <param name="tableName">数据库中表格的名称</param>
        /// <param name="url">数据文件位置</param>
        /// <param name="data">UseDatebase类</param>
        public static void ImportData(string tableName, string url, UseDatebase data)
        {
            string sql;
            string[] lines = System.IO.File.ReadAllLines(url);
            int ColumnNum = (Regex.Matches(lines[0], "\t")).Count;
            string[] subLines = new string[ColumnNum + 1];//定义用于存储行信息的变量，在循环中使用，中间变量
            string[] fieldName = subLines = lines[0].Split(new char[] { '\t' });
            for (int i = 1; i < lines.Length; i++)
            {
                subLines = lines[i].Split(new char[] { '\t' });//对第i行进行分割，得到sublines数组
                sql = GenerateInsert(tableName, fieldName, subLines);
                data.InsertDate(sql);
            }
            //Console.WriteLine(ColumnNum.ToString());
        }
        #endregion
    }
}
