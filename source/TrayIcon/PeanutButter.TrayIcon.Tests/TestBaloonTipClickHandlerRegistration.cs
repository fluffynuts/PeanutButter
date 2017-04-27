using NUnit.Framework;

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestBalloonTipClickHandlerRegistration
    {
        [Test]
        public void Construct_ShouldCopyParametersToProperties()
        {
            //---------------Set up test pack-------------------
            var clickActionCalled = false;
            var closeActionCalled = false;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new BalloonTipClickHandlerRegistration(() => clickActionCalled = true, () => closeActionCalled = true);
            sut.ClickAction();
            sut.ClosedAction();

            //---------------Test Result -----------------------
            Assert.IsTrue(clickActionCalled);
            Assert.IsTrue(closeActionCalled);
        }

    }
}
