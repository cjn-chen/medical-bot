using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MedicBot.Dialogs
{
    [Serializable]
    public class ValueOfResult
    {
        #region 变量
        private string value;//用于记录luisresult中的value，临时变量
        private List<valueAndType> valueAndTypeStr = new List<valueAndType>();//对Luis返回的结果解析，value和type组成的List
        private string[] fieldName;//查询的字段条件，作为限制条件
        private string[] fieldValue;//查询的字段值条件，作为限制条件 
        private string[] selectField;//需要查询的字段
        private bool _doHaveValue = false;//判断是否有value值，List Entity才有value值
        #endregion

        #region 可返回变量
        /// <summary>
        /// 由Luis返回的entity对应的value组成的字符串
        /// </summary>
        public List<valueAndType> ValueAndType { get { return valueAndTypeStr; }}

        /// <summary>
        /// 是否在Luis返回的结果中找到value
        /// </summary>
        public bool DoHaveValue { get { return _doHaveValue; }}

        /// <summary>
        /// 返回需要查询的字段在luis中的value值
        /// </summary>
        public string[] SelectValue { get { return selectField; } } 
        #endregion

        #region 构造方法
        /// <summary>
        /// 构造的时候，检索Luis返回的结果中涉及的value和对应的type。
        /// </summary>
        /// <param name="luisResult">Luis返回的结果</param>
        public ValueOfResult(LuisResult luisResult)
        {
            List<EntityRecommendation> luisResultEntitiesAfterClear = ClearDuplicateValue(luisResult);
            if (luisResultEntitiesAfterClear != null)
            {
                foreach (EntityRecommendation entityTemp in luisResultEntitiesAfterClear)
                {
                    valueAndType valueAndTypeStrTemp = new valueAndType();
                    value = getValue(entityTemp);
                    valueAndTypeStrTemp.value = value;
                    if (value != null && _doHaveValue == false)
                    {
                        _doHaveValue = true;
                    }
                    valueAndTypeStrTemp.type = entityTemp.Type;
                    valueAndTypeStr.Add(valueAndTypeStrTemp);
                }
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 该函数用于获取Luis返回的entity的value值，并整理形式
        /// </summary>
        /// <param name="query">Luis返回的结果</param>
        /// <param name="i">返回的第i个value值</param>
        /// <returns>返回的entity的value值，为字符串</returns>
        private string getValue(LuisResult query, int i)
        {
            string result = string.Empty;
            foreach (var item in query.Entities[i].Resolution.Values)
            {
                if (query.Entities[i].Resolution.Count == 0)
                {
                    result = null;
                }
                else
                {
                    result = item.ToString().Replace("[", string.Empty).Replace("]", string.Empty).Replace(" ", string.Empty).Replace("\"", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
                    return (result);
                }
            }
            return (result);
        }

        /// <summary>
        /// 该函数用于获取Luis返回的entity的value值，并整理形式
        /// </summary>
        /// <param name="query">Luis.entities[i],entity中的某一个</param>
        /// <returns>返回的entity的value值，为字符串</returns>
        private string getValue(EntityRecommendation query)
        {
            string result = string.Empty;
            foreach (var item in query.Resolution.Values)
            {
                if (query.Resolution.Count == 0)
                {
                    result = null;
                }
                else
                {
                    result = item.ToString().Replace("[", string.Empty).Replace("]", string.Empty).Replace(" ", string.Empty).Replace("\"", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
                    return (result);
                }
            }
            return (result);
        }

        /// <summary>
        /// 用value的值进行查询
        /// </summary>
        /// <param name="database">数据库连接用的类，陈剑楠定义</param>
        /// <param name="tableName">表格名</param>
        /// <param name="mappingListLimit">规定Luis返回的type和表头的对应关系，查询的限制条件</param>
        /// <param name="mappingListSelect">规定Luis返回的type和表头的对应关系，查询内容</param>
        /// <return>返回有查询的结果，查询结果与UseDatebase的类型一致</return>
        public List<string[]> QueryByValue(string ConnectString, string tableName,string[] typeForSelect, string[] typeForLimit)
        {
            string sql;
            List<string[]> result = new List<string[]>();
            List<string> tempSelectStr = new List<string>();
            List<string> tempFieldNameStr = new List<string>();
            List<string> tempFieldValueStr = new List<string>();
            //检索字典中，确定哪些type作为查询内容，哪些为查询条件
            foreach (valueAndType item in valueAndTypeStr)
            {
                foreach (string itemOftypeForSelect in typeForSelect)
                {
                    if (item.type == itemOftypeForSelect)
                    {
                        tempSelectStr.Add(item.value);
                    }
                }
                foreach (string itemOftypeForLimit in typeForLimit)
                {
                    if (item.type == itemOftypeForLimit)
                    {
                        tempFieldNameStr.Add(item.type);
                        tempFieldValueStr.Add(item.value);
                    }
                }
            }
            fieldName = new string[tempFieldNameStr.Count];
            fieldValue = new string[tempFieldNameStr.Count];
            selectField = new string[tempSelectStr.Count];
            for (int i = 0; i < tempFieldNameStr.Count; i++)
            {
                fieldName[i] = tempFieldNameStr[i];
                fieldValue[i] = tempFieldValueStr[i];
            }
            for (int i = 0; i < tempSelectStr.Count; i++)
            {
                selectField[i] = tempSelectStr[i];
            }

            //一直都是全部查询，即每个表头都查询，方便后续处理
            sql = UseDatebase.GenerateQuery(tableName, fieldName, fieldValue, null);
            UseDatebase data = new UseDatebase(ConnectString);
            data.Query(sql);
            result = data.queryResul;
            if (data.queryResul.Count == 0)
            {
                result = null;
            }
            return (result);
        }

        /// <summary>
        /// 过滤掉重复的value，因为我们只使用了type的名字和type的value值
        /// </summary>
        /// <param name="luisResult">输入luis返回的结果</param>
        /// <returns>刚回luis返回结果中的entities序列，Luis.entities</returns>
        private List<EntityRecommendation> ClearDuplicateValue(LuisResult luisResult)
        {
            List<EntityRecommendation> result = new List<EntityRecommendation>();
            string value;
            string valueOfresult;
            bool isFindInResult = false;//记录是否已经将value值的entity存储在result中
            
            //遍历entities序列，把每一个和result中已有的做比较，已经存储的不再存储
            for (int i = 0; i < luisResult.Entities.Count; i++)
            {
                value = getValue(luisResult, i);
                for (int j = 0; j < result.Count; j++)
                {
                    valueOfresult = getValue(result[j]);
                    if (value == valueOfresult)
                    {
                        isFindInResult = true;
                    }
                }
                if (isFindInResult == false)
                {
                    result.Add(luisResult.Entities[i]);
                }
                isFindInResult = false;
            }
            if (result.Count == 0)
            {
                result = null;
            }
            return (result);
        }

        /// <summary>
        /// 用于确定SelectValue，确定符合特定type要求的value
        /// </summary>
        /// <param name="typeForSelect">需要查询的type列表</param>
        public void generateSelectValue(string[] typeForSelect)
        {
            List<string> selectFieldTemp = new List<string>();
            foreach (var item1 in typeForSelect)
            {
                foreach (var item2 in ValueAndType)
                {
                    if (item1 == item2.type)
                    {
                        selectFieldTemp.Add(item2.value);
                    }
                }
            }
            selectField = selectFieldTemp.ToArray();
        }

        #endregion
    }
}