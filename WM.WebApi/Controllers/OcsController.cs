using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WM.Application.Interface;
using WM.Application.ViewModel.OC;

namespace WM.WebApi.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OcsController : ControllerBase
    {
        private readonly IOCService _ocService;

        public OcsController(IOCService ocService)
        {
            _ocService = ocService;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult> GetListTree()
        {
            return Ok(await _ocService.GetListTree());
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<IActionResult> Rename([FromBody]TreeViewRename oC)
        {
            return Ok(await _ocService.Rename(oC));
        }
        [HttpPost]
        public async Task<IActionResult> CreateOC([FromBody]CreateOCViewModel oC)
        {
            return Ok(await _ocService.CreateOC(oC));
        }
        [HttpPost]
        public async Task<IActionResult> CreateSubOC([FromBody]CreateOCViewModel oC)
        {
            return Ok(await _ocService.CreateSubOC(oC));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _ocService.Delete(id));
        }
    }
}