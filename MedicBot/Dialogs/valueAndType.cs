using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MedicBot.Dialogs
{
    [Serializable]
    public class valueAndType
    {
        /// <summary>
        /// Luis返回的value的值
        /// </summary>
        public string value;
        /// <summary>
        /// Luis返回的type的值
        /// </summary>
        public string type;
    }
}