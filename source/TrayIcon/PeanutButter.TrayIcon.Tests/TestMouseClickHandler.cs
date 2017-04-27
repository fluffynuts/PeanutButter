using System;
using System.Windows.Forms;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestMouseClickHandler: AssertionHelper
    {
        [Test]
        public void Construct_ShouldCopyParameters_ToProperties()
        {
            //--------------- Arrange -------------------
            var clicks = GetRandomEnum<MouseClicks>();
            var button = GetRandomEnum<MouseButtons>();
            var called = false;
            Action action = () => called = true;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = new MouseClickHandler(clicks, button, action);

            //--------------- Assert -----------------------
            Expect(sut.Clicks, Is.EqualTo(clicks));
            Expect(sut.Button, Is.EqualTo(button));
            Expect(sut.Action, Is.EqualTo(action));
            sut.Action();
            Expect(called, Is.True);
        }

    }
}
