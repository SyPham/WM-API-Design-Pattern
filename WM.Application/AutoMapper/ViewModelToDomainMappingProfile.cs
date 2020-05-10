using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using WM.Application.ViewModel.OC;
using WM.Data.Entities;

namespace WM.Application.AutoMapper
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile()
        {
            CreateMap<CreateOCViewModel, OC>();
            CreateMap<CreateOCViewModel, OC>();
            CreateMap<TreeViewRename, OC>();
            CreateMap<TreeViewOC, OC>();
            //CreateMap<ProductCategoryViewModel, ProductCategory>().ConstructUsing(c =>
            //new ProductCategory(
            //    c.Name,
            //    c.Description,
            //    c.ParentId,
            //    c.HomeOrder,
            //    c.Image,
            //    c.HomeFlag,
            //    c.SortOrder,
            //    c.Status,
            //    c.SeoPageTitle,
            //    c.SeoAlias,
            //    c.SeoKeywords,
            //    c.Description));

            //CreateMap<AppUserViewModel, AppUser>()
            //  .ConstructUsing(c => new AppUser(c.Id.GetValueOrDefault(Guid.Empty), c.FullName, c.UserName,
            //  c.Email, c.PhoneNumber, c.Avatar, c.Status, c.Address));


            //CreateMap<PermissionViewModel, Permission>()
            //.ConstructUsing(c => new Permission(c.RoleId, c.FunctionId, c.CanCreate, c.CanRead, c.CanUpdate, c.CanDelete)).ReverseMap();
        }


    }
}
