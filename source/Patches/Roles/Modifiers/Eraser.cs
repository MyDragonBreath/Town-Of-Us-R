using System.Collections.Generic;
using System.Linq;
using TMPro;
using TownOfUs.Patches;
using UnityEngine;
using Hazel;
using TownOfUs.NeutralRoles.ExecutionerMod;
using TownOfUs.NeutralRoles.GuardianAngelMod;

namespace TownOfUs.Roles.Modifiers
{
    public class Eraser : Ability
    {
        public Dictionary<byte, (GameObject, GameObject, GameObject, TMP_Text)> Buttons = new Dictionary<byte, (GameObject, GameObject, GameObject, TMP_Text)>();


        private Dictionary<string, Color> EraseColorMapping = new Dictionary<string, Color>();

        public Dictionary<string, Color> EraseSortedColorMapping;

        public Dictionary<byte, string> Guesses = new Dictionary<byte, string>();


        public Eraser(PlayerControl player) : base(player)
        {
            Name = "Eraser";
            TaskText = () => "Guess the modifiers of the people and remove them mid-meeting";
            Color = Patches.Colors.Impostor;
            AbilityType = AbilityEnum.Eraser;

            RemainingErases = CustomGameOptions.EraserMod;

            

            if (CustomGameOptions.FlashOn > 0) EraseColorMapping.Add("Flash", Colors.Flash);
            if (CustomGameOptions.BaitOn > 0) EraseColorMapping.Add("Bait", Colors.Bait);
            if (CustomGameOptions.GiantOn > 0) EraseColorMapping.Add("Giant", Colors.Giant);
            if (CustomGameOptions.DiseasedOn > 0) EraseColorMapping.Add("Diseased", Colors.Diseased);
            if (CustomGameOptions.ButtonBarryOn > 0) EraseColorMapping.Add("Button Barry", Colors.ButtonBarry);
            if (CustomGameOptions.LoversOn > 0) EraseColorMapping.Add("Lover", Colors.Lovers);
            if (CustomGameOptions.SleuthOn > 0) EraseColorMapping.Add("Sleuth", Colors.Sleuth);
            if (CustomGameOptions.TiebreakerOn > 0) EraseColorMapping.Add("Tiebreaker", Colors.Tiebreaker);
            if (CustomGameOptions.TorchOn > 0) EraseColorMapping.Add("Torch", Colors.Torch);
            if (CustomGameOptions.DrunkOn > 0) EraseColorMapping.Add("Drunk", Colors.Drunk);

            EraseSortedColorMapping = EraseColorMapping.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public bool GuessedThisMeeting { get; set; } = false;

        public int RemainingErases { get; set; }

        public List<string> PossibleModGuesses => EraseSortedColorMapping.Keys.ToList();
    }
}
