using System.Web.Mvc;

namespace EPYSLTEXCore.Report.Controllers
{
    public class ReportsController : Controller
    {

   

        public ReportsController(
          )
        {
        }

        public ActionResult Index()
        {

            ViewBag.ProfilePic = "/images/user.png";
            ViewBag.EmployeeName = "Nishadur Rahman";
            return View();
        }



 


    }

}