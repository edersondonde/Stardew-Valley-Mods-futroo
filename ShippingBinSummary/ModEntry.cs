﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace ShippingBinSummary
{
    class ModEntry : Mod
    {
        private bool isCJBSellItemPriceLoaded;

        private DataModel Data = null!;
        private readonly Rectangle CoinSourceRect = new(5, 69, 6, 6);

        private bool showGui = false;
    
        string allItemsSellPriceString;
        int allItemsSellPrice;

        public override void Entry(IModHelper helper)
        {
            this.Monitor.Log("Loading ShippingBinSummaryMod...");
            isCJBSellItemPriceLoaded = this.Helper.ModRegistry.IsLoaded("CJBok.ShowItemSellPrice");
            if(!isCJBSellItemPriceLoaded)
            {
                this.Monitor.Log("CJBSellItemPrice mod not found! ShippingBinSummaryMod can't work without it!");
                return;
            }

            this.Data = helper.Data.ReadJsonFile<DataModel>("assets/data.json") ?? new DataModel(null);

            helper.Events.Input.CursorMoved += OnCursorMoved;
            helper.Events.Display.RenderedHud += OnPostRenderHudEvent;
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Building shippingBin = Game1.getFarm().buildings.Where(obj => obj.buildingType == "Shipping Bin").FirstOrDefault();
            
            List<Vector2> shippingBinTiles = new List<Vector2>();
            if (shippingBin != null)
            {
                shippingBinTiles.Add(new Vector2(shippingBin.tileX, shippingBin.tileY));
                shippingBinTiles.Add(new Vector2(shippingBin.tileX, shippingBin.tileY-1));
                shippingBinTiles.Add(new Vector2(shippingBin.tileX+1, shippingBin.tileY));
                shippingBinTiles.Add(new Vector2(shippingBin.tileX+1, shippingBin.tileY-1));
            }
            Vector2 mouseTile = e.NewPosition.Tile;
            if (mouseTile != null)
            {
                if (shippingBinTiles.Contains(mouseTile))
                {
                    allItemsSellPrice = 0;
                    foreach (var item in Game1.getFarm().getShippingBin(Game1.player))
                    {
                        int? price = GetSellPrice(item);
                        allItemsSellPrice += price != null ? price.Value : 0;
                    }
                    if (allItemsSellPrice > 0)
                    {
                        allItemsSellPriceString = allItemsSellPrice.ToString();
                        showGui = true;
                    } else
                    {
                        allItemsSellPriceString = "Empty";
                        showGui = true;
                    }
                } else
                {
                    showGui = false;
                }
            } else
            {
                showGui = false;
            }
        }
        private void OnPostRenderHudEvent(object sender, RenderedHudEventArgs e)
        {
            if(showGui)
            {
                Vector2 stringLength = Game1.smallFont.MeasureString(allItemsSellPriceString);
                bool isEmpty = allItemsSellPriceString == "Empty";

                int width = isEmpty ? (int)stringLength.X + Game1.tileSize / 2 : (int)stringLength.X + Game1.tileSize / 2 + 30;
                int height = (int)stringLength.Y + Game1.tileSize / 3 + 5;

                int x = (int)(Mouse.GetState().X) - width/2 + 15;
                int y = (int)(Mouse.GetState().Y) + Game1.tileSize / 2 + 10;

                IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White);
                Utility.drawTextWithShadow(Game1.spriteBatch, allItemsSellPriceString, Game1.smallFont, new Vector2(x + Game1.tileSize / 4, y + Game1.tileSize / 4), Game1.textColor);
                if(!isEmpty)
                {
                    Game1.spriteBatch.Draw(Game1.debrisSpriteSheet, new Vector2(x + width - 38, y + 18), this.CoinSourceRect, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                }
            }
        }
        private int? GetSellPrice(Item item)
        {
            if (!CanBeSold(item, this.Data.ForceSellable))
                return null;

            int price = Utility.getSellToStorePriceOfItem(item, countStack: true);
            return price >= 0
                ? price
                : null;
        }
        public static bool CanBeSold(Item item, ISet<int> forceSellable)
        {
            return
                (item is SObject obj && obj.canBeShipped())
                || forceSellable.Contains(item.Category);
        }
    }
}