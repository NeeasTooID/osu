// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Skinning;

namespace osu.Game.Configuration
{
    [ExcludeFromDynamicCompile]
    public class OsuConfigManager : IniConfigManager<OsuSetting>
    {
        protected override void InitialiseDefaults()
        {
            // UI/selection defaults
            SetDefault(OsuSetting.Ruleset, string.Empty);
            SetDefault(OsuSetting.Skin, SkinInfo.DEFAULT_SKIN.ToString());

            SetDefault(OsuSetting.BeatmapDetailTab, PlayBeatmapDetailArea.TabType.Details);
            SetDefault(OsuSetting.BeatmapDetailModsFilter, false);

            SetDefault(OsuSetting.ShowConvertedBeatmaps, true);
            SetDefault(OsuSetting.DisplayStarsMinimum, 0.0, 0, 10, 0.1);
            SetDefault(OsuSetting.DisplayStarsMaximum, 10.1, 0, 10.1, 0.1);

            SetDefault(OsuSetting.SongSelectGroupingMode, GroupMode.All);
            SetDefault(OsuSetting.SongSelectSortingMode, SortMode.Title);

            SetDefault(OsuSetting.RandomSelectAlgorithm, RandomSelectAlgorithm.RandomPermutation);

            SetDefault(OsuSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2f, 1f);

            SetDefault(OsuSetting.BeatmapListingCardSize, BeatmapCardSize.Normal);

            SetDefault(OsuSetting.ToolbarClockDisplayMode, ToolbarClockDisplayMode.Full);

            // Online settings
            SetDefault(OsuSetting.Username, string.Empty);
            SetDefault(OsuSetting.Token, string.Empty);

            SetDefault(OsuSetting.AutomaticallyDownloadWhenSpectating, false);

            SetDefault(OsuSetting.SavePassword, false).ValueChanged += enabled =>
            {
                if (enabled.NewValue) SetValue(OsuSetting.SaveUsername, true);
            };

            SetDefault(OsuSetting.SaveUsername, true).ValueChanged += enabled =>
            {
                if (!enabled.NewValue) SetValue(OsuSetting.SavePassword, false);
            };

            SetDefault(OsuSetting.ExternalLinkWarning, true);
            SetDefault(OsuSetting.PreferNoVideo, false);

            SetDefault(OsuSetting.ShowOnlineExplicitContent, false);

            SetDefault(OsuSetting.NotifyOnUsernameMentioned, true);
            SetDefault(OsuSetting.NotifyOnPrivateMessage, true);

            // Audio
            SetDefault(OsuSetting.VolumeInactive, 0.25, 0, 1, 0.01);

            SetDefault(OsuSetting.MenuVoice, true);
            SetDefault(OsuSetting.MenuMusic, true);

            SetDefault(OsuSetting.AudioOffset, 0, -500.0, 500.0, 1);

            // Input
            SetDefault(OsuSetting.MenuCursorSize, 1.0f, 0.5f, 2f, 0.01f);
            SetDefault(OsuSetting.GameplayCursorSize, 1.0f, 0.1f, 2f, 0.01f);
            SetDefault(OsuSetting.AutoCursorSize, false);

            SetDefault(OsuSetting.MouseDisableButtons, false);
            SetDefault(OsuSetting.MouseDisableWheel, false);
            SetDefault(OsuSetting.ConfineMouseMode, OsuConfineMouseMode.DuringGameplay);

            // Graphics
            SetDefault(OsuSetting.ShowFpsDisplay, false);

            SetDefault(OsuSetting.ShowStoryboard, true);
            SetDefault(OsuSetting.BeatmapSkins, true);
            SetDefault(OsuSetting.BeatmapColours, true);
            SetDefault(OsuSetting.BeatmapHitsounds, true);

            SetDefault(OsuSetting.CursorRotation, true);

            SetDefault(OsuSetting.MenuParallax, true);

            // See https://stackoverflow.com/a/63307411 for default sourcing.
            SetDefault(OsuSetting.Prefer24HourTime, CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains(@"tt"));

            // Gameplay
            SetDefault(OsuSetting.PositionalHitsounds, true); // replaced by level setting below, can be removed 20220703.
            SetDefault(OsuSetting.PositionalHitsoundsLevel, 0.2f, 0, 1);
            SetDefault(OsuSetting.DimLevel, 0.8, 0, 1, 0.01);
            SetDefault(OsuSetting.BlurLevel, 0, 0, 1, 0.01);
            SetDefault(OsuSetting.LightenDuringBreaks, true);

            SetDefault(OsuSetting.HitLighting, true);

            SetDefault(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Always);
            SetDefault(OsuSetting.ShowProgressGraph, true);
            SetDefault(OsuSetting.ShowHealthDisplayWhenCantFail, true);
            SetDefault(OsuSetting.FadePlayfieldWhenHealthLow, true);
            SetDefault(OsuSetting.KeyOverlay, false);
            SetDefault(OsuSetting.AlwaysPlayFirstComboBreak, true);

            SetDefault(OsuSetting.FloatingComments, false);

            SetDefault(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised);

            SetDefault(OsuSetting.IncreaseFirstObjectVisibility, true);
            SetDefault(OsuSetting.GameplayDisableWinKey, true);

            // Update
            SetDefault(OsuSetting.ReleaseStream, ReleaseStream.Lazer);

            SetDefault(OsuSetting.Version, string.Empty);

            SetDefault(OsuSetting.ShowFirstRunSetup, true);

            SetDefault(OsuSetting.ScreenshotFormat, ScreenshotFormat.Jpg);
            SetDefault(OsuSetting.ScreenshotCaptureMenuCursor, false);

            SetDefault(OsuSetting.SongSelectRightMouseScroll, false);

            SetDefault(OsuSetting.Scaling, ScalingMode.Off);

            SetDefault(OsuSetting.ScalingSizeX, 0.8f, 0.2f, 1f);
            SetDefault(OsuSetting.ScalingSizeY, 0.8f, 0.2f, 1f);

            SetDefault(OsuSetting.ScalingPositionX, 0.5f, 0f, 1f);
            SetDefault(OsuSetting.ScalingPositionY, 0.5f, 0f, 1f);

            SetDefault(OsuSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f);

            SetDefault(OsuSetting.UIHoldActivationDelay, 200.0, 0.0, 500.0, 50.0);

            SetDefault(OsuSetting.IntroSequence, IntroSequence.Triangles);

            SetDefault(OsuSetting.MenuBackgroundSource, BackgroundSource.Skin);
            SetDefault(OsuSetting.SeasonalBackgroundMode, SeasonalBackgroundMode.Sometimes);

            SetDefault(OsuSetting.DiscordRichPresence, DiscordRichPresenceMode.Full);

            SetDefault(OsuSetting.EditorWaveformOpacity, 0.25f);
        }

