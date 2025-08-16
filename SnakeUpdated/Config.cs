using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace SnakeSystem
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;

        // Snake Game Monitor Settings
        public float MonitoringInterval { get; set; } = 0.5f;

        // Market Settings
        public bool EnableMarket { get; set; } = true;
        public int MarketCooldownAfterRoundStart { get; set; } = 180; // seconds (3 minutes)

        // Performance Settings
        public int MaxPlayerDataAge { get; set; } = 60; // seconds
        public float PlayerDetectionRange { get; set; } = 5.0f; // meters
        public int AntiSpamDelay { get; set; } = 5; // seconds

        // Permission Settings
        public string DoubleXpPermission { get; set; } = "snake.doublexp";
        public float XpMultiplier { get; set; } = 2.0f;

        // Market Items Configuration
        public List<MarketItemConfig> MarketItems { get; set; } = new List<MarketItemConfig>
        {
            new MarketItemConfig { Code = "medkit", DisplayName = "Medkit", Price = 10, ItemType = "Medkit" },
            new MarketItemConfig { Code = "ammo556", DisplayName = "5.56 Ammo", Price = 15, AmmoType = "Nato556", AmmoAmount = 60 },
            new MarketItemConfig { Code = "flashlight", DisplayName = "Flashlight", Price = 5, ItemType = "Flashlight" },
            new MarketItemConfig { Code = "keycard", DisplayName = "Scientist Keycard", Price = 25, ItemType = "KeycardScientist" },
            new MarketItemConfig { Code = "grenade", DisplayName = "Grenade", Price = 30, ItemType = "GrenadeHE" },
            new MarketItemConfig { Code = "scp500", DisplayName = "SCP-500", Price = 50, ItemType = "SCP500" },
            new MarketItemConfig { Code = "armor", DisplayName = "Armor", Price = 35, ItemType = "ArmorCombat" },
            new MarketItemConfig { Code = "radio", DisplayName = "Radio", Price = 8, ItemType = "Radio" }
        };

        // Language Settings
        public LanguageConfig Language { get; set; } = new LanguageConfig();
    }

    public class MarketItemConfig
    {
        public string Code { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Price { get; set; } = 0;
        public string ItemType { get; set; } = "";
        public string AmmoType { get; set; } = "";
        public int AmmoAmount { get; set; } = 0;
        public bool Enabled { get; set; } = true;
    }

    public class LanguageConfig
    {
        // System Messages
        public string SystemEnabled { get; set; } = "Snake Market System enabled!";
        public string MonitoringStarted { get; set; } = "Snake game monitoring started";
        public string MonitoringStopped { get; set; } = "Snake game monitoring stopped";

        // Game Messages
        public string GameEnded { get; set; } = "Snake Game Ended!\nYou earned {0} points!\nTotal points: {1}\nUse .shop to see items!";
        public string GameEndedDoubleXp { get; set; } = "Snake Game Ended! (2X XP)\nYou earned {0} points (doubled from {1})!\nTotal points: {2}\nUse .shop to see items!";

        // Market Messages
        public string CurrentPoints { get; set; } = "Current points: {0}\n\n{1}\n\nFor detailed list: .shop";
        public string PurchaseSuccessful { get; set; } = "Purchase successful!";
        public string PurchaseFailed { get; set; } = "Purchase failed!\nFor valid item codes: .shop";
        public string InvalidItem { get; set; } = "Invalid item: '{0}'!\nUse .shop command to see items.";
        public string InsufficientPoints { get; set; } = "Insufficient points!\nRequired: {0} points\nCurrent: {1} points";
        public string ItemPurchased { get; set; } = "{0} purchased!\nRemaining points: {1}";
        public string InventoryFull { get; set; } = "Could not give item! Inventory might be full.";
        public string MarketCooldownActive { get; set; } = "Market is on cooldown!\nTime remaining: {0} seconds\nMarket will be available after round start cooldown.";

        // Command Descriptions
        public string ShopCommandDesc { get; set; } = "Buy items with Snake points";
        public string ScoreCommandDesc { get; set; } = "View Snake scores";

        // Score Board
        public string ScoreBoard { get; set; } = "SNAKE SCOREBOARD\n\nYour points: {0}\n\nTop scores:\n";
        public string PlayerOnlyCommand { get; set; } = "Only players can use this command!";

        // Market List
        public string ShopTitle { get; set; } = "SNAKE SHOP\n";
        public string ShopInstructions { get; set; } = "To buy: .shop <item_code>\n\n";
        public string ShopExample { get; set; } = "Example usage:\n.shop medkit\n.shop scp500\n.shop armor";
        public string QuickShopTitle { get; set; } = "Quick Shop List:\n\n";
    }
}