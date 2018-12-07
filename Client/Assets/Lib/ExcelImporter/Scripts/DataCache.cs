using System;
using System.Collections.Generic;

using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

#if UNITY_EDITOR
using Excel;
using System.Data;
#endif
namespace excel
{
    public class DataCache
    {
        public static string classPrefix = "excel.";
        private static Dictionary<String, List<BaseGamePo>> gamePoListMaps = new Dictionary<String, List<BaseGamePo>>();
        private static Dictionary<String, Dictionary<int, BaseGamePo>> gamePoMapMaps = new Dictionary<string, Dictionary<int, BaseGamePo>>();

#if UNITY_EDITOR
        /**
         * 加载EXCEL
         * 2015/4/23 Johnny
         */
        public static void LoadDataFromExcelFile()
        {
            String sourceFile = Application.dataPath + "/Data/data.xlsx";
            string destinationFile = Application.dataPath + "/Data/data2.xlsx";
            File.Copy(sourceFile, destinationFile, true);

            Debug.Log("加载EXCEL 文件:" + destinationFile);
            FileStream stream = File.OpenRead(destinationFile);
            List<SheetBeanVo> sheetBeanVos = new List<SheetBeanVo>();
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            DataSet result = excelReader.AsDataSet();
            int tables = result.Tables.Count;
            for (int k = 0; k < tables; k++)
            {
                //Debug.logger.Log(result.Tables[k].TableName);
                int columns = result.Tables[k].Columns.Count;
                int rows = result.Tables[k].Rows.Count;
                if (!result.Tables[k].TableName.StartsWith("T-"))
                {
                    continue;
                }
                SheetBeanVo sheetBeanVo = new SheetBeanVo();
                List<String> fieldsNames = new List<string>();
                List<String> fieldsTypes = new List<string>();
                List<List<String>> sheetDatas = new List<List<String>>();
                for (int i = 0; i < rows; i++)
                {
                    List<String> dataValues = new List<string>();
                    for (int j = 0; j < columns; j++)
                    {
                        string word = result.Tables[k].Rows[i][j].ToString();
                        if (i == 0 && j == 0)
                        {
                            string keyword = word;
                            keyword = keyword.Split(',')[0].Split(':')[1];
                            sheetBeanVo.sheetKeyword = keyword;
                        }
                        if (i == 1)
                        {
                            if (word != null)
                            {
                                fieldsNames.Add(word);
                            }
                        }
                        if (i == 2)
                        {
                            if (word != null)
                            {
                                fieldsTypes.Add(word);
                            }
                        }
                        if (i >= 5)
                        {
                            dataValues.Add(word);
                        }
                    }
                    if (i >= 5)
                    {
                        sheetDatas.Add(dataValues);
                    }
                }
                sheetBeanVo.fieldsNames = fieldsNames;
                sheetBeanVo.fieldsTypes = fieldsTypes;
                sheetBeanVo.sheetDatas = sheetDatas;
                sheetBeanVos.Add(sheetBeanVo);
            }

            for (int i = 0; i < sheetBeanVos.Count; i++)
            {
                SheetBeanVo vo = sheetBeanVos[i];
                string className = Utils.firstUpper(Utils.getJavaStyleString(vo.sheetKeyword) + "Po");

                Assembly assem = Assembly.GetExecutingAssembly();
                object o = null;
                for (int k = 0; k < vo.sheetDatas.Count; k++)
                {
                    o = assem.CreateInstance(classPrefix+className, false, BindingFlags.ExactBinding, null, new System.Object[] { }, null, null);
                    for (int j = 0; j < vo.fieldsNames.Count; j++)
                    {
                        String fieldName = vo.fieldsNames[j];
                        String fieldType = vo.fieldsTypes[j];
                        String theValue = vo.sheetDatas[k][j];
                        fieldName = Utils.getJavaStyleString(fieldName);
                        //Debug.Log(className + "--" + fieldName + "--" + theValue);
                        var property = o.GetType().GetProperty(fieldName);
                        if (fieldType == "int")
                        {
                            if (theValue != null && !("".Equals(theValue)))
                            {
                                property.SetValue(o, Convert.ToInt32(theValue), null);
                            }
                            else
                            {
                                property.SetValue(o, 0, null);
                            }
                        }
                        else
                        {
                            property.SetValue(o, theValue, null);
                        }
                    }
                    if (!gamePoListMaps.ContainsKey(className))
                    {
                        gamePoListMaps[className] = new List<BaseGamePo>();
                        gamePoMapMaps[className] = new Dictionary<int, BaseGamePo>();
                    }
                    ((BaseGamePo)o).load();
                    gamePoListMaps[className].Add((BaseGamePo)o);
                    gamePoMapMaps[className][((BaseGamePo)o).id] = (BaseGamePo)o;
                }
            }
        }

        /**
         * 保存到二进制
         * 2015/4/23 Johnny
         */
        public static void SavaDataToBinaryFile()
        {
            string sPath = Application.streamingAssetsPath + "/game.dat";
            Debug.Log("写入二进制配置文件:" + sPath);
            if (File.Exists(sPath))
            {
                File.Delete(sPath);
            }
            Stream fStream = new FileStream(sPath, FileMode.CreateNew, FileAccess.ReadWrite);
            BinaryFormatter binFormat = new BinaryFormatter();//创建二进制序列化器
            binFormat.Serialize(fStream, gamePoListMaps);
            fStream.Close();
        }
#endif
        /**
         * 加载二进制
         * 2015/4/23 Johnny
         */
        public static void LoadDataFromBytes(byte[] p)
        {
            Debug.Log("通过二进制加载配置文件");
            MemoryStream fStream2 = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(fStream2);
            writer.Write(p);
            writer.Flush();
            fStream2.Position = 0;//重置流位置
            BinaryFormatter binFormat = new BinaryFormatter();//创建二进制序列化器
            gamePoListMaps = (Dictionary<string, List<BaseGamePo>>)binFormat.Deserialize(fStream2);//反序列化对象
            fStream2.Close();

            foreach (string key in gamePoListMaps.Keys)
            {
                List<BaseGamePo> pos = gamePoListMaps[key];
                if (!gamePoMapMaps.ContainsKey(key))
                {
                    gamePoMapMaps[key] = new Dictionary<int, BaseGamePo>();
                }
                for (int i = 0; i < pos.Count; i++)
                {
                    int id =((BaseGamePo)(pos[i])).id;
                    gamePoMapMaps[key][id]=(BaseGamePo)pos[i];
                }
            }
        }

        internal static List<ItemPo> fetchAllItem()
        {
            return getAll<ItemPo>("ItemPo");
        }
        public static List<T> getAll<T>(string className)
        {
            List<BaseGamePo> list = gamePoListMaps[className];
            return list.ConvertAll<T>(item => (T)(object)item);
        }

        public static BaseGamePo findEntity(String className, int id)
        {
            Dictionary<int, BaseGamePo> games = gamePoMapMaps[className];
            if (games.ContainsKey(id))
            {
                return games[id];
            }
            return null;
        }
    }
}
