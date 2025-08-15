using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.Permissions.Extensions;

namespace SnakeSystem
{
    public static class SnakeMarketSystem
    {
        // Main product list - loaded from config
        public static Dictionary<string, MarketItem> GetMarketItems()
        {
            var config = SnakePlugin.Instance.Config;
            var marketItems = new Dictionary<string, MarketItem>();

            foreach (var itemConfig in config.MarketItems)
            {
                if (!itemConfig.Enabled) continue;

                ItemType? itemType = null;
                AmmoType? ammoType = null;

                // Parse ItemType if specified
                if (!string.IsNullOrEmpty(itemConfig.ItemType))
                {
                    if (System.Enum.TryParse<ItemType>(itemConfig.ItemType, out var parsedItemType))
                    {
                        itemType = parsedItemType;
                    }
                }

                // Parse AmmoType if specified
                if (!string.IsNullOrEmpty(itemConfig.AmmoType))
                {
                    if (System.Enum.TryParse<AmmoType>(itemConfig.AmmoType, out var parsedAmmoType))
                    {
                        ammoType = parsedAmmoType;
                    }
                }

                var marketItem = new MarketItem(
                    itemConfig.DisplayName,
                    itemConfig.Price,
                    itemType,
                    ammoType,
                    (ushort)itemConfig.AmmoAmount
                );

                marketItems[itemConfig.Code.ToLower()] = marketItem;
            }

            return marketItems;
        }

        public class MarketItem
        {
            public string DisplayName { get; set; }
            public int Price { get; set; }
            public ItemType? ItemType { get; set; }
            public AmmoType? AmmoType { get; set; }
            public ushort AmmoAmount { get; set; } // int -> ushort

            public MarketItem(string displayName, int price, ItemType? itemType = null, AmmoType? ammoType = null, ushort ammoAmount = 0) // int -> ushort
            {
                DisplayName = displayName;
                Price = price;
                ItemType = itemType;
                AmmoType = ammoType;
                AmmoAmount = ammoAmount;
            }
        }

        public static void ProcessGameScore(Player player, int finalScore)
        {
            if (finalScore <= 0) return;

            var config = SnakePlugin.Instance.Config;
            var currentTotal = GetPlayerScore(player.UserId);
            
            // Check for double XP permission
            bool hasDoubleXp = player.CheckPermission(config.DoubleXpPermission);
            int actualScore = finalScore;
            
            if (hasDoubleXp)
            {
                actualScore = (int)(finalScore * config.XpMultiplier);
                Log.Debug($"Player {player.Nickname} has double XP permission. Score: {finalScore} -> {actualScore}");
            }

            var newTotal = currentTotal + actualScore;
            PlayerScoreManager.SavePlayerScore(player.UserId, newTotal);

            // Show appropriate message based on XP multiplier
            string message = hasDoubleXp 
                ? string.Format(config.Language.GameEndedDoubleXp, actualScore, finalScore, newTotal)
                : string.Format(config.Language.GameEnded, actualScore, newTotal);

            player.ShowHint(message, 8);
        }

        public static int GetPlayerScore(string userId)
        {
            return PlayerScoreManager.GetPlayerScore(userId);
        }

        public static bool ProcessMarketPurchase(Player player, string itemName)
        {
            var cleanItemName = itemName.ToLower().Trim();
            var marketItems = GetMarketItems();
            var config = SnakePlugin.Instance.Config;

            if (!marketItems.ContainsKey(cleanItemName))
            {
                player.ShowHint(string.Format(config.Language.InvalidItem, itemName), 4);
                Log.Debug($"Player {player.Nickname} tried to buy invalid item: {itemName}");
                return false;
            }

            var item = marketItems[cleanItemName];
            var playerScore = GetPlayerScore(player.UserId);

            if (playerScore < item.Price)
            {
                player.ShowHint(string.Format(config.Language.InsufficientPoints, item.Price, playerScore), 4);
                return false;
            }

            if (GiveItemToPlayer(player, item))
            {
                PlayerScoreManager.SavePlayerScore(player.UserId, playerScore - item.Price);
                player.ShowHint(string.Format(config.Language.ItemPurchased, item.DisplayName, playerScore - item.Price), 4);
                Log.Debug($"Player {player.Nickname} bought {cleanItemName} for {item.Price} points");
                return true;
            }
            else
            {
                player.ShowHint(config.Language.InventoryFull, 3);
                return false;
            }
        }

        private static bool GiveItemToPlayer(Player player, MarketItem item)
        {
            try
            {
                if (item.ItemType.HasValue)
                {
                    player.AddItem(item.ItemType.Value);
                    Log.Debug($"Giving {item.ItemType.Value} to {player.Nickname}");
                    return true;
                }
                else if (item.AmmoType.HasValue)
                {
                    player.AddAmmo(item.AmmoType.Value, item.AmmoAmount);
                    Log.Debug($"Giving {item.AmmoAmount} {item.AmmoType.Value} ammo to {player.Nickname}");
                    return true;
                }

                Log.Error($"Item has no valid type: {item.DisplayName}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error giving item {item.DisplayName} to player {player.Nickname}: {ex}");
                return false;
            }
        }

        public static string GetShopList()
        {
            var config = SnakePlugin.Instance.Config;
            var marketItems = GetMarketItems();
            var list = "SNAKE SHOP\n";
            list += "To buy: .shop <item_code>\n\n";

            foreach (var item in marketItems)
            {
                list += $"- {item.Value.DisplayName}\n";
                list += $"  Code: {item.Key} | Price: {item.Value.Price} points\n\n";
            }

            list += "Example usage:\n.shop medkit\n.shop scp500\n.shop armor";

            return list;
        }

        public static string GetQuickShopList()
        {
            var config = SnakePlugin.Instance.Config;
            var marketItems = GetMarketItems();
            var list = "Quick Shop List:\n\n";

            foreach (var item in marketItems)
            {
                list += $"{item.Key} - {item.Value.DisplayName} ({item.Value.Price}p)\n";
            }

            return list;
        }
    }
}