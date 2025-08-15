using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;

namespace SnakeSystem
{
    public static class SnakeEventManager
    {
        public static event Action<Player, int> OnSnakeGameEnd;
        public static event Action<Player, int> OnSnakeScoreChanged;

        public static void NotifyScoreChange(Player player, int newScore, bool isGameOver)
        {
            if (player == null) return;

            // Anti-spam: Aynı oyuncuya çok hızlı puan vermeyi önle
            if (isGameOver && newScore > 0)
            {
                // Son 5 saniye içinde puan almış mı kontrol et
                var lastScoreTime = GetLastScoreTime(player);
                if (lastScoreTime.HasValue && (DateTime.Now - lastScoreTime.Value).TotalSeconds < SnakePlugin.Instance.Config.AntiSpamDelay)
                {
                    Log.Warn($"Preventing duplicate score for {player.Nickname} - too soon after last score");
                    return;
                }

                SetLastScoreTime(player, DateTime.Now);

                if (OnSnakeGameEnd != null)
                    OnSnakeGameEnd(player, newScore);
                Log.Debug($"Snake game ended for {player.Nickname} with score {newScore}");
            }

            if (OnSnakeScoreChanged != null)
                OnSnakeScoreChanged(player, newScore);
        }

        // Son skor alma zamanlarını takip et
        private static System.Collections.Generic.Dictionary<string, DateTime> _lastScoreTimes =
            new System.Collections.Generic.Dictionary<string, DateTime>();

        private static DateTime? GetLastScoreTime(Player player)
        {
            if (_lastScoreTimes.ContainsKey(player.UserId))
                return _lastScoreTimes[player.UserId];
            return null;
        }

        private static void SetLastScoreTime(Player player, DateTime time)
        {
            _lastScoreTimes[player.UserId] = time;
        }

        public static void Initialize()
        {
            OnSnakeGameEnd += HandleGameEnd;

            // Oyuncu ayrılma event'ini dinle
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        }

        private static void HandleGameEnd(Player player, int score)
        {
            SnakeMarketSystem.ProcessGameScore(player, score);
        }

        private static void OnPlayerLeft(LeftEventArgs ev)
        {
            // Oyuncu ayrıldığında verilerini temizle
            if (_lastScoreTimes.ContainsKey(ev.Player.UserId))
            {
                _lastScoreTimes.Remove(ev.Player.UserId);
            }

            SnakeGameMonitor.OnPlayerLeft(ev.Player);
        }

        public static void Cleanup()
        {
            OnSnakeGameEnd = null;
            OnSnakeScoreChanged = null;
            _lastScoreTimes.Clear();

            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        }
    }
}