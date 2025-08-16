using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using Exiled.API.Features;
using UnityEngine;
using InventorySystem.Items.Keycards.Snake;

namespace SnakeSystem
{
    public static class SnakeGameMonitor
    {
        private static CoroutineHandle _monitorCoroutine;
        private static Dictionary<Player, PlayerSnakeData> _playerSnakeData = new Dictionary<Player, PlayerSnakeData>();

        private class PlayerSnakeData
        {
            public int LastScore { get; set; }
            public SnakeDisplay LastDisplay { get; set; }
            public DateTime LastUpdate { get; set; }
            public bool IsPlayingSnake { get; set; }
        }

        public static void StartMonitoring()
        {
            if (_monitorCoroutine.IsRunning)
                Timing.KillCoroutines(_monitorCoroutine);

            _monitorCoroutine = Timing.RunCoroutine(MonitorSnakeGames());
            Log.Debug(SnakePlugin.Instance.Config.Language.MonitoringStarted);
        }

        public static void StopMonitoring()
        {
            if (_monitorCoroutine.IsRunning)
                Timing.KillCoroutines(_monitorCoroutine);

            _playerSnakeData.Clear();
            Log.Debug(SnakePlugin.Instance.Config.Language.MonitoringStopped);
        }

        private static IEnumerator<float> MonitorSnakeGames()
        {
            while (true)
            {
                try
                {
                    CheckAllSnakeDisplays();
                    CleanupOldData();
                }
                catch (Exception ex)
                {
                    Log.Error($"Snake monitor error: {ex}");
                }

                yield return Timing.WaitForSeconds(SnakePlugin.Instance.Config.MonitoringInterval);
            }
        }

        private static void CheckAllSnakeDisplays()
        {
            SnakeDisplay[] snakeDisplays;

            try
            {
                snakeDisplays = UnityEngine.Object.FindObjectsByType<SnakeDisplay>(FindObjectsSortMode.None);
            }
            catch (System.Exception)
            {
#pragma warning disable CS0618
                snakeDisplays = UnityEngine.Object.FindObjectsOfType<SnakeDisplay>();
#pragma warning restore CS0618
            }

            Log.Debug($"Found {snakeDisplays.Length} SnakeDisplay objects");

            // Önce tüm oyuncuları kontrol et
            foreach (var player in Player.List)
            {
                if (player == null || !player.IsAlive) continue;

                bool isHoldingSnakeCard = IsPlayerHoldingSnakeCard(player);
                Log.Debug($"Player {player.Nickname} - Holding snake card: {isHoldingSnakeCard}");

                if (isHoldingSnakeCard)
                {
                    var activeDisplay = FindActiveDisplayForPlayer(player, snakeDisplays);
                    if (activeDisplay != null)
                    {
                        Log.Debug($"Player {player.Nickname} - Found active display");
                        ProcessPlayerSnakeDisplay(player, activeDisplay);
                    }
                    else
                    {
                        Log.Debug($"Player {player.Nickname} - No active display found");
                    }
                }
                else
                {
                    if (_playerSnakeData.ContainsKey(player))
                    {
                        Log.Debug($"Player {player.Nickname} - No longer holding snake card, marking as not playing");
                        _playerSnakeData[player].IsPlayingSnake = false;
                    }
                }
            }
        }

