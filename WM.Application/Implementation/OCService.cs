using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Application.ViewModel.OC;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{

    public class OCService : IOCService
    {
        private readonly IOCRepository _oCRepository;
        IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _mapperConfig;

        public OCService(
            IOCRepository oCRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            MapperConfiguration mapperConfig
            )
        {
            _oCRepository = oCRepository;
            _mapper = mapper;
            _mapperConfig = mapperConfig;
            _unitOfWork = unitOfWork;
        }
        public async Task<List<TreeView>> GetListTree()
        {
            var levels = await _oCRepository.FindAll().OrderBy(x => x.Level).ProjectTo<TreeView>(_mapperConfig).ToListAsync();
            List<TreeView> hierarchy = new List<TreeView>();
            hierarchy = levels.Where(c => c.parentid == 0)
                            .Select(c => new TreeView()
                            {
                                key = c.key,
                                title = c.title,
                                code = c.code,
                                levelnumber = c.levelnumber,
                                parentid = c.parentid,
                                children = GetChildren(levels, c.key)
                            })
                            .ToList();
            return hierarchy;
        }
        private async Task<List<TreeView>> GetListTree(int parentID, int id)
        {
            var levels = await _oCRepository.FindAll().OrderBy(x => x.Level).ProjectTo<TreeView>(_mapperConfig).ToListAsync();
            List<TreeView> hierarchy = new List<TreeView>();

            hierarchy = levels.Where(c => c.key == id && c.parentid == parentID)
                            .Select(c => new TreeView()
                            {
                                key = c.key,
                                title = c.title,
                                code = c.code,
                                levelnumber = c.levelnumber,
                                parentid = c.parentid,
                                children = GetChildren(levels, c.key)
                            })
                            .ToList();
            return hierarchy;
        }
        private void HieararchyWalk(List<TreeView> hierarchy)
        {
            if (hierarchy != null)
            {
                foreach (var item in hierarchy)
                {
                    //Console.WriteLine(string.Format("{0} {1}", item.Id, item.Text));
                    HieararchyWalk(item.children);
                }
            }
        }
        public List<TreeView> GetChildren(List<TreeView> levels, int parentid)
        {
            return levels
                    .Where(c => c.parentid == parentid)
                    .Select(c => new TreeView()
                    {
                        key = c.key,
                        title = c.title,
                        code = c.code,
                        levelnumber = c.levelnumber,
                        parentid = c.parentid,
                        children = GetChildren(levels, c.key)
                    })
                    .ToList();
        }
        public async Task<bool> IsExistsCode(int ID)
        {
            return await _oCRepository.FindAll().AnyAsync(x => x.ID == ID);
        }

        public async Task<bool> Rename(TreeViewRename level)
        {
            var item =await _oCRepository.FindByIdAsync(level.key);
            item.Name = level.title;
            try
            {
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<IEnumerable<TreeViewOC>> GetListTreeOC(int parentID, int id)
        {
            var levels = await _oCRepository.FindAll().OrderBy(x => x.Level).ProjectTo<TreeViewOC>(_mapperConfig).ToListAsync();

            List<TreeViewOC> hierarchy = new List<TreeViewOC>();

            hierarchy = levels.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewOC()
                            {
                                ID = c.ID,
                                Name = c.Name,
                                Level = c.Level,
                                ParentID = c.ParentID,
                                children = GetTreeChildren(levels, c.ID)
                            })
                            .ToList();
            return hierarchy;
        }
        private void HieararchyWalkTree(List<TreeViewOC> hierarchy)
        {
            if (hierarchy != null)
            {
                foreach (var item in hierarchy)
                {
                    //Console.WriteLine(string.Format("{0} {1}", item.Id, item.Text));
                    HieararchyWalkTree(item.children);
                }
            }
        }
        public List<TreeViewOC> GetTreeChildren(List<TreeViewOC> levels, int parentid)
        {
            return levels
                    .Where(c => c.ParentID == parentid)
                    .Select(c => new TreeViewOC()
                    {
                        ID = c.ID,
                        Name = c.Name,
                        Level = c.Level,
                        ParentID = c.ParentID,
                        children = GetTreeChildren(levels, c.ID)
                    })
                    .ToList();
        }
        public List<TreeView> GetTreeChildren(List<TreeView> levels, int parentid)
        {
            return levels
                    .Where(c => c.parentid == parentid)
                    .Select(c => new TreeView()
                    {
                        key = c.key,
                        title = c.title,
                        levelnumber = c.levelnumber,
                        parentid = c.parentid,
                        children = GetTreeChildren(levels, c.key)
                    })
                    .ToList();
        }

        public async Task<object> CreateOC(CreateOCViewModel oc)
        {
            try
            {
                if (oc.ID == 0)
                {
                    var item = _mapper.Map<CreateOCViewModel,OC>(oc);
                    item.Level = 1;

                   await _oCRepository.AddAsync(item);
                }
                else
                {
                    var item =await _oCRepository.FindByIdAsync(oc.ID);
                    item.Name = oc.Name;
                }

                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<object> ListOCIDofUser(int ocid)
        {
            try
            {
                var item =await _oCRepository.FindByIdAsync(ocid);
                var ocs = await GetListTree(item.ParentID, item.ID);

                var arrocs = GetAllDescendants(ocs).Select(x => x.key).ToArray();
                return arrocs;
            }
            catch (Exception)
            {

                return new int[] { };
            }

        }
        public async Task<object> CreateSubOC(CreateOCViewModel oc)
        {

            var item = _mapper.Map<CreateOCViewModel,OC>(oc);

            //Level cha tang len 1 va gan parentid cho subtask
            var taskParent =await _oCRepository.FindByIdAsync(item.ParentID);
            item.Level = taskParent.Level + 1;
            item.ParentID = oc.ParentID;
           await _oCRepository.AddAsync(item);

            try
            {
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private IEnumerable<TreeView> GetAllDescendants(IEnumerable<TreeView> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllDescendants(x.children));
            return rootNodes.Concat(descendants);
        }
        public async Task<bool> Delete(int ID)
        {
            var item =await _oCRepository.FindByIdAsync(ID);

            var levels = await _oCRepository.FindAll().OrderBy(x => x.Level).ProjectTo<TreeView>(_mapperConfig).ToListAsync();
            var ocs = GetTreeChildren(levels, item.ID);
            var arrOCs = GetAllDescendants(ocs).Select(x => x.key).ToList();
            arrOCs.Add(item.ID);
            var items = _oCRepository.FindAll().Where(x => arrOCs.Contains(x.ID)).ToList();
            _oCRepository.RemoveMultiple(items);
            try
            {
                await _unitOfWork.Commit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
