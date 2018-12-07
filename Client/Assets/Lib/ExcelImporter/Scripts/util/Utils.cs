using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace excel
{
    /**
     * 文件类
     * 2015/4/23 Johnny
     */
    class Utils
    {

        /**
         * 获得只读文件
         * 2015/4/23 Johnny
         */
        public static string findReadOnlyStreamFile(string fileName)
        {
            string sPath = Application.streamingAssetsPath + "/" + fileName;

            #if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_IOS
                sPath = "file://" + sPath;
            #endif
            return sPath;
        }

        /**
         * 写文件
         * 2015/4/23 Johnny
         */
        public static void writeObjectToFile(string fileName,System.Object instance)
        {
            Stream fStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite);
            BinaryFormatter binFormat = new BinaryFormatter();
            binFormat.Serialize(fStream, instance);
            fStream.Close();
        }

        /**
 * 获得JAVA风格字符串
 * 2015/4/23 Johnny
 */
        public static String getJavaStyleString(String fieldName)
        {
            char[] chars = fieldName.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char current = chars[i];
                if (current == '_')
                {
                    if ((i + 1) <= chars.Length - 1)
                    {
                        char c = chars[i + 1];
                        string value = c.ToString();
                        value = value.ToUpper();
                        chars[i + 1] = value[0];
                    }
                }
            }
            String result = new string(chars);
            result = result.Replace("_", "");
            return result;
        }

        /**
* 方法功能:
* 更新时间:2010-10-28, 作者:johnny
* @param javaFieldName
* @return 首字母大写
*/
        public static String firstUpper(String javaFieldName)
        {
            char[] chars = javaFieldName.ToCharArray();
            String single = chars[0].ToString().ToUpper();
            chars[0] = single[0];
            return new string(chars);
        }
    }
}
