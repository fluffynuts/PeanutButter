using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NSubstitute;

namespace PeanutButter.TestUtils.MVC
{
    public class FakeHtmlHelper
    {
        /// <summary>
        /// Creates a faked HtmlHelper with substituted view and view data container
        /// </summary>
        /// <param name="viewData">The view data.</param>
        /// <returns></returns>
        public static HtmlHelper CreateHtmlHelper(ViewDataDictionary viewData)
        {
            var httpContextMock = Substitute.For<HttpContextBase>();

            var controllerContext = Substitute.For<ControllerContext>(
                httpContextMock,
                new RouteData(),
                Substitute.For<ControllerBase>());

            var viewContext = Substitute.For<ViewContext>(
                controllerContext,
                Substitute.For<IView>(),
                viewData,
                new TempDataDictionary(),
                new System.IO.StringWriter()
                );

            var mockViewDataContainer = Substitute.For<IViewDataContainer>();
            mockViewDataContainer.ViewData.Returns(viewData);

            return new HtmlHelper(viewContext, mockViewDataContainer);
        }
    }
}
