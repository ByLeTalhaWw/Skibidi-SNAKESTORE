using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Exiled.API.Features;
using UnityEngine;
using InventorySystem.Items.Keycards.Snake;

namespace SnakeSystem
{
    public static class SnakeDisplayPatcher
    {
        private static Harmony _harmony;
        private static Dictionary<object, int> _lastScores = new Dictionary<object, int>();
        private static Dictionary<object, bool> _lastGameOverStates = new Dictionary<object, bool>();

        public static void PatchMethods()
        {
            try
            {
                _harmony = new Harmony("SnakeSystem.DisplayPatcher");

                var originalSetDisplay = typeof(SnakeDisplay).GetMethod("SetDisplay",
                    BindingFlags.Public | BindingFlags.Instance);

                var patchSetDisplay = typeof(SnakeDisplayPatcher).GetMethod("SetDisplayPostfix",
                    BindingFlags.Static | BindingFlags.Public);

                _harmony.Patch(originalSetDisplay, postfix: new HarmonyMethod(patchSetDisplay));

                Log.Debug("SnakeDisplay patching successful!");
            }
            catch (Exception ex)
            {
                Log.Error($"Snake patching failed: {ex}");
            }
        }

        public static void UnpatchMethods()
        {
            try
            {
                _harmony?.UnpatchAll("SnakeSystem.DisplayPatcher");
                _lastScores.Clear();
                _lastGameOverStates.Clear();
            }
            catch (Exception ex)
            {
                Log.Error($"Snake unpatching failed: {ex}");
            }
        }

        // C# 7.3 uyumlu postfix metodu
        public static void SetDisplayPostfix(SnakeDisplay __instance, List<Vector2Int> positions,
            Vector2Int nextMove, Vector2Int? nextFood, int score, bool gameover)
        {
            try
            {
                var player = GetPlayerFromSnakeDisplay(__instance);
                if (player == null) return;

                bool scoreChanged = !_lastScores.ContainsKey(__instance) || _lastScores[__instance] != score;
                bool gameOverStateChanged = !_lastGameOverStates.ContainsKey(__instance) || _lastGameOverStates[__instance] != gameover;

                if (scoreChanged || gameOverStateChanged)
                {
                    _lastScores[__instance] = score;
                    _lastGameOverStates[__instance] = gameover;

                    // Skor değişti veya oyun bitti, event'i tetikle
                    SnakeEventManager.NotifyScoreChange(player, score, gameover);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SetDisplayPostfix error: {ex}");
            }
        }

        private static Player GetPlayerFromSnakeDisplay(SnakeDisplay display)
        {
            try
            {
                // SnakeDisplay'in hangi oyuncuya ait olduğunu bulmaya çalış
                var transform = display.transform;

                // Parent objelerde oyuncu bilgisi arayalım
                while (transform != null)
                {
                    var collider = transform.GetComponent<Collider>();
                    if (collider != null)
                    {
                        // Collider'ın yakınında olan oyuncuyu bul
                        foreach (var player in Player.List)
                        {
                            if (player?.GameObject != null)
                            {
                                float distance = Vector3.Distance(player.Position, transform.position);
                                if (distance < 5.0f) // 5 metre yakınlık
                                {
                                    return player;
                                }
                            }
                        }
                    }
                    transform = transform.parent;
                }

                // Alternatif: Aktif olan Snake kartına sahip oyuncuyu bul
                foreach (var player in Player.List)
                {
                    if (player?.CurrentItem?.Type.ToString().Contains("Keycard") == true)
                    {
                        // Snake kartı kullanıyor olabilir
                        return player;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetPlayerFromSnakeDisplay error: {ex}");
            }

            return null;
        }
    }
}