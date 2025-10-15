using EximbayTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public class PaymentController : Controller
{
    private readonly EximbaySettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentController(IOptions<EximbaySettings> settings, IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
    }

    #region Eximbay 결제 설정 공통
    private string GetApiReadyUrl() => _settings.ApiReadyUrl;
    private string GetApiKey() => _settings.ApiKey;
    private string GetMerchantId() => _settings.MerchantId;
    private string GetApiKeyToken() => _settings.ApiKeyToken;
    private string GetMerchantIdToken() => _settings.MerchantIdToken;
    private string GenerateOrderId() => DateTime.Now.ToString("yyyyMMddHHmmssfff");
    private string GetEximbayRequestUrl() => "https://4cd8cf94a572.ngrok-free.app";
    #endregion

    #region 공통 API 함수

    // POST 요청 (Json)
    private async Task<(bool Success, string Response, dynamic? Result)> PostJsonAsync(string url, string apiKey, object data)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKey);
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return (false, body, null);

        dynamic? result = null;
        try { result = JsonConvert.DeserializeObject(body); } catch { }
        return (true, body, result);
    }

    // GET 요청
    private async Task<(bool Success, string Response, dynamic? Result)> GetJsonAsync(string url, string apiKey)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return (false, body, null);

        dynamic? result = null;
        try { result = JsonConvert.DeserializeObject(body); } catch { }
        return (true, body, result);
    }
    #endregion

    [HttpGet]
    public IActionResult Ready()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Return()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Rebill()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Status()
    {
        return View();
    }


    [HttpPost]
    [Route("Payment/ReadyAsync")]
    public async Task<IActionResult> ReadyAsync()
    {
        var apiUrl = GetApiReadyUrl();
        var apiKey = GetApiKey();
        var mid = GetMerchantId();
        var orderId = GenerateOrderId();
        var requestUrl = GetEximbayRequestUrl();

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(mid))
        {
            ViewBag.ErrorMessage = "결제 설정값이 누락되었습니다.";
            return View("Ready");
        }

        var requestBody = new
        {
            payment = new
            {
                transaction_type = "PAYMENT",
                order_id = orderId,
                currency = "USD",
                amount = "1",
                lang = "KR"
            },
            merchant = new { mid },
            buyer = new { name = "eximbay", email = "test@eximbay.com" },
            url = new
            {
                return_url = $"{requestUrl}/Payment/ReturnAsync",
                status_url = $"{requestUrl}/Payment/StatusAsync"
            },
            settings = new { display_type = "R", issuer_country = "", ostype = "P" },
            other_param = new { param1 = orderId, param2 = "TIGERBOOKING" }
        };

        var (success, responseBody, result) = await PostJsonAsync(apiUrl, apiKey, requestBody);

        if (success && result != null)
        {
            ViewBag.Fgkey = result.fgkey;
            ViewBag.Rescode = result.rescode;
            ViewBag.Resmsg = result.resmsg;
            ViewBag.OrderId = orderId;
            ViewBag.PaymentData = JsonConvert.SerializeObject(requestBody.payment);
            ViewBag.MerchantData = JsonConvert.SerializeObject(requestBody.merchant);
            ViewBag.BuyerData = JsonConvert.SerializeObject(requestBody.buyer);
            ViewBag.UrlData = JsonConvert.SerializeObject(requestBody.url);
            ViewBag.SettingsData = JsonConvert.SerializeObject(requestBody.settings);
            ViewBag.OtherParamData = JsonConvert.SerializeObject(requestBody.other_param);
            return View("Ready");
        }
        ViewBag.ErrorMessage = $"ReadyAsync / API 호출 실패: {responseBody}";
        return View("Ready");
    }

    [HttpPost]
    [Route("Payment/ReadyTokenAsync")]
    public async Task<IActionResult> ReadyTokenAsync()
    {
        var apiUrl = GetApiReadyUrl();
        var apiKey = GetApiKeyToken();
        var mid = GetMerchantIdToken();
        var orderId = GenerateOrderId();
        var requestUrl = GetEximbayRequestUrl();
        var userId = "edgar";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(mid))
        {
            ViewBag.ErrorMessage = "결제 설정값이 누락되었습니다.";
            return View("Ready");
        }

        var requestBody = new
        {
            payment = new { transaction_type = "PAYMENT", order_id = orderId, currency = "KRW", amount = "1000", lang = "KR" },
            merchant = new { mid },
            buyer = new { name = "eximbay", email = "test@eximbay.com" },
            url = new
            {
                return_url = $"{requestUrl}/Payment/ReturnAsync",
                status_url = $"{requestUrl}/Payment/StatusAsync"
            },
            settings = new { display_type = "R", issuer_country = "", ostype = "P" },
            other_param = new { param1 = orderId, param2 = "TIGERBOOKING" },
            tokenbilling = new { token_creation = "Y" },
            fast_payment = new { user_id = userId, user_ci = "", phone_number = "", birthday = "", gender = "", foreigner = "" }
        };

        var (success, responseBody, result) = await PostJsonAsync(apiUrl, apiKey, requestBody);

        if (success && result != null)
        {
            ViewBag.Fgkey = result.fgkey;
            ViewBag.Rescode = result.rescode;
            ViewBag.Resmsg = result.resmsg;
            ViewBag.OrderId = orderId;
            ViewBag.PaymentData = JsonConvert.SerializeObject(requestBody.payment);
            ViewBag.MerchantData = JsonConvert.SerializeObject(requestBody.merchant);
            ViewBag.BuyerData = JsonConvert.SerializeObject(requestBody.buyer);
            ViewBag.UrlData = JsonConvert.SerializeObject(requestBody.url);
            ViewBag.SettingsData = JsonConvert.SerializeObject(requestBody.settings);
            ViewBag.OtherParamData = JsonConvert.SerializeObject(requestBody.other_param);
            ViewBag.TokenbillingData = JsonConvert.SerializeObject(requestBody.tokenbilling);
            ViewBag.FastPaymentData = JsonConvert.SerializeObject(requestBody.fast_payment);
            return View("Ready");
        }
        ViewBag.ErrorMessage = $"ReadyTokenAsync / API 호출 실패: {responseBody}";
        return View("Ready");
    }

    [HttpPost]
    [Route("Payment/RebillAsync")]
    public async Task<IActionResult> RebillAsync(string tokenid)
    {
        ViewBag.InputTokenId = tokenid;
        var apiKey = GetApiKeyToken();
        var mid = GetMerchantIdToken();
        var orderId = GenerateOrderId();

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(mid))
        {
            ViewBag.ErrorMessage = "결제 설정값이 누락되었습니다.";
            return View("Ready");
        }

        // 1. 토큰 카드 정보 조회
        string token = tokenid;
        string getUrl = $"https://api-test.eximbay.com/v1/payments/tokenbilling/{token}";
        string rebillUrl = $"https://api-test.eximbay.com/v1/payments/tokenbilling/{token}/rebill";

        var (getSuccess, tokenCardInfo, tokenResult) = await GetJsonAsync(getUrl, apiKey);
        ViewBag.TokenCardInfo = tokenCardInfo;
        string rescode = tokenResult?.rescode ?? null;
        if (!getSuccess || rescode != "0000")
        {
            ViewBag.ErrorMessage = $"토큰 카드 식별값 조회 실패(rescode={rescode}): {tokenCardInfo}";
            return View("Rebill");
        }

        // 2. 토큰 카드 결제(재결제) 요청
        var requestBody = new
        {
            payment = new { order_id = orderId, currency = "KRW", amount = "1000", lang = "KR" },
            merchant = new { mid },
            buyer = new { name = "eximbay", email = "test@eximbay.com" },
            product = new[] { new { name = "test 상품", quantity = 1, unit_price = "1000" } },
            other_param = new { param1 = orderId, param2 = "TIGERBOOKING" }
        };

        var (postSuccess, responseBody, result) = await PostJsonAsync(rebillUrl, apiKey, requestBody);

        if (postSuccess && result != null)
        {
            ViewBag.Rescode = result.rescode;
            ViewBag.Resmsg = result.resmsg;
            ViewBag.OrderId = orderId;
            ViewBag.ResResult = JsonConvert.SerializeObject(result);
            return View("Rebill");
        }
        ViewBag.ErrorMessage = $"RebillAsync / API 호출 실패: {responseBody}";
        return View("Rebill");
    }

    #region 콜백
    [HttpPost]
    [Route("Payment/ReturnAsync")]
    public async Task<IActionResult> ReturnAsync()
    {
        string bodyStr = await ReadRequestBodyAsync();
        if (!string.IsNullOrEmpty(bodyStr))
            ViewBag.bodyStr = bodyStr;
        else
            ViewBag.ErrorMessage = "응답 수신 실패: Request.Body 없음";
        return View("Return");
    }

    [HttpPost]
    [Route("Payment/StatusAsync")]
    public async Task<IActionResult> StatusAsync()
    {
        string bodyStr = await ReadRequestBodyAsync();
        if (!string.IsNullOrEmpty(bodyStr))
            ViewBag.bodyStr = bodyStr;
        else
            ViewBag.ErrorMessage = "StatusAsync / 응답 수신 실패: Request.Body 없음";
        return View("Status");
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        if (!Request.Body.CanSeek)
            Request.EnableBuffering();
        Request.Body.Seek(0, SeekOrigin.Begin);
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            var body = await reader.ReadToEndAsync();
            return string.IsNullOrEmpty(body) ? "" : body;
        }
    }
    #endregion
}