        private static bool IsPlayerHoldingSnakeCard(Player player)
        {
            try
            {
                var currentItem = player.CurrentItem;
                if (currentItem == null)
                {
                    Log.Debug($"Player {player.Nickname} - No current item");
                    return false;
                }

                var itemType = currentItem.Type.ToString();
                Log.Debug($"Player {player.Nickname} - Current item: {itemType}");

                // Tüm keycard türlerini kontrol et
                bool isKeycard = itemType.Contains("Keycard") || itemType.Contains("Card");

                if (isKeycard)
                {
                    Log.Debug($"Player {player.Nickname} is holding a keycard: {itemType}");
                    // Keycard tutuyorsa muhtemelen Snake oynuyor
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error checking snake card for {player.Nickname}: {ex}");
                return false;
            }
        }

        private static SnakeDisplay FindActiveDisplayForPlayer(Player player, SnakeDisplay[] displays)
        {
            SnakeDisplay closestDisplay = null;
            float closestDistance = float.MaxValue;

            foreach (var display in displays)
            {
                try
                {
                    if (display == null) continue;

                    float distance = Vector3.Distance(player.Position, display.transform.position);
                    Log.Debug($"Player {player.Nickname} - Display distance: {distance}");

                    if (distance < SnakePlugin.Instance.Config.PlayerDetectionRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestDisplay = display;
                        Log.Debug($"Player {player.Nickname} - Found closer display at {distance}m");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error checking display distance: {ex}");
                }
            }

            if (closestDisplay != null)
            {
                Log.Debug($"Player {player.Nickname} - Active display found at {closestDistance}m");
            }

            return closestDisplay;
        }

        private static void ProcessPlayerSnakeDisplay(Player player, SnakeDisplay display)
        {
            try
            {
                if (display.ScoreText == null)
                {
                    Log.Debug($"Player {player.Nickname} - Display has no ScoreText");
                    return;
                }

                var scoreText = display.ScoreText.text;
                Log.Debug($"Player {player.Nickname} - Score text: '{scoreText}'");

                // Farklı skor formatlarını dene
                int currentScore = 0;
                if (!int.TryParse(scoreText, out currentScore))
                {
                    // "Score: 123" formatı
                    var cleanText = scoreText.Replace("Score: ", "").Replace("SKOR: ", "").Replace("Puan: ", "").Trim();
                    if (!int.TryParse(cleanText, out currentScore))
                    {
                        Log.Debug($"Player {player.Nickname} - Could not parse score from: '{scoreText}'");
                        return;
                    }
                }

                Log.Debug($"Player {player.Nickname} - Current score: {currentScore}");

                if (!_playerSnakeData.ContainsKey(player))
                {
                    _playerSnakeData[player] = new PlayerSnakeData
                    {
                        LastScore = currentScore,
                        LastDisplay = display,
                        LastUpdate = DateTime.Now,
                        IsPlayingSnake = true
                    };
                    Log.Debug($"Player {player.Nickname} - Started tracking with score {currentScore}");
                    return;
                }

                var playerData = _playerSnakeData[player];

                if (playerData.LastDisplay != display)
                {
                    Log.Debug($"Player {player.Nickname} - Switched to different display");
                    playerData.LastDisplay = display;
                    playerData.LastScore = currentScore;
                    playerData.LastUpdate = DateTime.Now;
                    playerData.IsPlayingSnake = true;
                    return;
                }

                // Skor değişikliği kontrolü
                if (currentScore != playerData.LastScore)
                {
                    Log.Debug($"Player {player.Nickname} - Score changed from {playerData.LastScore} to {currentScore}");

                    if (currentScore == 0 && playerData.LastScore > 0)
                    {
                        Log.Debug($"Player {player.Nickname} - Game ended! Final score: {playerData.LastScore}");

                        if (playerData.IsPlayingSnake && (DateTime.Now - playerData.LastUpdate).TotalSeconds < 30)
                        {
                            SnakeEventManager.NotifyScoreChange(player, playerData.LastScore, true);
                            Log.Debug($"Player {player.Nickname} - Awarding {playerData.LastScore} points");
                        }
                        else
                        {
                            Log.Warn($"Player {player.Nickname} - Not awarding points (not playing or too old)");
                        }

                        playerData.IsPlayingSnake = false;
                    }
                    else if (currentScore > playerData.LastScore)
                    {
                        Log.Debug($"Player {player.Nickname} - Score increased, confirming active play");
                        playerData.IsPlayingSnake = true;
                        SnakeEventManager.NotifyScoreChange(player, currentScore, false);
                    }

                    playerData.LastScore = currentScore;
                }

                playerData.LastUpdate = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing snake display for {player.Nickname}: {ex}");
            }
        }

        private static void CleanupOldData()
        {
            var playersToRemove = new List<Player>();

            foreach (var kvp in _playerSnakeData)
            {
                var player = kvp.Key;
                var data = kvp.Value;

                if (player == null || !player.IsConnected ||
                    (DateTime.Now - data.LastUpdate).TotalSeconds > SnakePlugin.Instance.Config.MaxPlayerDataAge)
                {
                    playersToRemove.Add(player);
                }
            }

            foreach (var player in playersToRemove)
            {
                _playerSnakeData.Remove(player);
                Log.Debug($"Cleaned up old data for player: {player?.Nickname}");
            }
        }

        public static void OnPlayerLeft(Player player)
        {
            if (_playerSnakeData.ContainsKey(player))
            {
                _playerSnakeData.Remove(player);
                Log.Debug($"Cleaned up snake data for disconnected player: {player?.Nickname}");
            }
        }
    }
}