# medical-bot
A robot about biomedical question and answer.

由于微软框架迁移到Azure上，之前的链接已无法访问。

将LUIS返回的结果进行组装，生成对应的SQL语句在数据库中进行查询，代码见：
https://github.com/cjn-chen/medical-bot/blob/master/MedicBot/database/useDatebase.cs

简单的医疗问答机器人， 询问常见的血液检测分子的正常水平，
可以尝试的问题为： “尿酸是什么？ ”，“血钾是什么？ ”，“尿酸 200 偏高吗？ ”“磷酸参考水平？ ”，“血清铁的英文名”“血清铁的简称” 。 

使用了微软的LUIS语义理解服务(https://www.luis.ai/applications)</br>Bot框架（https://dev.botframework.com/）</br>以及阿里云服务器（9.9包月版）。
