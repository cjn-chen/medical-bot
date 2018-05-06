using MedicBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicBot
{

    [LuisModel("06a1f6c1-ab95-4a75-b106-85be98207a04", "6ab12b1554dc45acbf9dba61ac4bb8eb")]//id和key，key通过生成的url查看
    [Serializable]
    public class SimpleLuisDialog : LuisDialog<object>
    {
        #region 变量
        ValueOfResult getValueAndType;//用于使用Luis返回的结果进行查询，得到需要查询的数据库名称和表头
        List<string[]> queryResultHistory = new List<string[]>();
        string connectString = @"server=.;database=Medic;uid=jiannanchen;pwd=19921230CJNsxl";
        string[] fieldOfTable = new string[] { "IndicatorOfHealth", "englishName", "abbreviation", "related diseases", "hospital department", "introduction", "referenceValue", "higher", "lower", "advice", "reference" };
        string[] fieldOfTableChineseName = new string[] { "体检项目名称：", "英文名：", "英文缩写：", "相关疾病：", "对应科室：", "简要介绍：", "参考值：", "检验值过高：", "检验值过低：", "生活饮食及相关事项建议：", "参考链接：" };
        string[] typeForSelect = new string[] { "keyWordForAskInfo" };//用于查询的entity的类
        string[] typeForLimit = new string[] { "IndicatorOfHealth", "englishName" };//用于限制条件建立的entity的类
        string tableOfExamination = @"physicalExamination";//用于查询体检相关的信息,表格名称
        //string connectString = @"server=localhost;database=Medic;uid=jiannanchen;pwd=19921230CJNsxl";
        #endregion

        [LuisIntent("体检信息")]
        public async Task BodyCheck(IDialogContext context, LuisResult result)
        {
            string textToShow = string.Empty;
            List<string[]> queryResult = new List<string[]>();
            getValueAndType = new ValueOfResult(result);
            bool isHaveKey = HelpMethod.isHave(getValueAndType, typeForLimit);//判断是否含有需要的用于查询的关键词关键词
            if (isHaveKey)
            {
                //查询关键词
                queryResult = new List<string[]>(getValueAndType.QueryByValue(connectString, tableOfExamination, typeForSelect, typeForLimit).ToArray());//查询数据库
                if (queryResult.Count != 0)
                {
                    //如果查询得到相关信息，保证数据库返回了信息
                    int stateOfAskLevel = HelpMethod.isAskLevel(getValueAndType);
                    if (stateOfAskLevel != 0 && queryResult.Count == 1)
                    {
                        //如果是查询检测水平是否正常的话，执行如下代码
                        string[] textToShows = HelpMethod.GenerateTextForAskLevel(getValueAndType, queryResult, stateOfAskLevel);
                        foreach (var itemText in textToShows)
                        {
                            await context.PostAsync(itemText);
                        }
                    }
                    else
                    {
                        //不是查询检查水平是否正常，执行如下代码
                        List<int> indexOfSelectField = HelpMethod.translateSelectIndex(getValueAndType.SelectValue, fieldOfTable);//确定需要查询的内容
                        if (indexOfSelectField.Count == 0)
                        {
                            //如果没有指明要查询的内容，默认返回简要介绍
                            indexOfSelectField.Add(5);
                        }

                        foreach (var item in indexOfSelectField)
                        {//合成查询结果，返回需要的信息
                            for (int i = 0; i < queryResult.Count(); i++)
                            {
                                textToShow = fieldOfTableChineseName[item] + queryResult[i][item] + "\r\n";
                            }
                        }
                    }
                }
                else
                {
                    //数据库查询不到相关信息
                    textToShow = "数据库查询不到相关信息";
                }
            }
            else
            {
                //不含有查询关键词
                if (HelpMethod.isHave(getValueAndType, new string[] { "keyWordForAskInfo" }) && queryResultHistory.Count > 0)
                {
                    //判断是否含有需要需要查询的信息
                    //如果有，则认为是连续查询
                    getValueAndType.generateSelectValue(typeForSelect);
                    List<int> indexOfSelectField = HelpMethod.translateSelectIndex(getValueAndType.SelectValue, fieldOfTable);//确定需要查询的内容
                    foreach (var item in indexOfSelectField)
                    {//合成查询结果，返回需要的信息
                        for (int i = 0; i < queryResultHistory.Count(); i++)
                        {
                            textToShow = fieldOfTableChineseName[item] + queryResultHistory[i][item] + "\r\n";
                        }
                    }
                }
                else
                {
                    textToShow = "识别不出指标名称，请输入合适的关键词(如指标名称，指标英文名称)";
                }
            }

            if (queryResult.Count()>0)
            {
                queryResultHistory = queryResult;
            }
            if (textToShow.Length > 0)
            {
                await context.PostAsync(textToShow);
            }
            context.Wait(MessageReceived);
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string textToShow = string.Empty;
            List<string[]> queryResult = new List<string[]>();
            getValueAndType = new ValueOfResult(result);
            bool isHaveKey = HelpMethod.isHave(getValueAndType, typeForLimit);//判断是否含有需要的用于查询的关键词关键词
            int stateOfAskLevel = HelpMethod.isAskLevel(getValueAndType);
            if (stateOfAskLevel != 0 && isHaveKey)
            {
                //含有查询关键词，能够定义到某一行数据，由stateOfAskLevel判断为查询测试水平是否正常
                queryResult = new List<string[]>(getValueAndType.QueryByValue(connectString, tableOfExamination, typeForSelect, typeForLimit).ToArray());//查询数据库
                string[] textToShows = HelpMethod.GenerateTextForAskLevel(getValueAndType, queryResult, stateOfAskLevel);//比较单位，生成输出文本
                foreach (var itemText in textToShows)
                {
                    await context.PostAsync(itemText);
                }
            }
            else if (HelpMethod.isHave(getValueAndType, new string[] { "keyWordForAskInfo" }) && queryResultHistory.Count > 0)
            {
                //判断是否含有需要需要查询的信息
                //如果有，则认为是连续查询
                getValueAndType.generateSelectValue(typeForSelect);
                List<int> indexOfSelectField = HelpMethod.translateSelectIndex(getValueAndType.SelectValue, fieldOfTable);//确定需要查询的内容
                foreach (var item in indexOfSelectField)
                {//合成查询结果，返回需要的信息
                    for (int i = 0; i < queryResultHistory.Count(); i++)
                    {
                        textToShow = fieldOfTableChineseName[item] + queryResultHistory[i][item] + "\r\n";
                    }
                }
            }
            else
            {
                textToShow = "不知道你说什么？";
            }
            if (textToShow.Length > 0)
            {
                await context.PostAsync(textToShow);
            }
            context.Wait(MessageReceived);
        }
    }
}