using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WM.Application.Interface;
using WM.Application.ViewModel.OC;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;
using WM.WebApi.helpers;

namespace WM.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleService _roleService;

        public RolesController(IUnitOfWork unitOfWork, IRoleService roleService)
        {
            _unitOfWork = unitOfWork;
            _roleService = roleService;
        }

        // GET: api/Roles
        [HttpGet("{page}/{pageSize}/{text}")]
        [HttpGet("{page}/{pageSize}")]
        public async Task<ActionResult> GetAllPaging(int page, int pageSize, string text)
        {
            var model = await _roleService.GetAllPaging(page, pageSize, text);
            return Ok(new
            {
                data = model,
                total = model.TotalPages,
                page,
                pageSize
            });
        }
        [HttpGet("{page}/{pageSize}")]
        [HttpGet("{page}/{pageSize}/{text}")]
        public async Task<ActionResult> GetRoles(int page, int pageSize, string text)
        {
            var model = await _roleService.GetAllPaging(page, pageSize, text);
            Response.AddPagination(model.CurrentPage, model.PageSize, model.TotalCount, model.TotalPages);
            return Ok(model);
        }
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            return Ok(await _roleService.GetAll());
        }
        [HttpGet]
        public async Task<ActionResult> GetListRole()
        {
            return Ok(await _roleService.GetAll());
        }
        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByID(int id)
        {
            var project = await _roleService.GetByID(id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        // PUT: api/Roles/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<IActionResult> Update(Role project)
        {
            return Ok(await _roleService.Update(project));
        }

        // POST: api/Roles
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Role>> Create(Role project)
        {
            return Ok(await _roleService.Create(project));
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Role>> DeleteRole(int id)
        {
            return Ok(await _roleService.Delete(id));
        }
    }
}