using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WM.Application.Interface;

namespace WM.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineNotifyController : ControllerBase
    {
        private readonly ILineService _lineService;
        private readonly IConfiguration _config;
        private readonly string _successUri;
        public LineNotifyController(ILineService lineService, IConfiguration config)
        {
            _lineService = lineService;
            _config = config;
            var lineConfig = _config.GetSection("LineNotifyConfig");
            _successUri = lineConfig.GetValue<string>("successUri");
        }
        // GET: api/Authorize
        [HttpGet]
        public IActionResult GetAuthorize()
        {
            var uri = _lineService.GetAuthorizeUri();
            Response?.Redirect(uri);

            return new EmptyResult();
        }

        // GET: api/Authorize/Callback
        /// <summary>取得使用者 code</summary>
        /// <param name="code">用來取得 Access Tokens 的 Authorize Code</param>
        /// <param name="state">驗證用。避免 CSRF 攻擊</param>
        /// <param name="error">錯誤訊息</param>
        /// <param name="errorDescription">錯誤描述</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Callback")]
        public async Task<IActionResult> GetCallback(
            [FromQuery]string code,
            [FromQuery]string state,
            [FromQuery]string error,
            [FromQuery][JsonProperty("error_description")]string errorDescription)
        {
            if (!string.IsNullOrEmpty(error))
                return new JsonResult(new
                {
                    error,
                    state,
                    errorDescription
                });
            Response.Redirect(_successUri + "?token=" + await FetchToken(code));

            return new EmptyResult();
        }

        /// <summary>Nhận mã thông báo người dùng</summary>
        /// <param name="code">Mã ủy quyền được sử dụng để nhận Mã thông báo truy cập</param>
        /// <returns></returns>
        private async Task<string> FetchToken(string code)
        {
             return JsonConvert.DeserializeObject<JObject>(await _lineService.FetchToken(code))["access_token"].ToString();
        }
    }
}
