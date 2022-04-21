using System;
using HarmonyLib;
using Reactor.Extensions;
using TMPro;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.AssassinMod;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.EraserMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class AddButton
    {
        private static Sprite CycleBackSprite => TownOfUs.CycleBackSprite;
        private static Sprite CycleForwardSprite => TownOfUs.CycleForwardSprite;

        private static Sprite GuessSprite => TownOfUs.GuessSprite;


        private static bool IsExempt(PlayerVoteArea voteArea)
        {
            if (voteArea.AmDead) return true;
            var player = Utils.PlayerById(voteArea.TargetPlayerId);
            if (
                player == null ||
                player.Data.IsImpostor() ||
                player.Data.IsDead ||
                player.Data.Disconnected
            ) return true;
            var role = Role.GetRole(player);
            return role != null && role.Criteria();
        }


        public static void GenButton(Eraser role, PlayerVoteArea voteArea)
        {
            var targetId = voteArea.TargetPlayerId;
            if (IsExempt(voteArea))
            {
                role.Buttons[targetId] = (null, null, null, null);
                return;
            }

            var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;
            var parent = confirmButton.transform.parent.parent;
            
            var nameText = Object.Instantiate(voteArea.NameText, voteArea.transform);
            voteArea.NameText.transform.localPosition = new Vector3(0.55f, 0.12f, -0.1f);
            nameText.transform.localPosition = new Vector3(0.55f, -0.12f, -0.1f);
            nameText.text = "Guess";

            var cycleBack = Object.Instantiate(confirmButton, voteArea.transform);
            var cycleRendererBack = cycleBack.GetComponent<SpriteRenderer>();
            cycleRendererBack.sprite = CycleBackSprite;
            cycleBack.transform.localPosition = new Vector3(-0.5f, 0.15f, -2f);
            cycleBack.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            cycleBack.layer = 5;
            cycleBack.transform.parent = parent;
            var cycleEventBack = new Button.ButtonClickedEvent();
            cycleEventBack.AddListener(Cycle(role, voteArea, nameText, false));
            cycleBack.GetComponent<PassiveButton>().OnClick = cycleEventBack;
            var cycleColliderBack = cycleBack.GetComponent<BoxCollider2D>();
            cycleColliderBack.size = cycleRendererBack.sprite.bounds.size;
            cycleColliderBack.offset = Vector2.zero;
            cycleBack.transform.GetChild(0).gameObject.Destroy();

            var cycleForward = Object.Instantiate(confirmButton, voteArea.transform);
            var cycleRendererForward = cycleForward.GetComponent<SpriteRenderer>();
            cycleRendererForward.sprite = CycleForwardSprite;
            cycleForward.transform.localPosition = new Vector3(-0.2f, 0.15f, -2f);
            cycleForward.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            cycleForward.layer = 5;
            cycleForward.transform.parent = parent;
            var cycleEventForward = new Button.ButtonClickedEvent();
            cycleEventForward.AddListener(Cycle(role, voteArea, nameText, true));
            cycleForward.GetComponent<PassiveButton>().OnClick = cycleEventForward;
            var cycleColliderForward = cycleForward.GetComponent<BoxCollider2D>();
            cycleColliderForward.size = cycleRendererForward.sprite.bounds.size;
            cycleColliderForward.offset = Vector2.zero;
            cycleForward.transform.GetChild(0).gameObject.Destroy();

            var guess = Object.Instantiate(confirmButton, voteArea.transform);
            var guessRenderer = guess.GetComponent<SpriteRenderer>();
            guessRenderer.sprite = GuessSprite;
            guess.transform.localPosition = new Vector3(-0.35f, -0.15f, -2f);
            guess.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            guess.layer = 5;
            guess.transform.parent = parent;
            var guessEvent = new Button.ButtonClickedEvent();
            guessEvent.AddListener(Guess(role, voteArea));
            guess.GetComponent<PassiveButton>().OnClick = guessEvent;
            var bounds = guess.GetComponent<SpriteRenderer>().bounds;
            bounds.size = new Vector3(0.52f, 0.3f, 0.16f);
            var guessCollider = guess.GetComponent<BoxCollider2D>();
            guessCollider.size = guessRenderer.sprite.bounds.size;
            guessCollider.offset = Vector2.zero;
            guess.transform.GetChild(0).gameObject.Destroy();

            
            role.Guesses.Add(targetId, "None");
            role.Buttons[targetId] = (cycleBack, cycleForward, guess, nameText);
            
            

            
        }


        private static Action Cycle(Eraser role, PlayerVoteArea voteArea, TextMeshPro nameText, bool forwardsCycle = true)
        {
            void Listener()
            {
                if (MeetingHud.Instance.state == MeetingHud.VoteStates.Discussion) return;
                var currentGuess = role.Guesses[voteArea.TargetPlayerId];
                    var guessIndex = currentGuess == "None"
                    ? -1
                    : role.PossibleModGuesses.IndexOf(currentGuess);
                    if (forwardsCycle)
                    {
                        if (++guessIndex >= role.PossibleModGuesses.Count)
                            guessIndex = 0;
                    }
                    else
                    {
                        if (--guessIndex < 0)
                            guessIndex = role.PossibleModGuesses.Count - 1;
                    }
                    var newGuess = role.Guesses[voteArea.TargetPlayerId] = role.PossibleModGuesses[guessIndex];

                    nameText.text = newGuess == "None"
                        ? "Guess"
                        : $"<color=#{role.EraseSortedColorMapping[newGuess].ToHtmlStringRGBA()}>{newGuess}</color>";
           
                
            }

            return Listener;
        }

        private static Action Guess(Eraser role, PlayerVoteArea voteArea)
        {
            void Listener()
            {
                if (
                    MeetingHud.Instance.state == MeetingHud.VoteStates.Discussion ||
                    IsExempt(voteArea) || PlayerControl.LocalPlayer.Data.IsDead
                ) return;
                var targetId = voteArea.TargetPlayerId;
                var currentGuess = role.Guesses[targetId];
                if (currentGuess == "None") return;

                var playerRole = Role.GetRole(voteArea);
                var playerModifier = Modifier.GetModifier(voteArea);

                    var Eraseable = false;
                    if (playerModifier != null)
                    {
                        Eraseable = playerModifier.Name == currentGuess;
                    }
                    role.RemainingErases--;
                    ShowHideButtons.HideSingle(role, targetId, false); // return to this

                    if (Eraseable)
                    {
                        AssassinKill.RpcErasePlayer(playerModifier.Player);
                        if (playerModifier.Player.IsLover())
                        {
                            var lover = ((Lover)playerModifier).OtherLover.Player;
                            ShowHideButtons.HideSingle(role, lover.PlayerId, false);
                        }
                    }
                
            }

            return Listener;
        }

        public static void Postfix(MeetingHud __instance)
        {
            foreach (var role in Ability.GetAbilities(AbilityEnum.Assassin))
            {
                var eraser = (Eraser) role;
                eraser.Guesses.Clear();
                eraser.Buttons.Clear();
                eraser.GuessedThisMeeting = false;
            }

            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            if (!PlayerControl.LocalPlayer.Is(AbilityEnum.Assassin)) return;

            var eraserRole = Ability.GetAbility<Eraser>(PlayerControl.LocalPlayer);
            if (eraserRole.RemainingErases <= 0) return;
            foreach (var voteArea in __instance.playerStates)
            {
                GenButton(eraserRole, voteArea);
            }
        }
    }
}