        public IDictionary<OsuSetting, string> GetLoggableState() =>
            new Dictionary<OsuSetting, string>(ConfigStore.Where(kvp => !keyContainsPrivateInformation(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()));

        private static bool keyContainsPrivateInformation(OsuSetting argKey)
        {
            switch (argKey)
            {
                case OsuSetting.Token:
                    return true;
            }

            return false;
        }

        public OsuConfigManager(Storage storage)
            : base(storage)
        {
            Migrate();
        }

        public void Migrate()
        {
            // arrives as 2020.123.0
            string rawVersion = Get<string>(OsuSetting.Version);

            if (rawVersion.Length < 6)
                return;

            string[] pieces = rawVersion.Split('.');

            // on a fresh install or when coming from a non-release build, execution will end here.
            // we don't want to run migrations in such cases.
            if (!int.TryParse(pieces[0], out int year)) return;
            if (!int.TryParse(pieces[1], out int monthDay)) return;

            int combined = (year * 10000) + monthDay;

            if (combined < 20220103)
            {
                var positionalHitsoundsEnabled = GetBindable<bool>(OsuSetting.PositionalHitsounds);
                if (!positionalHitsoundsEnabled.Value)
                    SetValue(OsuSetting.PositionalHitsoundsLevel, 0);
            }
        }

        public override TrackedSettings CreateTrackedSettings()
        {
            // these need to be assigned in normal game startup scenarios.
            Debug.Assert(LookupKeyBindings != null);
            Debug.Assert(LookupSkinName != null);

            return new TrackedSettings
            {
                new TrackedSetting<bool>(OsuSetting.MouseDisableButtons, disabledState => new SettingDescription(
                    rawValue: !disabledState,
                    name: GlobalActionKeyBindingStrings.ToggleGameplayMouseButtons,
                    value: disabledState ? CommonStrings.Disabled.ToLower() : CommonStrings.Enabled.ToLower(),
                    shortcut: LookupKeyBindings(GlobalAction.ToggleGameplayMouseButtons))
                ),
                new TrackedSetting<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode, visibilityMode => new SettingDescription(
                    rawValue: visibilityMode,
                    name: GameplaySettingsStrings.HUDVisibilityMode,
                    value: visibilityMode.GetLocalisableDescription(),
                    shortcut: new TranslatableString(@"_", @"{0}: {1} {2}: {3}",
                        GlobalActionKeyBindingStrings.ToggleInGameInterface,
                        LookupKeyBindings(GlobalAction.ToggleInGameInterface),
                        GlobalActionKeyBindingStrings.HoldForHUD,
                        LookupKeyBindings(GlobalAction.HoldForHUD)))
                ),
                new TrackedSetting<ScalingMode>(OsuSetting.Scaling, scalingMode => new SettingDescription(
                        rawValue: scalingMode,
                        name: GraphicsSettingsStrings.ScreenScaling,
                        value: scalingMode.GetLocalisableDescription()
                    )
                ),
                new TrackedSetting<string>(OsuSetting.Skin, skin =>
                {
                    string skinName = string.Empty;

                    if (Guid.TryParse(skin, out var id))
                        skinName = LookupSkinName(id) ?? string.Empty;

                    return new SettingDescription(
                        rawValue: skinName,
                        name: SkinSettingsStrings.SkinSectionHeader,
                        value: skinName,
                        shortcut: new TranslatableString(@"_", @"{0}: {1}",
                            GlobalActionKeyBindingStrings.RandomSkin,
                            LookupKeyBindings(GlobalAction.RandomSkin))
                    );
                }),
                new TrackedSetting<float>(OsuSetting.UIScale, scale => new SettingDescription(
                        rawValue: scale,
                        name: GraphicsSettingsStrings.UIScaling,
                        value: $"{scale:N2}x"
                        // TODO: implement lookup for framework platform key bindings
                    )
                ),
            };
        }

