// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;

namespace osu.Desktop.LegacyIpc
{
    /// <summary>
    /// Provides IPC to legacy osu! clients.
    /// </summary>
    public class LegacyTcpIpcProvider : TcpIpcProvider
    {
        private static readonly Logger logger = Logger.GetLogger("legacy-ipc");

        /// <summary>
        /// Invoked when a message is received from a legacy client.
        /// </summary>
        public new Func<object, object> MessageReceived;

        public LegacyTcpIpcProvider()
            : base(45357)
        {
            base.MessageReceived += msg =>
            {
                try
                {
                    logger.Add($"Processing legacy IPC message...");
                    logger.Add($"\t{msg.Value}", LogLevel.Debug);

                    var legacyData = ((JObject)msg.Value).ToObject<LegacyIpcMessage.Data>();
                    object value = parseObject((JObject)legacyData!.MessageData, legacyData.MessageType);

                    object result = onLegacyIpcMessageReceived(value);
                    return result != null ? new LegacyIpcMessage { Value = result } : null;
                }
                catch (Exception ex)
                {
                    logger.Add($"Processing IPC message failed: {msg.Value}", exception: ex);
                    return null;
                }
            };
        }

        private object parseObject(JObject value, string type)
        {
            switch (type)
            {
                case nameof(LegacyIpcDifficultyCalculationRequest):
                    return value.ToObject<LegacyIpcDifficultyCalculationRequest>();

                case nameof(LegacyIpcDifficultyCalculationResponse):
                    return value.ToObject<LegacyIpcDifficultyCalculationResponse>();

                default:
                    throw new ArgumentException($"Unknown type: {type}");
            }
        }

        private object onLegacyIpcMessageReceived(object message)
        {
            switch (message)
            {
                case LegacyIpcDifficultyCalculationRequest req:
                    try
                    {
                        Ruleset ruleset = req.RulesetId switch
                        {
                            0 => new OsuRuleset(),
                            1 => new TaikoRuleset(),
                            2 => new CatchRuleset(),
                            3 => new ManiaRuleset(),
                            _ => throw new ArgumentException("Invalid ruleset id")
                        };

                        Mod[] mods = ruleset.ConvertFromLegacyMods((LegacyMods)req.Mods).ToArray();
                        WorkingBeatmap beatmap = new FlatFileWorkingBeatmap(req.BeatmapFile, _ => ruleset);

                        return new LegacyIpcDifficultyCalculationResponse
                        {
                            StarRating = ruleset.CreateDifficultyCalculator(beatmap).Calculate(mods).StarRating
                        };
                    }
                    catch
                    {
                        return new LegacyIpcDifficultyCalculationResponse();
                    }
            }

            Console.WriteLine("Type not matched.");
            return null;
        }
    }
}
