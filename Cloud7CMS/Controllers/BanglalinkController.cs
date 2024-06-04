using Cloud7CMS.Models;
using Cloud7CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;

namespace Cloud7CMS.Controllers
{
    [Authorize(Roles = "Admin,FunBoxManager")]
    public class BanglalinkController : Controller
    {
        private readonly ILogger<BanglalinkController> _logger;
		private readonly IConfiguration _configuration;
		public BanglalinkController(ILogger<BanglalinkController> logger, IConfiguration configuration)
        {
            _logger = logger;
			_configuration = configuration;
		}

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BLDataViewer()
        {          
            
            return View();
        }

        public IActionResult BLDataEditor()
        {

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult DialogPartial()
        {
			int updateInterval = _configuration.GetValue<int>("AppSettings:UpdateIntervalMilliseconds");
            ViewBag.UpdateInterval = updateInterval;

			return PartialView("_DialogPartial");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public dynamic GetCloud7BLServices()
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetServices("All");          

            return Json(jsonServices);
        }

        public dynamic GetActivationData(string serviceIds, string fromDate, string toDate, string dataType)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetActivationData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate), dataType);
            return Json(jsonServices);
        }

        public dynamic GetRenewalData(string serviceIds, string fromDate, string toDate, string dataType)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetRenewalData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate), dataType);
            return Json(jsonServices);
        }

        public dynamic GetChurnData(string serviceIds, string fromDate, string toDate, string dataType)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetChurnData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate), dataType);
            return Json(jsonServices);
        }

        public dynamic GetTrafficData(string serviceIds, string fromDate, string toDate)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetTrafficData(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate));
            return Json('['+jsonServices+']');
        }

        public dynamic GetTrafficDataMoreFun(string serviceIds, string fromDate, string toDate)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetTrafficDataMoreFun(serviceIds, DateTime.Parse(fromDate), DateTime.Parse(toDate));
            return Json('[' + jsonServices + ']');
        }

        public dynamic GetSubscriptionDetailsData(string msisdn, string reportDate)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetSubscriptionDetailsData(msisdn, DateTime.Parse(reportDate));
            return Json(jsonServices);
        }

        public async Task<dynamic> DeactivateMSISDNByServiceId(string msisdn, string serviceId, string subscriptionId, string reason)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = await dataService.DeactivateMSISDNByServiceId(msisdn, serviceId, subscriptionId, reason);
            return Json(jsonServices);
        }

        public dynamic DNDMSISDNByServiceId(string msisdn, string serviceId, string subscriptionId)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            int jsonServices = dataService.DNDMSISDNByServiceId(msisdn, serviceId, subscriptionId);
            return Json(jsonServices);
        }

        public dynamic GetRenewalDetailsData(string msisdn, string reportDate)
        {
            BanglalinkDataService dataService = new BanglalinkDataService();
            string jsonServices = dataService.GetRenewalDetailsData(msisdn, DateTime.Parse(reportDate));
            return Json(jsonServices);
        }

		public dynamic GetFunBoxLiveData(string productName)
		{
			int updateInterval = _configuration.GetValue<int>("AppSettings:UpdateIntervalMilliseconds");

			BanglalinkDataService dataService = new BanglalinkDataService();
			string jsonServices = dataService.GetLiveRenewalData(productName, updateInterval);			
			return Json(jsonServices);
		}

	}
}