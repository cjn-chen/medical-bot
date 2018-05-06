using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MedicBot.Dialogs
{
    ///// <summary>
    ///// 用于解析参考值
    ///// </summary>
    class Reference
    {
        public List<FirstNode> firstnode = new List<FirstNode>();//第一个节点，个数同“:”的个数，按“;”划分
        public class Interval
        {
            public double down = new double();//下区间
            public double up = new double();//上区间
            public string unit = string.Empty;//计量单位
        }
        public class SecondNode
        {
            public string nameOfNode = string.Empty;
            public List<Interval> Interval = new List<Interval>();//用于记录参考值和单位
        }
        public class FirstNode
        {
            public string nameOfNode = string.Empty;
            public List<SecondNode> secondNode = new List<SecondNode>();
        }

        /// <summary>
        /// 输入参考值范围进行解析,构造函数
        /// </summary>
        /// <param name="text">参考值文本</param>
        public Reference(string text)
        {
            string[] splitFirstNodeName;//第二次分割，按冒号分割，得到第一节点的名称
            string[] splitFirstNode;//第一次分割，按分号分割
            string[] splitSecondNode;//第三次分割，按照空格分割，得到第二节点的块
            string[] splitSecondNodeName;//第四次划分，按照逗号划分，得到二级节点的名称
            string[] splitIncludeNumAndUnit;//分割后剩下单位和数字的部分
            string temp = string.Empty;//临时变量
            splitFirstNode = text.Split(';');

            for (int i = 0; i < splitFirstNode.Length; i++)
            {
                splitFirstNodeName = splitFirstNode[i].Split(':');
                FirstNode firstnode_temp = new FirstNode();
                if (splitFirstNodeName.Length > 1)
                {
                    firstnode_temp.nameOfNode = splitFirstNodeName[0];
                    splitSecondNode = splitFirstNodeName[1].Split(' ');
                }
                else
                {
                    splitSecondNode = splitFirstNodeName[0].Split(' ');
                }
                firstnode.Add(firstnode_temp);
                for (int j = 0; j < splitSecondNode.Length; j++)
                {
                    splitSecondNodeName = splitSecondNode[j].Split(',');
                    SecondNode secondNode_temp = new SecondNode();
                    secondNode_temp.nameOfNode = splitSecondNodeName[0];
                    firstnode[i].secondNode.Add(secondNode_temp);
                    splitIncludeNumAndUnit = splitSecondNodeName[1].Split('|');
                    for (int k = 0; k < splitIncludeNumAndUnit.Length; k++)
                    {
                        Interval intervalTemp = new Interval();
                        intervalTemp.unit = splitIncludeNumAndUnit[k].Split(new char[] { '(', ')' })[1];
                        if (splitIncludeNumAndUnit[k].Split(new char[] { '(', ')' })[0].Split('-')[1] == "#")
                        {
                            intervalTemp.up = 999999;//用这个数字表示正无穷
                        }
                        else
                        {
                            intervalTemp.up = Convert.ToDouble(splitIncludeNumAndUnit[k].Split(new char[] { '(', ')' })[0].Split('-')[1]);
                        }
                        intervalTemp.down = Convert.ToDouble(splitIncludeNumAndUnit[k].Split(new char[] { '(', ')' })[0].Split('-')[0]);

                        firstnode[i].secondNode[j].Interval.Add(intervalTemp);
                    }
                }
            }
        }
    }

    public class HelpMethod
    {
        /// <summary>
        /// 该函数根据ValueOfResult类型的selectField，确定需要查找的数据库对应表格的哪里列表头
        /// </summary>
        /// <param name="selectField">需要查询的表头</param>
        /// <param name="fieldOfTable">对应的表头的排列，从左到右</param>
        /// <returns>返回一个index数组，即哪个表头时需要查找的。</returns>
        public static List<int> translateSelectIndex(string[] selectField,string[] fieldOfTable)
        {
            List<int> result = new List<int>();
            foreach (var item in selectField)
            {
                for (int i = 0; i < fieldOfTable.Length; i++)
                {
                    if (item == fieldOfTable[i])
                    {
                        result.Add(i);//把需要查询的表头对应在表中的序号记录下来
                    }
                }
            }
            return (result);
        }

        /// <summary>
        /// 对原字符串进行修改，对数字加双引号进行区分
        /// </summary>
        /// <param name="text">输入的字符串</param>
        /// <returns>返回对数字进行修饰后的字符串</returns>
        public static string decorateNumber(string text)
        {
            string result = string.Empty;//用于存放结果
            bool isLastCharIsNum = false;
            int ascii;//记录当前字符串的ascii码
            for (int i = 0; i < text.Length; i++)//遍历字符串
            {
                ascii = (int)(text[i]);//将字符转化为ascii码，方便判断是否为数字
                if ((ascii >= 48 && ascii <= 57) || ascii == 46)
                {
                    //数字以及小数点的ascii码
                    if (!isLastCharIsNum)
                    {
                        //如果前一个不是数字，则加入双引号
                        result += "\"" + text[i];
                        isLastCharIsNum = true;
                    }
                    else
                    {
                        //如果前一个已经是数字，则不用加双引号
                        result += text[i];
                    }
                }
                else if (isLastCharIsNum)
                {
                    //对于非数字的处理，非数字且前一个为数字，加入双引号
                    result += "\"" + text[i];
                    isLastCharIsNum = false;
                }
                else if (!isLastCharIsNum)
                {
                    //对于非数字的处理，非数字且前一个不是数字，不加入双引号
                    result += text[i];
                    isLastCharIsNum = false;
                }
            }
            if (isLastCharIsNum)
            {
                //如果末尾有数字，则加入冒号作为结尾
                result += "\"";
            }
            return (result);
        }

        #region 用于判断查询内容的信号
        /// <summary>
        /// 用于判断，是否询问检查水平
        /// </summary>
        /// <param name="getValueAndType">ValueOfResult类，用来检查value和type</param>
        /// <returns>返回状态数字，0代表不是，2代表含数字和指标名，3代表还有单位</returns>
        public static int isAskLevel (ValueOfResult getValueAndType)
        {
            int result = 0;//0表示不包含数字，不是该类问题
            if (getValueAndType.DoHaveValue)
            {
                int typeNum = 0;//计算含有的type的种类
                string typeCombine = string.Empty;
                foreach (var item in getValueAndType.ValueAndType)
                {
                    typeCombine += item.type + ";";
                    typeNum += 1;
                }
                if (typeCombine.Contains("builtin.number") && typeCombine.Contains("IndicatorOfHealth"))
                {
                    if (typeNum == 2)
                    {
                        result = 2;//表示只含有数字和指标
                    }
                    else if (typeNum == 3 && typeCombine.Contains("unit"))
                    {
                        result = 3;//表示含有数字和指标，单位
                    }
                }
            }
            return (result);
        }

        /// <summary>
        /// 用来查找是否具有用于数据库查询的合适的type
        /// </summary>
        /// <param name="getValueAndType">value和type，使用type进行比对</param>
        /// <param name="checkType">需要检查的type</param>
        /// <returns>找到为true，没找到为false</returns>
        public static bool isHave(ValueOfResult getValueAndType, string[] checkType)
        {
            if (getValueAndType.DoHaveValue)
            {
                foreach (var item_ValueAndType in getValueAndType.ValueAndType)
                {
                    foreach (var item_checkType in checkType)
                    {
                        if (item_checkType == item_ValueAndType.type)
                        {
                            return (true);//找到相同的type，返回true
                        }
                    }
                }
            }
            else
            {
                return (false);
            }
            return (false);//找不到相同的type，返回false
        }
        #endregion

        #region 用于生成与查询水平有关的回答语句

        /// <summary>
        /// 用于生成分析参考值的结果
        /// </summary>
        /// <param name="getValueAndType">luis的result分析的结果</param>
        /// <param name="queryResult">查询结果</param>
        /// <param name="stateOfAskLevel">含有多少entity的分析</param>
        /// <returns></returns>
        public static string[] GenerateTextForAskLevel(ValueOfResult getValueAndType, List<string[]> queryResult, int stateOfAskLevel)
        {
            int numOfResult = 0;//返回的字符串变量的个数
            bool isHigh = false;//判断是否有偏高的体检结果
            bool isLow = false;//判断是否有偏低的体检结果
            List<string[]> compareResult;//用于记录不同的参考值区间
            compareResult = compareDiffLevel(getValueAndType, queryResult, stateOfAskLevel);
            if (compareResult[0].Length > 0)
            {
                numOfResult += 2;
                isLow = true;
            }
            if (compareResult[1].Length > 0)
            {
                numOfResult += 2;
                isHigh = true;
            }
            if (numOfResult == 0)
            {
                numOfResult = 1;
            }
            string[] result_textForShow = new string[numOfResult];//最终返回的结果
            int i = 0;
            string textForShowTemp = string.Empty;
            if (isLow)
            {
                textForShowTemp += "检查水平偏低,参考：";
                foreach (var itemLow in compareResult[0])
                {
                    textForShowTemp += itemLow + ",";
                }
                textForShowTemp = textForShowTemp.TrimEnd(',');
                result_textForShow[i] = textForShowTemp;
                textForShowTemp = "";
                i += 1;
                result_textForShow[i] = "检测水平过低可能的情况：" + queryResult[0][8] + "\n";
                i += 1;
            }
            if (isHigh)
            {
                textForShowTemp += "检查水平偏高,参考：";
                foreach (var itemLow in compareResult[1])
                {
                    textForShowTemp += itemLow + ",";
                }
                textForShowTemp = textForShowTemp.TrimEnd(',');
                result_textForShow[i] = textForShowTemp;
                textForShowTemp = "";
                i += 1;
                result_textForShow[i] = "检测水平过高可能的情况：" + queryResult[0][7] + "\n";
                i += 1;
            }
            else if (!isLow && compareResult[2][0] == "no" && stateOfAskLevel == 3)
            {
                //既没有偏高也没有偏低，而且具有单位
                result_textForShow[0] += "没有找到对应单位，以下为参考的区间:" + queryResult[0][6];
            }
            else if ((!isLow && compareResult[2][0] == "yes" && stateOfAskLevel == 3)|| (!isLow && stateOfAskLevel == 2))
            {
                result_textForShow[0] += "正常，以下为参考的区间:" + queryResult[0][6];
            }
            return (result_textForShow);
        }

        /// <summary>
        /// 比较检测值与数据库数据大小关系
        /// </summary>
        /// <param name="getValueAndType">对luis返回的result的分析结果</param>
        /// <param name="queryResult">数据库查询结果</param>
        /// <param name="stateOfAskLevel">用来判断有几个相关实体，是否含有单位</param>
        /// <returns>返回一个以string[]为单位的list，其中compareResult[0]是数值偏低的数据,compareResult[1]是数值偏高的数据</returns>
        public static List<string[]> compareDiffLevel(ValueOfResult getValueAndType, List<string[]> queryResult, int stateOfAskLevel)
        {
            List<string[]> compareResult = new List<string[]>();//最终结果，compareResult[0]记录测试值偏小的区间，compareResult[1]记录测试值偏大的区间
            List<string> biggerLevel = new List<string>();//记录偏大的区间 
            List<string> smallerLevel = new List<string>();//记录偏小的区间
            string[] isHaveUnit = new string[1] { "no" };
            string textToShow = string.Empty;
            Reference reference = new Reference(queryResult[0][6]);

            //找到数字的具体数值
            double number = new double();
            foreach (var item in getValueAndType.ValueAndType)
            {
                if (item.type == "builtin.number")
                {
                    number = Convert.ToDouble(item.value);
                    break;
                }
            }
            if (stateOfAskLevel == 2)
            {
                //只有数字和指标名的情况
                foreach (var itemFirstNode in reference.firstnode)
                {
                    foreach (var itemSecondNode in itemFirstNode.secondNode)
                    {
                        foreach (var itemInterval in itemSecondNode.Interval)
                        {
                            if (number < itemInterval.down)
                            {
                                string firstNodeTemp = string.Empty;
                                if (itemFirstNode.nameOfNode.Length > 0)
                                {
                                    //判断是否有一级节点的名称，有则加冒号
                                    firstNodeTemp = itemFirstNode.nameOfNode + ":";
                                }
                                textToShow += "\"" + firstNodeTemp + itemSecondNode.nameOfNode + " " + itemInterval.down + "-" + itemInterval.up + itemInterval.unit + "\"";
                                smallerLevel.Add(textToShow);
                                textToShow = "";
                            }
                            else if (number > itemInterval.up)
                            {
                                string firstNodeTemp = string.Empty;
                                if (itemFirstNode.nameOfNode.Length > 0)
                                {
                                    //判断是否有一级节点的名称，有则加冒号
                                    firstNodeTemp = itemFirstNode.nameOfNode + ":";
                                }
                                textToShow += "\"" + firstNodeTemp + itemSecondNode.nameOfNode + " " + itemInterval.down + "-" + itemInterval.up + itemInterval.unit + "\""; ;
                                biggerLevel.Add(textToShow);
                                textToShow = "";
                            }
                        }
                    }
                }
            }
            else if (stateOfAskLevel == 3)
            {
                //只有数字,指标名，单位的情况
                string unit = string.Empty;
                foreach (var item in getValueAndType.ValueAndType)
                {
                    if (item.type == "unit")
                    {
                        unit = item.value.ToUpper();
                        break;
                    }
                }
                foreach (var itemFirstNode in reference.firstnode)
                {
                    foreach (var itemSecondNode in itemFirstNode.secondNode)
                    {
                        foreach (var itemInterval in itemSecondNode.Interval)
                        {
                            if (itemInterval.unit.ToUpper() == unit)
                            {
                                isHaveUnit[0] = "yes";
                                if (number < itemInterval.down)
                                {
                                    string firstNodeTemp = string.Empty;
                                    if (itemFirstNode.nameOfNode.Length > 0)
                                    {
                                        //判断是否有一级节点的名称，有则加冒号
                                        firstNodeTemp = itemFirstNode.nameOfNode + ":";
                                    }
                                    textToShow += "\"" + firstNodeTemp + itemSecondNode.nameOfNode + " " + itemInterval.down + "-" + itemInterval.up + itemInterval.unit + "\"";
                                    smallerLevel.Add(textToShow);
                                    textToShow = "";
                                }
                                else if (number > itemInterval.up)
                                {
                                    string firstNodeTemp = string.Empty;
                                    if (itemFirstNode.nameOfNode.Length > 0)
                                    {
                                        //判断是否有一级节点的名称，有则加冒号
                                        firstNodeTemp = itemFirstNode.nameOfNode + ":";
                                    }
                                    textToShow += "\"" + firstNodeTemp + itemSecondNode.nameOfNode + " " + itemInterval.down + "-" + itemInterval.up + itemInterval.unit + "\""; ;
                                    biggerLevel.Add(textToShow);
                                    textToShow = "";
                                }
                            }
                        }
                    }
                }
            }
            string[] bigger = biggerLevel.ToArray();
            string[] small = smallerLevel.ToArray();
            compareResult.Add(small);
            compareResult.Add(bigger);
            compareResult.Add(isHaveUnit);
            return (compareResult);
        }
        #endregion
    }
}