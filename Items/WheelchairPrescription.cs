using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraWheelchair.Items
{
	public class WheelchairPrescription : ModItem
	{
		public override void SetStaticDefaults() 
		{
			DisplayName.AddTranslation(Terraria.Localization.GameCulture.Chinese, "����֤��");
			Tooltip.SetDefault("A prescription for wheelchair from your doctor\nFavorite this item to allow others to carry you with their wheelchair\n\"Need Help\"");
			Tooltip.AddTranslation(Terraria.Localization.GameCulture.Chinese, "ҽ�����ߵ�����֤����\n���Ϊϲ��ʱ�������˿��������ν�����\n���������Ҫ������");
		}

		public override void SetDefaults() 
		{
			item.scale = 0.5f;
			item.holdStyle = ItemHoldStyleID.HoldingUp;
			item.width = 10;
			item.height = 10;
			item.useStyle = 0;
			item.value = 10000;
			item.rare = ItemRarityID.Pink;
		}

        public override void AddRecipes() 
		{
			ModRecipe recipe = new ModRecipe(mod);
			//recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}