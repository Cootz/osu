﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyAttributes : DifficultyAttributes
    {
        /// <summary>
        /// The difficulty corresponding to the rhythm skill.
        /// </summary>
        [JsonProperty("rhythm_difficulty")]
        public double RhythmDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the reading skill.
        /// </summary>
        [JsonProperty("reading_difficulty")]
        public double ReadingDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the colour skill.
        /// </summary>
        [JsonProperty("colour_difficulty")]
        public double ColourDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the stamina skill.
        /// </summary>
        [JsonProperty("stamina_difficulty")]
        public double StaminaDifficulty { get; set; }

        /// <summary>
        /// The ratio of stamina difficulty from mono-color (single colour) streams to total stamina difficulty.
        /// </summary>
        [JsonProperty("mono_stamina_factor")]
        public double MonoStaminaFactor { get; set; }

        [JsonProperty("rhythm_difficult_strains")]
        public double RhythmTopStrains { get; set; }

        [JsonProperty("colour_difficult_strains")]
        public double ColourTopStrains { get; set; }

        [JsonProperty("stamina_difficult_strains")]
        public double StaminaTopStrains { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (ATTRIB_ID_DIFFICULTY, StarRating);
            yield return (ATTRIB_ID_MONO_STAMINA_FACTOR, MonoStaminaFactor);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            base.FromDatabaseAttributes(values, onlineInfo);

            StarRating = values[ATTRIB_ID_DIFFICULTY];
            MonoStaminaFactor = values[ATTRIB_ID_MONO_STAMINA_FACTOR];
        }
    }
}
