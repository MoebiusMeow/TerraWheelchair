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
		}

		public override void SetDefaults() 
		{
			item.scale = 0.5f;
			item.holdStyle = ItemHoldStyleID.HoldUp;
			item.width = 10;
			item.height = 10;
			item.useStyle = ItemUseStyleID.None;
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