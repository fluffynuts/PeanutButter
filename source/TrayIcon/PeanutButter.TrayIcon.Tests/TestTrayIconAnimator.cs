using System.Drawing;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestTrayIconAnimator
    {
        [Test]
        public void Busy_GivenNoText_ShouldSetTextToDefault()
        {
            //--------------- Arrange -------------------
            var trayIcon = new TrayIcon(Resource1.Happy_smiley_face);
            var sut = Create(trayIcon);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Busy();

            //--------------- Assert -----------------------
            Expect(trayIcon.DefaultTipText).To.Equal("busy...");
        }

        [Test]
        public void Busy_GivenText_ShouldSetText()
        {
            //--------------- Arrange -------------------
            var trayIcon = new TrayIcon(Resource1.Happy_smiley_face);
            var expected = GetRandomString();
            var sut = Create(trayIcon);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Busy(expected);

            //--------------- Assert -----------------------
            Expect(trayIcon.DefaultTipText).To.Equal(expected);
        }

        [Test]
        public void Rest_GivenText_ShouldSetText()
        {
            //--------------- Arrange -------------------
            var trayIcon = new TrayIcon(Resource1.Happy_smiley_face);
            var expected = GetRandomString();
            var sut = Create(trayIcon);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Busy();
            sut.Rest(expected);

            //--------------- Assert -----------------------
            Expect(trayIcon.DefaultTipText).To.Equal(expected);
        }


        [Test]
        public void Rest_GivenNoText_ShouldSetTextToLastText()
        {
            //--------------- Arrange -------------------
            var trayIcon = new TrayIcon(Resource1.Happy_smiley_face);
            var expected = GetRandomString();
            trayIcon.DefaultTipText = expected;
            var sut = Create(trayIcon);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Rest(expected);
            sut.Busy();
            sut.Rest();

            //--------------- Assert -----------------------
            Expect(trayIcon.DefaultTipText).To.Equal(expected);
        }

        private TrayIconAnimator Create(TrayIcon trayIcon)
        {
            var ico = Icon.FromHandle(Resource1.Happy_smiley_face.GetHicon());
            return new TrayIconAnimator(
                trayIcon,
                ico,
                ico,
                ico
            );
        }
    }
}