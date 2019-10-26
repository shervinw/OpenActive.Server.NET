using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookingSystem.AspNetFramework;
using BookingSystem.AspNetFramework.Controllers;

namespace BookingSystem.AspNetFramework.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            DatasetSiteController controller = new DatasetSiteController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}
