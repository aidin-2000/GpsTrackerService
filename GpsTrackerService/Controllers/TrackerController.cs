using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GpsTrackerService.Controllers
{
    public class TrackerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TrackerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Метод действия для отображения страницы
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] dynamic request)
        {
            string name = request.Name;

            var xmlLoginRequest = $@"
                <v:Envelope xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns:d='http://www.w3.org/2001/XMLSchema' xmlns:c='http://schemas.xmlsoap.org/soap/encoding/' xmlns:v='http://schemas.xmlsoap.org/soap/envelope/'>
                    <v:Header />
                    <v:Body>
                        <Login xmlns='http://tempuri.org/' id='o0' c:root='1'>
                            <LoginAPP i:type='d:string'>AKSH</LoginAPP>
                            <Pass i:type='d:string'>123456</Pass>
                            <LoginType i:type='d:int'>1</LoginType>
                            <Key i:type='d:string'>7DU2DJFDR8321</Key>
                            <Name i:type='d:string'>{name}</Name>
                            <GMT i:type='d:string'>6:00</GMT>
                        </Login>
                    </v:Body>
                </v:Envelope>";

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(xmlLoginRequest, Encoding.UTF8, "text/xml");

            var response = await client.PostAsync("http://app.aika168.com:8088/openapiv3.asmx", content);

            if (response.IsSuccessStatusCode)
            {
                var xmlResponse = await response.Content.ReadAsStringAsync();
                return Content(xmlResponse);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetTracking()
        {
            var xmlTrackingRequest = @"
                <v:Envelope xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns:d='http://www.w3.org/2001/XMLSchema' xmlns:c='http://schemas.xmlsoap.org/soap/encoding/' xmlns:v='http://schemas.xmlsoap.org/soap/envelope/'>
                    <v:Header />
                    <v:Body>
                        <GetTracking xmlns='http://tempuri.org/' id='o0' c:root='1'>
                            <TimeZones i:type='d:string'>Central Asia Standard Time</TimeZones>
                            <Language i:type='d:string'>ru_RU</Language>
                            <DeviceID i:type='d:int'>67548</DeviceID>
                            <Model i:type='d:int'>0</Model>
                            <MapType i:type='d:string'>Google</MapType>
                            <Key i:type='d:string'>ps1YFt/WtX2X42vh4W6L3rpaA9xBLiCJ7kLrFUmG32A2kCcqr6h1cYjDFT9hsa1k+/o5n1Aj3R6sAoLrkulM4g5qxi4+1Rcq71W9nUh8JDdlrK3ynJFrdr5Ledzt2O2UpS7xvu+bhK0ify6RIkn8Hg==</Key>
                        </GetTracking>
                    </v:Body>
                </v:Envelope>";

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(xmlTrackingRequest, Encoding.UTF8, "text/xml");

            var response = await client.PostAsync("http://app.aika168.com:8088/openapiv3.asmx", content);

            if (response.IsSuccessStatusCode)
            {
                var xmlResponse = await response.Content.ReadAsStringAsync();

                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(xmlResponse);

                var jsonResult = xmlDoc.GetElementsByTagName("GetTrackingResult")[0].InnerText;
                var trackingData = JObject.Parse(jsonResult);

                var lat = trackingData["lat"].ToString();
                var lng = trackingData["lng"].ToString();

                return Json(new { latitude = lat, longitude = lng });
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
