using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using WM.Application.ViewModel.OC;
using WM.Data.Entities;

namespace WM.Application.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<OC, TreeView>()
                .ForMember(d => d.key, s => s.MapFrom(p => p.ID))
                .ForMember(d => d.levelnumber, s => s.MapFrom(p => p.Level))
                .ForMember(d => d.title, s => s.MapFrom(p => p.Name))
                .ForMember(d => d.parentid, s => s.MapFrom(p => p.ParentID));
            CreateMap<OC, TreeViewOC>();
            CreateMap<OC, TreeViewRename>();
            //CreateMap<Function, FunctionViewModel>();
            //CreateMap<Permission, PermissionViewModel>();

            //CreateMap<Product, ProductViewModel>();
            //CreateMap<AppUser, AppUserViewModel>();
            //CreateMap<AppRole, AppRoleViewModel>();
        }
    }
}
