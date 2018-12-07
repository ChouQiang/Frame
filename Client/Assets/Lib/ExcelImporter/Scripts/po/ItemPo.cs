
using System;

namespace excel
{
	/**
	 *
	 * 描述 
	 *
	 * @author Johnny
	 * @version 
	 */
 [Serializable]
	public class ItemPo : BaseGamePo {


		/**
		*道具名等级
		**/
		public string itemName { get; set; }

		/**
		*道具描述需要经验
		**/
		public string description { get; set; }

		/**
		*道具iconnull
		**/
		public string icon { get; set; }

		/**
		*使用表达式null
		**/
		public string expression { get; set; }

        public int resultShow { get; set; }

        public int quality { get; set; }

        public override void load(){
            
        }
		public static ItemPo findEntity(int id)
		{
			return (ItemPo)DataCache.findEntity("ItemPo", id);
		}

	}
}
