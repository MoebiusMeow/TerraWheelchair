using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraWheelchair.Items
{
	public class WheelchairPrescription : ModItem
	{
		public Item item => Item;
		public override void SetStaticDefaults() 
		{
			DisplayName.AddTranslation(GameCulture.FromCultureName(GameCulture.CultureName.Chinese), "轮椅证明");
			Tooltip.SetDefault("A prescription for wheelchair from your doctor\nFavorite this item to allow others to carry you with their wheelchair\n\"Need Help\"");
			Tooltip.AddTranslation(GameCulture.FromCultureName(GameCulture.CultureName.Chinese), "医生出具的轮椅证明书\n标记为收藏时，其他人可以用轮椅接送你\n“这个人需要帮助”");
		}

		public override void SetDefaults() 
		{
			item.scale = 0.5f;
			item.holdStyle = ItemHoldStyleID.HoldUp;
			item.width = 10;
			item.height = 10;
			item.useStyle = 0;
			item.value = 1;
			item.rare = ItemRarityID.Pink;
		}

        public override void AddRecipes() 
		{
			Recipe recipe = CreateRecipe();
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}