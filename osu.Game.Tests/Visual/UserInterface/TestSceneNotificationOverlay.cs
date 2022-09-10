﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneNotificationOverlay : OsuManualInputManagerTestScene
    {
        private NotificationOverlay notificationOverlay = null!;

        private readonly List<ProgressNotification> progressingNotifications = new List<ProgressNotification>();

        private SpriteText displayedCount = null!;

        public double TimeToCompleteProgress { get; set; } = 2000;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            TimeToCompleteProgress = 2000;
            progressingNotifications.Clear();

            Children = new Drawable[]
            {
                notificationOverlay = new NotificationOverlay
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                },
                displayedCount = new OsuSpriteText()
            };

            notificationOverlay.UnreadCount.ValueChanged += count => { displayedCount.Text = $"displayed count: {count.NewValue}"; };
        });

        [Test]
        public void TestDismissWithoutActivationRightClick()
        {
            bool activated = false;
            SimpleNotification notification = null!;

            AddStep("post", () =>
            {
                activated = false;
                notificationOverlay.Post(notification = new SimpleNotification
                {
                    Text = @"Welcome to osu!. Enjoy your stay!",
                    Activated = () => activated = true,
                });
            });

            AddStep("click to activate", () =>
            {
                InputManager.MoveMouseTo(notificationOverlay.ChildrenOfType<Notification>().Single());
                InputManager.Click(MouseButton.Right);
            });

            AddUntilStep("wait for closed", () => notification.WasClosed);
            AddAssert("was not activated", () => !activated);
            AddStep("reset mouse position", () => InputManager.MoveMouseTo(Vector2.Zero));
        }

        [Test]
        public void TestActivate()
        {
            bool activated = false;
            SimpleNotification notification = null!;

            AddStep("post", () =>
            {
                activated = false;
                notificationOverlay.Post(notification = new SimpleNotification
                {
                    Text = @"Welcome to osu!. Enjoy your stay!",
                    Activated = () => activated = true,
                });
            });

            AddStep("click to activate", () =>
            {
                InputManager.MoveMouseTo(notificationOverlay.ChildrenOfType<Notification>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for closed", () => notification.WasClosed);
            AddAssert("was activated", () => activated);
            AddStep("reset mouse position", () => InputManager.MoveMouseTo(Vector2.Zero));
        }

        [Test]
        public void TestPresence()
        {
            AddAssert("tray not present", () => !notificationOverlay.ChildrenOfType<NotificationOverlayToastTray>().Single().IsPresent);
            AddAssert("overlay not present", () => !notificationOverlay.IsPresent);

            AddStep(@"post notification", sendBackgroundNotification);

            AddUntilStep("wait tray not present", () => !notificationOverlay.ChildrenOfType<NotificationOverlayToastTray>().Single().IsPresent);
            AddUntilStep("wait overlay not present", () => !notificationOverlay.IsPresent);
        }

        [Test]
        public void TestPresenceWithManualDismiss()
        {
            AddAssert("tray not present", () => !notificationOverlay.ChildrenOfType<NotificationOverlayToastTray>().Single().IsPresent);
            AddAssert("overlay not present", () => !notificationOverlay.IsPresent);

            AddStep(@"post notification", sendBackgroundNotification);
            AddStep("click notification", () => notificationOverlay.ChildrenOfType<Notification>().Single().TriggerClick());

            AddUntilStep("wait tray not present", () => !notificationOverlay.ChildrenOfType<NotificationOverlayToastTray>().Single().IsPresent);
            AddUntilStep("wait overlay not present", () => !notificationOverlay.IsPresent);
        }

        [Test]
        public void TestCompleteProgress()
        {
            ProgressNotification notification = null!;

            AddStep("add progress notification", () =>
            {
                notification = new ProgressNotification
                {
                    Text = @"Uploading to BSS...",
                    CompletionText = "Uploaded to BSS!",
                };
                notificationOverlay.Post(notification);
                progressingNotifications.Add(notification);
            });

            AddUntilStep("wait completion", () => notification.State == ProgressNotificationState.Completed);

            AddAssert("Completion toast shown", () => notificationOverlay.ToastCount == 1);
            AddUntilStep("wait forwarded", () => notificationOverlay.ToastCount == 0);
        }

        [Test]
        public void TestCompleteProgressSlow()
        {
            ProgressNotification notification = null!;

            AddStep("Set progress slow", () => TimeToCompleteProgress *= 2);
            AddStep("add progress notification", () =>
            {
                notification = new ProgressNotification
                {
                    Text = @"Uploading to BSS...",
                    CompletionText = "Uploaded to BSS!",
                };
                notificationOverlay.Post(notification);
                progressingNotifications.Add(notification);
            });

            AddUntilStep("wait completion", () => notification.State == ProgressNotificationState.Completed);

            AddAssert("Completion toast shown", () => notificationOverlay.ToastCount == 1);
        }

        [Test]
        public void TestCancelProgress()
        {
            ProgressNotification notification = null!;
            AddStep("add progress notification", () =>
            {
                notification = new ProgressNotification
                {
                    Text = @"Uploading to BSS...",
                    CompletionText = "Uploaded to BSS!",
                };
                notificationOverlay.Post(notification);
                progressingNotifications.Add(notification);
            });

            AddWaitStep("wait 3", 3);

            AddStep("cancel notification", () => notification.State = ProgressNotificationState.Cancelled);
        }

        [Test]
        public void TestBasicFlow()
        {
            setState(Visibility.Visible);
            AddStep(@"simple #1", sendHelloNotification);
            AddStep(@"simple #2", sendAmazingNotification);
            AddStep(@"progress #1", sendUploadProgress);
            AddStep(@"progress #2", sendDownloadProgress);

            checkProgressingCount(2);

            setState(Visibility.Hidden);

            AddRepeatStep(@"add many simple", sendManyNotifications, 3);

            waitForCompletion();

            AddStep(@"progress #3", sendUploadProgress);

            checkProgressingCount(1);

            checkDisplayedCount(33);

            waitForCompletion();
        }

        [Test]
        public void TestImportantWhileClosed()
        {
            AddStep(@"simple #1", sendHelloNotification);

            AddAssert("toast displayed", () => notificationOverlay.ToastCount == 1);
            AddAssert("is not visible", () => notificationOverlay.State.Value == Visibility.Hidden);

            checkDisplayedCount(1);

            AddStep(@"progress #1", sendUploadProgress);
            AddStep(@"progress #2", sendDownloadProgress);

            checkProgressingCount(2);
            checkDisplayedCount(3);
        }

        [Test]
        public void TestUnimportantWhileClosed()
        {
            AddStep(@"background #1", sendBackgroundNotification);

            AddAssert("Is not visible", () => notificationOverlay.State.Value == Visibility.Hidden);

            checkDisplayedCount(1);

            AddStep(@"background progress #1", sendBackgroundUploadProgress);

            checkProgressingCount(1);

            waitForCompletion();

            checkDisplayedCount(2);

            AddStep(@"simple #1", sendHelloNotification);

            checkDisplayedCount(3);
        }

        [Test]
        public void TestError()
        {
            setState(Visibility.Visible);
            AddStep(@"error #1", sendErrorNotification);
            AddAssert("Is visible", () => notificationOverlay.State.Value == Visibility.Visible);
            checkDisplayedCount(1);
        }

        [Test]
        public void TestSpam()
        {
            setState(Visibility.Visible);
            AddRepeatStep("send barrage", sendBarrage, 10);
        }

        protected override void Update()
        {
            base.Update();

            progressingNotifications.RemoveAll(n => n.State == ProgressNotificationState.Completed);

            if (progressingNotifications.Count(n => n.State == ProgressNotificationState.Active) < 3)
            {
                var p = progressingNotifications.Find(n => n.State == ProgressNotificationState.Queued);

                if (p != null)
                    p.State = ProgressNotificationState.Active;
            }

            foreach (var n in progressingNotifications.FindAll(n => n.State == ProgressNotificationState.Active))
            {
                if (n.Progress < 1)
                    n.Progress += (float)(Time.Elapsed / TimeToCompleteProgress);
                else
                    n.State = ProgressNotificationState.Completed;
            }
        }

        private void checkDisplayedCount(int expected) =>
            AddUntilStep($"Displayed count is {expected}", () => notificationOverlay.UnreadCount.Value == expected);

        private void sendDownloadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Downloading Haitai...",
                CompletionText = "Downloaded Haitai!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendUploadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Uploading to BSS...",
                CompletionText = "Uploaded to BSS!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendBackgroundUploadProgress()
        {
            var n = new BackgroundProgressNotification
            {
                Text = @"Uploading to BSS...",
                CompletionText = "Uploaded to BSS!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void setState(Visibility state) => AddStep(state.ToString(), () => notificationOverlay.State.Value = state);

        private void checkProgressingCount(int expected) => AddAssert($"progressing count is {expected}", () => progressingNotifications.Count == expected);

        private void waitForCompletion() => AddUntilStep("wait for notification progress completion", () => progressingNotifications.Count == 0);

        private void sendBarrage()
        {
            switch (RNG.Next(0, 5))
            {
                case 0:
                    sendHelloNotification();
                    break;

                case 1:
                    sendAmazingNotification();
                    break;

                case 2:
                    sendUploadProgress();
                    break;

                case 3:
                    sendDownloadProgress();
                    break;

                case 4:
                    sendErrorNotification();
                    break;
            }
        }

        private void sendAmazingNotification()
        {
            notificationOverlay.Post(new SimpleNotification { Text = @"You are amazing" });
        }

        private void sendHelloNotification()
        {
            notificationOverlay.Post(new SimpleNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }

        private void sendBackgroundNotification()
        {
            notificationOverlay.Post(new BackgroundNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }

        private void sendErrorNotification()
        {
            notificationOverlay.Post(new SimpleErrorNotification { Text = @"Rut roh!. Something went wrong!" });
        }

        private void sendManyNotifications()
        {
            for (int i = 0; i < 10; i++)
                notificationOverlay.Post(new SimpleNotification { Text = @"Spam incoming!!" });
        }

        private class BackgroundNotification : SimpleNotification
        {
            public override bool IsImportant => false;
        }

        private class BackgroundProgressNotification : ProgressNotification
        {
            public override bool IsImportant => false;
        }
    }
}
