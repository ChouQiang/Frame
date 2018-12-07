/**
* ------------------------------------------------------------------------------
* created:	2014/10/31
* filename: 	MyScript\po\LvConfigPo.cs
* file path:	MyScript\po
* file base:	LvConfigPo
* file ext:	cs
* author:		UQIANTU
* 
* purpose:	等级配置信息
* ------------------------------------------------------------------------------
**/

using System;
using System.Collections.Generic;
namespace excel
{
    /**
     * EXCEL格子
     * 2015/4/23 Johnny
     */
    public class SheetBeanVo
    {
        /*
         *	关键词
         */
        public String sheetKeyword;
        /*
         *	字段名
         */
        public List<String> fieldsNames { get; set; }
        /*
         *	字段类型
         */
        public List<String> fieldsTypes { get; set; }
        /*
         *	sheet
         */
        public List<List<String>> sheetDatas { get; set; }

    }
}