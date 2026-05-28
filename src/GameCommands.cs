using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace STS2LudicrousSpeed;

public static class GameCommands
{
    public static void StartRun(string characterName, string seed, int ascension)
    {
        var allChars = ModelDb.AllCharacters.ToList();
        var character = allChars.FirstOrDefault(c =>
            c.GetType().Name.Contains(characterName, StringComparison.OrdinalIgnoreCase));

        if (character == null)
        {
            HeadlessController.Emit(new
            {
                type = "error",
                message = $"Unknown character '{characterName}'",
                available_characters = allChars.Select(c => c.GetType().Name).ToArray()
            });
            return;
        }

        var acts = ActModel.GetDefaultList();
        var modifiers = new List<ModifierModel>();

        if (seed.Length == 0)
            seed = SeedHelper.GetRandomSeed();

        HeadlessController.Emit(new
        {
            type = "run_starting",
            character = character.GetType().Name,
            seed,
            acts = acts.Select(a => a.GetType().Name).ToArray()
        });

        NGame.Instance!
            .StartNewSingleplayerRun(
                character,
                shouldSave: false,
                acts,
                modifiers,
                seed: seed,
                gameMode: ResolveNormalGameMode(),
                ascensionLevel: ascension,
                dailyTime: null)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    HeadlessController.Emit(new
                    {
                        type = "error",
                        message = "Run failed to start: " + t.Exception?.GetBaseException().Message
                    });
                    return;
                }
                var state = t.Result;
                HeadlessController.Emit(new
                {
                    type = "run_started",
                    seed = state.Rng.StringSeed,
                    floor = state.ActFloor,
                    act = state.CurrentActIndex,
                    ascension = state.AscensionLevel,
                    players = state.Players.Select(p => new
                    {
                        character = p.Character.GetType().Name,
                        hp = p.Creature.CurrentHp,
                        max_hp = p.Creature.MaxHp,
                        gold = p.Gold,
                        deck_size = p.Deck.Cards.Count,
                        relic_count = p.Relics.Count
                    }).ToArray()
                });
            });
    }

    private static GameMode ResolveNormalGameMode()
    {
        foreach (GameMode v in Enum.GetValues(typeof(GameMode)))
            if (Enum.GetName(typeof(GameMode), v)?.IndexOf("normal", StringComparison.OrdinalIgnoreCase) >= 0)
                return v;
        return (GameMode)0;
    }
}
