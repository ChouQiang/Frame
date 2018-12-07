
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace excel
{
    public class GameController : MonoBehaviour
    {
        public IEnumerator LoadDataFile()
        {
            string sPath = Utils.findReadOnlyStreamFile("game.dat");
            WWW www = new WWW(sPath);
            yield return www;
            DataCache.LoadDataFromBytes(www.bytes);
        }

        void Start()
        {

#if UNITY_EDITOR
            DataCache.LoadDataFromExcelFile();
            DataCache.SavaDataToBinaryFile();
            StartCoroutine(LoadDataFile());
#else
            instance.StartCoroutine(instance.LoadDataFile());
#endif

            List<ItemPo> list = DataCache.fetchAllItem();
            Debug.Log("Item count: " + list.Count);
            ItemPo po = ItemPo.findEntity(1);
            Debug.Log("item name:" + po.itemName);
        }
    }
}
