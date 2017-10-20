using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
// ReSharper disable LocalizableElement

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestTrayIcon
    {
        [Test]
        [STAThread]
        [Explicit("Should be run manually; this is an interactive UI test")]
        public void AcceptanceTest_NotToBeRunAutomatically()
        {
            //---------------Set up test pack-------------------
            var trayIcon = new TrayIcon();
            trayIcon.Init(Resource1.Happy_smiley_face);
            trayIcon.DefaultBalloonTipTimeout = 5000;
            trayIcon.DefaultTipText = "Default tip text";
            trayIcon.DefaultTipTitle = "Default tip title";
            trayIcon.DefaultBalloonTipClickedAction = () => MessageBox.Show("You clicked the default balloon tip");
            trayIcon.AddMenuItem("MessageBox", () => MessageBox.Show("Hello"));
            trayIcon.AddMenuSeparator();
            trayIcon.AddMenuItem("Balloon tip", () =>
            {
                trayIcon.ShowBalloonTipFor(5000, "Current time",
                    "The current time is: " + DateTime.Now.ToString("HH:mm:ss"), ToolTipIcon.Info,
                    () => MessageBox.Show("You clicked the balloon!"),
                    () => MessageBox.Show("You closed the balloon ):"));
            });
            var sub = trayIcon.AddSubMenu("Sub Menu");
            trayIcon.AddMenuItem("Item 1", () => { }, sub);
            trayIcon.AddMenuItem("Destroy Sub", () =>
            {
                trayIcon.RemoveMenuItem("Sub Menu");
            }, sub);

            var buttons = new[] {MouseButtons.Left, MouseButtons.Right, MouseButtons.Middle};
            var handlers = new List<MouseClickHandler>();

            new[] {MouseClicks.Single, MouseClicks.Double}.ForEach(mc =>
            {
                buttons.ForEach(button =>
                {
                    handlers.Add(trayIcon.AddMouseClickHandler(mc, button, () =>
                    {
                        trayIcon.ShowBalloonTipFor(
                            500, 
                            "Mouse Click Handler", 
                            $"Mouse click handler: {mc} / {button}", 
                            ToolTipIcon.Info);
                    }));
                });
            });

            trayIcon.AddMenuItem("Remove all mouse click handlers", () =>
            {
                handlers.ForEach(trayIcon.RemoveMouseClickHandler);
            });

            trayIcon.AddMenuItem("Exit", () =>
            {
                trayIcon.Hide();
                Application.Exit();
            });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            trayIcon.Show();

            //---------------Test Result -----------------------
            Application.Run();
        }

        [Test]
        public void ShouldExposeInternalNotificationIcon()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var icon = new TrayIcon(Resource1.Happy_smiley_face);
            Assert.IsNotNull(icon.NotifyIcon);

            //---------------Test Result -----------------------
        }

        private TrayIcon Create()
        {
            return new TrayIcon(Resource1.Happy_smiley_face);
        }

        [Test]
        public void AddMouseClickHandler_ShouldAddHandler()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var clicks = GetRandom<MouseClicks>();
            var button = GetRandom<MouseButtons>();
            var called = false;
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.AddMouseClickHandler(clicks, button, () => called = true);
            var result = sut.MouseClickHandlers.First();

            //--------------- Assert -----------------------
            Expect(result.Clicks).To.Equal(clicks);
            Expect(result.Button).To.Equal(button);
            result.Action();
            Expect(called).To.Be.True();
        }

        [Test]
        public void RemoveMouseClickHandler_ShouldRemoveHandler()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var clicks = GetRandom<MouseClicks>();
            var button = GetRandom<MouseButtons>();
            var handler = sut.AddMouseClickHandler(clicks, button, () => { });
            //--------------- Assume ----------------
            Expect(sut.MouseClickHandlers).Not.To.Be.Empty();

            //--------------- Act ----------------------
            sut.RemoveMouseClickHandler(handler);

            //--------------- Assert -----------------------
            Expect(sut.MouseClickHandlers).To.Be.Empty();
        }
        [Test]
        public void AddMenuItem_ShouldAddHandler()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var text = GetRandomString();
            var called = false;
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.AddMenuItem(text, () => called = true);
            var result = sut.NotifyIcon.ContextMenu.MenuItems[0];

            //--------------- Assert -----------------------
            Expect(result.Text).To.Equal(text);
            result.PerformClick();
            Expect(called).To.Be.True();
        }

        [Test]
        public void RemoveMenuItem_ShouldRemoveHandler()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var text = GetRandomString();
            sut.AddMenuItem(text, () => { });
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.RemoveMenuItem(text);

            //--------------- Assert -----------------------
            Expect(sut.NotifyIcon.ContextMenu.MenuItems.Count).To.Equal(0);
        }

        [Test]
        public void DefaultTipText_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomString();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.DefaultTipText = expected;

            //--------------- Assert -----------------------
            Expect(sut.DefaultTipText).To.Equal(expected);
        }

        [Test]
        public void DefaultTipTitle_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomString();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.DefaultTipTitle = expected;

            //--------------- Assert -----------------------
            Expect(sut.DefaultTipTitle).To.Equal(expected);
        }

        [Test]
        public void DefaultBalloonTipClicked_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var expected = new Action(() => { });
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.DefaultBalloonTipClickedAction = expected;

            //--------------- Assert -----------------------
            Expect(sut.DefaultBalloonTipClickedAction).To.Equal(expected);
        }

        [Test]
        public void DefaultBalloonTipClosed_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var expected = new Action(() => { });
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.DefaultBalloonTipClosedAction = expected;

            //--------------- Assert -----------------------
            Expect(sut.DefaultBalloonTipClosedAction).To.Equal(expected);
        }

        [Test]
        public void AddMenuSeparator_ShouldAddMagickSeparator()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.AddMenuSeparator();

            //--------------- Assert -----------------------
            Expect(sut.NotifyIcon.ContextMenu.MenuItems[0].Text)
                .To.Equal("-");
        }

        [Test]
        public void ShowDefaultBalloonTip_ShouldNotThrowWithNullArgs()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var sub = Substitute.For<INotifyIcon>();
            sut.DefaultTipText = GetRandomString(5);
            sut.DefaultTipTitle = GetRandomString(5);
            sut.NotificationIcon = sub;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShowDefaultBalloonTip(null, null);
            sut.BalloonTipClickHandlers.ClosedAction();

            //--------------- Assert -----------------------
            Expect(sut.ShowingDefaultBaloonTip).To.Be.False();
            Expect(sut.BalloonTipClickHandlers).To.Be.Null();
        }

        [Test]
        public void ShowBalloonTipFor_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var timeout = GetRandomInt();
            var title = GetRandomString();
            var text = GetRandomString();
            var ico = GetRandom<ToolTipIcon>();
            var sub = Substitute.For<INotifyIcon>();
            sut.NotificationIcon = sub;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShowBalloonTipFor(
                timeout,
                title,
                text,
                ico,
                () => { },
                () => { }
            );


            //--------------- Assert -----------------------
            sub.Received(1).ShowBalloonTip(
                timeout,
                title,
                text,
                ico
            );
        }

        [Test]
        public void Hide_ShouldSetVisibilityOff()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var sub = Substitute.For<INotifyIcon>();
            sub.Visible = true;
            sut.NotificationIcon = sub;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Hide();


            //--------------- Assert -----------------------
            Expect(sub.Visible).To.Be.False();
        }

        [Test]
        public void Show_ShouldSetVisibilityOn()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var sub = Substitute.For<INotifyIcon>();
            var actual = new NotifyIcon();
            sub.Actual.Returns(actual);
            sub.Visible = false;
            sut.NotificationIcon = sub;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Show();

            //--------------- Assert -----------------------
            Expect(sub.Visible).To.Be.True();
        }

        [Test]
        public void Dispose_ShouldDisposeTheNotificationIcon()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var sub = Substitute.For<INotifyIcon>();
            sut.NotificationIcon = sub;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Dispose();

            //--------------- Assert -----------------------
            sub.Received(1).Dispose();
        }

        [TestCase(MouseButtons.Left)]
        [TestCase(MouseButtons.Right)]
        public void OnMouseClick_ShouldRunMouseClickHandlers(MouseButtons forButton)
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var clickCalled = false;
            var doubleClickCalled = false;
            sut.AddMouseClickHandler(
                MouseClicks.Single,
                forButton, 
                () => clickCalled = true
            );
            sut.AddMouseClickHandler(
                MouseClicks.Double,
                forButton,
                () => doubleClickCalled = true
            );

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.OnIconMouseClick(null, new MouseEventArgs(forButton, 1, 0, 0, 0));

            //--------------- Assert -----------------------
            Expect(clickCalled).To.Be.True();
            Expect(doubleClickCalled).To.Be.False();
        }

        [TestCase(MouseButtons.Left)]
        [TestCase(MouseButtons.Right)]
        public void OnMouseDoubleClick_ShouldRunMouseClickHandlers(MouseButtons forButton)
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var clickCalled = false;
            var doubleClickCalled = false;
            sut.AddMouseClickHandler(
                MouseClicks.Single,
                forButton, 
                () => clickCalled = true
            );
            sut.AddMouseClickHandler(
                MouseClicks.Double,
                forButton,
                () => doubleClickCalled = true
            );

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.OnIconMouseDoubleClick(null, new MouseEventArgs(forButton, 1, 0, 0, 0));

            //--------------- Assert -----------------------
            Expect(clickCalled).To.Be.False();
            Expect(doubleClickCalled).To.Be.True();
        }

    }
}
