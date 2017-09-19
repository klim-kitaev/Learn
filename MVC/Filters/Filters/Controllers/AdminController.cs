using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Filters.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        [Authorize(Users ="admin")]
        public ActionResult Index()
        {
            return View();
        }
    }
}