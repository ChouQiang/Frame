using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace excel
{
    /**
     * 静态类基类
     * 2015/4/23 Johnny
     */
    [Serializable]
    public abstract class BaseGamePo
    {
        /**
	    * 主键
	    **/
        public int id { get; set; }
        /**
         * 加载
         * 2015/4/23 Johnny
         */
        public abstract void load();
    }
}
