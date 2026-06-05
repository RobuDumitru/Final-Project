using System;
using LostInAForgottenCity.Controls;

namespace LostInAForgottenCity.Engine
{
    public enum MovementType
    {
        Carefully,
        Normally,
        Quickly
    }

    public enum NavigationState
    {
        OnTheWay,    // object → object
        OnThePath,   // landmark → landmark
        OnTheRoad    // location → location
    }

    public static class MovementSystem
    {
        // ── Base costs ────────────────────────────

        // ON THE WAY base (seconds + sleep)
        private static (int seconds, double sleep)
            GetOnTheWayBase(TravelDistance distance)
        {
            return distance switch
            {
                TravelDistance.Immediate => (5,  0.25),
                TravelDistance.Close     => (10, 0.5),
                TravelDistance.Near      => (15, 0.75),
                TravelDistance.Far       => (20, 1.0),
                TravelDistance.Distant   => (30, 1.5),
                _ => (10, 0.5)
            };
        }

        // ON THE PATH base (minutes + sleep)
        private static (int minutes, double sleep)
            GetOnThePathBase(TravelDistance distance)
        {
            return distance switch
            {
                TravelDistance.Immediate => (5,  1.0),
                TravelDistance.Close     => (10, 2.0),
                TravelDistance.Near      => (15, 3.0),
                TravelDistance.Far       => (20, 4.0),
                TravelDistance.Distant   => (25, 5.0),
                _ => (10, 2.0)
            };
        }

        // ON THE ROAD base (minutes + sleep)
        private static (int minutes, double sleep)
            GetOnTheRoadBase(TravelDistance distance)
        {
            return distance switch
            {
                TravelDistance.Immediate => (30, 5.0),
                TravelDistance.Close     => (45, 8.0),
                TravelDistance.Near      => (60, 10.0),
                TravelDistance.Far       => (75, 13.0),
                TravelDistance.Distant   => (90, 15.0),
                _ => (45, 8.0)
            };
        }

        // ── Modifiers ─────────────────────────────

        private static double GetTimeModifier(
            MovementType type) => type switch
        {
            MovementType.Carefully => 1.5,
            MovementType.Normally  => 1.0,
            MovementType.Quickly   => 0.5,
            _ => 1.0
        };

        private static double GetSleepModifier(
            MovementType type) => type switch
        {
            MovementType.Carefully => 0.95,
            MovementType.Normally  => 1.0,
            MovementType.Quickly   => 1.1,
            _ => 1.0
        };

        private static int GetStaminaRestore(
            MovementType type,
            TravelDistance distance)
        {
            if (type != MovementType.Carefully) return 0;
            return distance switch
            {
                TravelDistance.Immediate => 1,
                TravelDistance.Close     => 1,
                TravelDistance.Near      => 1,
                TravelDistance.Far       => 2,
                TravelDistance.Distant   => 2,
                _ => 1
            };
        }

        // ── Calculate effect ──────────────────────

        public static StatEffect Calculate(
            NavigationState navState,
            TravelDistance distance,
            MovementType movType)
        {
            double timeMod = GetTimeModifier(movType);
            double sleepMod = GetSleepModifier(movType);
            int stamina = GetStaminaRestore(movType, distance);

            switch (navState)
            {
                case NavigationState.OnTheWay:
                {
                    var (seconds, sleep) =
                        GetOnTheWayBase(distance);
                    return new StatEffect
                    {
                        TimeSeconds = (int)Math.Round(
                            seconds * timeMod),
                        Sleep = -Math.Round(
                            sleep * sleepMod, 2),
                        Stamina = stamina
                    };
                }

                case NavigationState.OnThePath:
                {
                    var (minutes, sleep) =
                        GetOnThePathBase(distance);
                    return new StatEffect
                    {
                        TimeMinutes = (int)Math.Round(
                            minutes * timeMod),
                        Sleep = -Math.Round(
                            sleep * sleepMod, 2),
                        Stamina = stamina
                    };
                }

                case NavigationState.OnTheRoad:
                {
                    var (minutes, sleep) =
                        GetOnTheRoadBase(distance);
                    return new StatEffect
                    {
                        TimeMinutes = (int)Math.Round(
                            minutes * timeMod),
                        Sleep = -Math.Round(
                            sleep * sleepMod, 2),
                        Stamina = stamina
                    };
                }

                default:
                    return new StatEffect();
            }
        }

        // ── Gameline text ─────────────────────────

        public static string GetResultGameline(
            StatEffect effect)
        {
            var parts = new System.Collections.Generic
                .List<string>();

            if (effect.TimeMinutes > 0)
                parts.Add($"{effect.TimeMinutes} minutes passed");
            if (effect.TimeSeconds > 0)
                parts.Add($"{effect.TimeSeconds} seconds passed");
            if (effect.Sleep < 0)
                parts.Add($"{Math.Abs(effect.Sleep):0.##} " +
                    "sleep lost");
            if (effect.Stamina > 0)
                parts.Add($"{effect.Stamina} stamina restored");

            return "[ " + string.Join(". ", parts) + ". ]";
        }

        // ── Confirmation text ─────────────────────

        public static string GetConfirmationText(
            string destination,
            NavigationState navState,
            TravelDistance distance,
            MovementType movType)
        {
            var effect = Calculate(navState, distance, movType);

            string timeText = effect.TimeMinutes > 0
                ? $"{effect.TimeMinutes} min"
                : $"{effect.TimeSeconds} sec";

            string movText = movType switch
            {
                MovementType.Carefully => "carefully",
                MovementType.Normally  => "normally",
                MovementType.Quickly   => "quickly",
                _ => "normally"
            };

            string staminaText = effect.Stamina > 0
                ? $" +{effect.Stamina} stamina" : "";

            return $"Head to {destination} moving {movText}." +
                $"\n{timeText} — " +
                $"{Math.Abs(effect.Sleep):0.##} sleep lost" +
                $"{staminaText}";
        }
    }
}