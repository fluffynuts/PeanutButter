using System;
using System.Windows.Forms;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
// ReSharper disable ConvertToLocalFunction

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestMouseClickHandler
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
            Expect(sut.Clicks).To.Equal(clicks);
            Expect(sut.Button).To.Equal(button);
            Expect(sut.Action).To.Equal(action);
            sut.Action();
            Expect(called).To.Be.True();
        }

    }
}
