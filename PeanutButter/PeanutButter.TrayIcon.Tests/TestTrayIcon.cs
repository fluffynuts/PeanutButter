using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestTrayIcon
    {
        [Test]
        [STAThread]
        [Ignore("Should be run manually; this is an interactive UI test")]
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
                        trayIcon.ShowBalloonTipFor(500, "Mouse Click Handler", string.Format("Mouse click handler: {0} / {1}", mc, button), ToolTipIcon.Info);
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

    }
}