        public Func<Guid, string> LookupSkinName { private get; set; } = _ => @"unknown";

        public Func<GlobalAction, LocalisableString> LookupKeyBindings { get; set; } = _ => @"unknown";
    }

    // IMPORTANT: These are used in user configuration files.
    // The naming of these keys should not be changed once they are deployed in a release, unless migration logic is also added.
    public enum OsuSetting
    {
        Ruleset,
        Token,
        MenuCursorSize,
        GameplayCursorSize,
        AutoCursorSize,
        DimLevel,
        BlurLevel,
        LightenDuringBreaks,
        ShowStoryboard,
        KeyOverlay,
        PositionalHitsounds,
        PositionalHitsoundsLevel,
        AlwaysPlayFirstComboBreak,
        FloatingComments,
        HUDVisibilityMode,

        // This has been migrated to the component itself. can be removed 20221027.
        ShowProgressGraph,
        ShowHealthDisplayWhenCantFail,
        FadePlayfieldWhenHealthLow,
        MouseDisableButtons,
        MouseDisableWheel,
        ConfineMouseMode,

        /// <summary>
        /// Globally applied audio offset.
        /// This is added to the audio track's current time. Higher values will cause gameplay to occur earlier, relative to the audio track.
        /// </summary>
        AudioOffset,

        VolumeInactive,
        MenuMusic,
        MenuVoice,
        CursorRotation,
        MenuParallax,
        Prefer24HourTime,
        BeatmapDetailTab,
        BeatmapDetailModsFilter,
        Username,
        ReleaseStream,
        SavePassword,
        SaveUsername,
        DisplayStarsMinimum,
        DisplayStarsMaximum,
        SongSelectGroupingMode,
        SongSelectSortingMode,
        RandomSelectAlgorithm,
        ShowFpsDisplay,
        ChatDisplayHeight,
        BeatmapListingCardSize,
        ToolbarClockDisplayMode,
        Version,
        ShowFirstRunSetup,
        ShowConvertedBeatmaps,
        Skin,
        ScreenshotFormat,
        ScreenshotCaptureMenuCursor,
        SongSelectRightMouseScroll,
        BeatmapSkins,
        BeatmapColours,
        BeatmapHitsounds,
        IncreaseFirstObjectVisibility,
        ScoreDisplayMode,
        ExternalLinkWarning,
        PreferNoVideo,
        Scaling,
        ScalingPositionX,
        ScalingPositionY,
        ScalingSizeX,
        ScalingSizeY,
        UIScale,
        IntroSequence,
        NotifyOnUsernameMentioned,
        NotifyOnPrivateMessage,
        UIHoldActivationDelay,
        HitLighting,
        MenuBackgroundSource,
        GameplayDisableWinKey,
        SeasonalBackgroundMode,
        EditorWaveformOpacity,
        DiscordRichPresence,
        AutomaticallyDownloadWhenSpectating,
        ShowOnlineExplicitContent,
    }
}
