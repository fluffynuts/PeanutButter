using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestNotifyIconWrapper
    {
        [Test]
        public void Icon_ShouldPassThroughToActualIcon()
        {
            //--------------- Arrange -------------------
            var actual = new NotifyIcon();
            var icon = Icon.FromHandle(Resource1.Happy_smiley_face.GetHicon());
            var sut = Create(actual);
            sut.Icon = icon;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.Icon;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(icon);
        }

        [Test]
        public void Visible_ShouldPassThroughToActualVisible()
        {
            //--------------- Arrange -------------------
            var actual = new NotifyIcon();
            var expected = GetRandomBoolean();
            var sut = Create(actual);
            sut.Visible = expected;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.Visible;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void ContextMenu_ShouldPassThroughToActualContextMenu()
        {
            //--------------- Arrange -------------------
            var actual = new NotifyIcon();
            var expected = new ContextMenu();
            var sut = Create(actual);
            sut.ContextMenu = expected;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.ContextMenu;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void Dispose_ShouldCallThrough()
        {
            //--------------- Arrange -------------------
            var actual = new NotifyIcon();
            var called = false;
            actual.Disposed += (s, e) => called = true;
            var sut = Create(actual);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Dispose();

            //--------------- Assert -----------------------
            Expect(called).To.Be.True();
        }

        private NotifyIconWrapper Create(NotifyIcon actual)
        {
            return new NotifyIconWrapper(actual);
        }
    }
}