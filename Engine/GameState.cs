using System;
using LostInAForgottenCity.Models;

namespace LostInAForgottenCity.Engine
{
    public class GameState
    {
        // Singleton
        private static GameState? _instance;
        public static GameState Instance => 
            _instance ??= new GameState();

        public GameEngine Engine { get; private set; } = new();
        public Player Player => Engine.CurrentPlayer;

        // Event fired whenever player stats change
        public event Action? OnStatsChanged;
        public event Action? OnLocationChanged;
        public event Action? OnInventoryChanged;
        public event Action? OnTimeChanged;

        private GameState() { }

        public void NotifyStatsChanged() => 
            OnStatsChanged?.Invoke();
        public void NotifyLocationChanged() => 
            OnLocationChanged?.Invoke();
        public void NotifyInventoryChanged() => 
            OnInventoryChanged?.Invoke();
        public void NotifyTimeChanged() => 
            OnTimeChanged?.Invoke();

        // ── Stat modifiers ───────────────────────

        public void ModifySanity(double amount)
        {
            Player.Sanity = Math.Clamp(
                Player.Sanity + amount, 0, 100);
            NotifyStatsChanged();
        }

        public void ModifyHP(int amount)
        {
            Player.HP = Math.Clamp(
                Player.HP + amount, 0, Player.MaxHP);
            NotifyStatsChanged();
        }

        public void ModifyStamina(int amount)
        {
            Player.Stamina = Math.Clamp(
                Player.Stamina + amount, 0, Player.MaxStamina);
            NotifyStatsChanged();
        }

        public void ModifySleep(int amount)
        {
            Player.Sleep = Math.Clamp(
                Player.Sleep + amount, 0, 100);
            NotifyStatsChanged();
        }

        public void ModifySoul(int amount)
        {
            Player.Soul = Math.Clamp(
                Player.Soul + amount, 0, Player.MaxSoul);
            NotifyStatsChanged();
        }

        public void ModifyResistance(int amount)
        {
            Player.Resistance = Math.Clamp(
                Player.Resistance + amount, 0, Player.MaxResistance);
            NotifyStatsChanged();
        }

        public void ModifySubconscious(int amount)
        {
            Player.Subconscious = Math.Clamp(
                Player.Subconscious + amount, 
                0, Player.MaxSubconscious);
            NotifyStatsChanged();
        }

        public void AdvanceTime(int minutes)
        {
            Player.Minute += minutes;
            while (Player.Minute >= 60)
            {
                Player.Minute -= 60;
                Player.Hour++;
            }
            while (Player.Hour >= 24)
            {
                Player.Hour -= 24;
                Player.Day++;
            }
            NotifyTimeChanged();
        }

        public void AddToInventory(string itemId)
        {
            if (Player.Inventory.Count < Player.MaxInventorySlots)
            {
                Player.Inventory.Add(itemId);
                NotifyInventoryChanged();
            }
        }

        public void AddRelic()
        {
            Player.Relics++;
            NotifyInventoryChanged();
        }

        public void AddToHistory(string text)
        {
            Player.NarrativeHistory.Add(text);
        }
    }
}