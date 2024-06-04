using Cloud7CMS.Models;
using Cloud7CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace Cloud7CMS.Controllers
{
    [Authorize(Roles = "Admin,FunBoxManager")]
    public class RobiController : Controller
    {
        private readonly ILogger<RobiController> _logger;

        public RobiController(ILogger<RobiController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RobiDataViewer()
        {          
            
            return View();
        }

        public IActionResult RobiDataEditor()
        {

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public dynamic GetCloud7RobiServices()
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = dataService.GetServices("All");
            return Json(jsonServices);
        }

        public dynamic GetActivationData(string serviceIds, string fromDate, string toDate, string dataType)
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = dataService.GetActivationData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate), dataType);
            return Json(jsonServices);
        }

        public dynamic GetRenewalData(string serviceIds, string fromDate, string toDate, string dataType)
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = dataService.GetRenewalData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate), dataType);
            return Json(jsonServices);
        }

        public dynamic GetChurnData(string serviceIds, string fromDate, string toDate, string dataType)
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = dataService.GetChurnData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate), dataType);
            return Json(jsonServices);
        }

        public dynamic GetTrafficData(string serviceIds, string fromDate, string toDate)
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = dataService.GetTrafficData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate));
            return Json('['+jsonServices+']');
        }

        public dynamic GetSubscriptionDetailsData(string msisdn, string reportDate)
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = dataService.GetSubscriptionDetailsData(msisdn, DateTime.Parse(reportDate));
            return Json(jsonServices);
        }

        public async Task<dynamic> DeactivateMSISDNByServiceId(string msisdn, string serviceId, string subscriptionId, string reason)
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = await dataService.DeactivateMSISDNByServiceId(msisdn, serviceId, subscriptionId, reason);
            return Json(jsonServices);
        }

        public dynamic DNDMSISDNByServiceId(string msisdn, string serviceId, string subscriptionId)
        {
            RobiDataService dataService = new RobiDataService();
            int jsonServices = dataService.DNDMSISDNByServiceId(msisdn, serviceId, subscriptionId);
            return Json(jsonServices);
        }

        public dynamic GetRenewalDetailsData(string msisdn, string reportDate)
        {
            RobiDataService dataService = new RobiDataService();
            string jsonServices = dataService.GetRenewalDetailsData(msisdn, DateTime.Parse(reportDate));
            return Json(jsonServices);
        }

    }
}