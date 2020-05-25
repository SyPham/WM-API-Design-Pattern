using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Ultilities.Helpers;
using WM.WebApi.helpers;

namespace WM.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public FollowController(ITaskService taskService)
        {

            _taskService = taskService;
        }

        [HttpGet("{sort}")]
        [HttpGet("{priority}/{sort}")]
        [HttpGet]
        public async Task<IActionResult> LoadFollow(string sort = "", string priority = "")
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            var OCID = JWTExtensions.GetDecodeTokenByProperty(token, "OCID").ToInt();
            return Ok(await _taskService.Follow(sort, priority, userID));
        }

        [HttpDelete("{taskid}")]
        public async Task<IActionResult> Unfollow(int taskid)
        {
            string token = Request.Headers["Authorization"];
            var userID = JWTExtensions.GetDecodeTokenByProperty(token, "nameid").ToInt();
            return Ok(await _taskService.Unsubscribe(taskid, userID));
        }



    }
}
