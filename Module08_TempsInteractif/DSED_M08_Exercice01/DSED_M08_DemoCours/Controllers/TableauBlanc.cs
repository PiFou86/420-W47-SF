using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DSED_M08_DemoCours.Controllers
{
    public class TableauBlanc : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
