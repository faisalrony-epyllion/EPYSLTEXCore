using System.Web.Mvc;
using System.Web.UI;

namespace EPYSLTEX.Web.Controllers
{
    public class CacheController : Controller
    {
        [OutputCache(Duration = 2592000, Location = OutputCacheLocation.Client, VaryByParam = "none")]
        public ActionResult CacheEj2MinJS()
        {
            return File("~/Content/lib/syncfusion/ej2.min.js", "text/javascript");
        }
    }
}