using CommandSystem;
using Exiled.API.Features;
using InventorySystem.Items.Keycards.Snake;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SnakeSystem.Commands
{
    public static class ResponseCleaner
    {
        public static string CleanResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return response;

            response = Regex.Replace(response, @"<[^>]*>", ""); 
            response = Regex.Replace(response, @"\[color[^\]]*\]", ""); 
            response = Regex.Replace(response, @"\[/color\]", "");
            response = Regex.Replace(response, @"<color[^>]*>", "");
            response = Regex.Replace(response, @"</color>", ""); 
            response = Regex.Replace(response, @"\{[^}]*\}", ""); 
            response = Regex.Replace(response, @"\\[a-zA-Z]+", ""); 
            response = Regex.Replace(response, @"\x1b\[[0-9;]*m", ""); 
            
            
            response = response.Replace("§", ""); 
            response = response.Replace("&", ""); 
            
            return response.Trim();
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class ShopCommand : ICommand
    {
        public string Command => "shop";
        public string[] Aliases => new[] { "s", "st", "store" };
        public string Description => SnakePlugin.Instance?.Config?.Language?.ShopCommandDesc ?? "Buy items with Snake points";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            var config = SnakePlugin.Instance.Config;
            
            if (player == null)
            {
                response = ResponseCleaner.CleanResponse(config.Language.PlayerOnlyCommand);
                return false;
            }

            if (arguments.Count == 0)
            {
                var playerScore = SnakeMarketSystem.GetPlayerScore(player.UserId);
                var rawResponse = string.Format(config.Language.CurrentPoints, playerScore, SnakeMarketSystem.GetQuickShopList()) + "\n" + SnakeMarketSystem.GetShopList();
                
                // Check if market is on cooldown and add warning
                if (SnakeEventManager.IsMarketOnCooldown(out int remainingSeconds))
                {
                    rawResponse = string.Format(config.Language.MarketCooldownActive, remainingSeconds) + "\n\n" + rawResponse;
                }
                
                response = ResponseCleaner.CleanResponse(rawResponse);
                return true;
            }

            var itemName = arguments.At(0).ToLower().Trim();

            Log.Debug($"Player {player.Nickname} attempting to buy: '{itemName}'");

            if (SnakeMarketSystem.ProcessMarketPurchase(player, itemName))
            {
                response = ResponseCleaner.CleanResponse(config.Language.PurchaseSuccessful);
                return true;
            }

            response = ResponseCleaner.CleanResponse(config.Language.PurchaseFailed);
            return false;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class ScoreCommand : ICommand
    {
        public string Command => "score";
        public string[] Aliases => new[] { "sc", "s", "points" };
        public string Description => SnakePlugin.Instance?.Config?.Language?.ScoreCommandDesc ?? "View Snake scores";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            var config = SnakePlugin.Instance.Config;
            
            if (player == null)
            {
                response = ResponseCleaner.CleanResponse(config.Language.PlayerOnlyCommand);
                return false;
            }

            var playerScore = SnakeMarketSystem.GetPlayerScore(player.UserId);
            var topScores = PlayerScoreManager.GetTopScores(5);

            var rawResponse = string.Format(config.Language.ScoreBoard, playerScore);

            for (int i = 0; i < topScores.Count; i++)
            {
                var score = topScores[i];
                rawResponse += $"{i + 1}. {score.LastKnownName}: {score.TotalScore} points\n";
            }

            response = ResponseCleaner.CleanResponse(rawResponse);
            return true;
        }
    }
}