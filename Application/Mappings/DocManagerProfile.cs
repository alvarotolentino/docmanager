using System;
using System.IO;
using System.Linq;
using Application.Features.Account.Commands.AddUserGroup;
using Application.Features.Account.Commands.AddUserRole;
using Application.Features.Account.Commands.RegisterAccount;
using Application.Features.Account.Queries.GetAccounts;
using Application.Features.Documents.Commands.AssignGroupPermission;
using Application.Features.Documents.Commands.AssignUserPermission;
using Application.Features.Documents.Commands.CreateDocument;
using Application.Features.Documents.Queries.DownloadDocumentById;
using Application.Features.Documents.Queries.GetAllDocuments;
using Application.Features.Documents.Queries.GetDocumentById;
using Application.Features.Groups.Commands.RegisterGroup;
using Application.Features.Groups.Commands.UpdateGroup;
using Application.Features.Groups.Queries.GetGroupById;
using Application.Features.Groups.Queries.GetGroups;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class DocManagerProfile : Profile
    {
        public DocManagerProfile()
        {
            CreateMap<CreateDocument, Document>()
            .ForMember(dto => dto.ContentType, opts => opts.MapFrom(s => s.file.ContentType))
            .ForMember(dto => dto.Length, opts => opts.MapFrom(s => s.file.Length))
            .ForMember(dto => dto.Name, opts => opts.MapFrom(s => s.file.FileName))
            .ForMember(dto => dto.Data, opts => opts.Ignore())
            .AfterMap((source, destination) =>
            {
                using (var ms = new MemoryStream())
                {
                    source.file.OpenReadStream().CopyTo(ms);
                    destination.Data = ms.ToArray();
                }
            });

            CreateMap<GetAllDocumentsQuery, GetAllDocumentsParameter>();

            CreateMap<Document, GetAllDocumentsViewModel>()
            .ForMember(dto => dto.Length, opts => opts.Ignore())
            .AfterMap((source, destination) =>
            {
                destination.Length = FormatSize(source.Length);
            });

            CreateMap<Document, DownloadDocumentViewModel>()
            .ForMember(dto => dto.FileName, opts => opts.MapFrom(s => s.Name))
            .ForMember(dto => dto.Content, opts => opts.MapFrom(s => s.Data));


            CreateMap<Document, GetDocumentInfoViewModel>()
            .ForMember(dto => dto.Length, opts => opts.Ignore())
            .AfterMap((source, destination) =>
            {
                destination.Length = FormatSize(source.Length);
            });

            CreateMap<RegisterGroup, Group>()
            .ForMember(dto => dto.Id, opts => opts.Ignore())
            .ForMember(dto => dto.UserGroups, opts => opts.Ignore());

            CreateMap<UpdateGroup, Group>()
            .ForMember(dto => dto.UserGroups, opts => opts.Ignore());

            CreateMap<AddUserGroup, UserGroup>()
            .ForMember(dto => dto.User, opts => opts.Ignore())
            .ForMember(dto => dto.Group, opts => opts.Ignore());

            CreateMap<Group, GetGroupByIdViewModel>();

            CreateMap<Group, UpdateGroupViewModel>();

            CreateMap<GetGroupsQuery, GetAllGroupsParameter>();
            CreateMap<Group, GetGroupsViewModel>();
            CreateMap<User, AddUserGroupViewModel>()
            .ForMember(dto => dto.UserId, opts => opts.MapFrom(s => s.Id))
            .ForMember(dto => dto.UserName, opts => opts.MapFrom(s => s.UserName))
            .ForMember(dto => dto.GroupId, opts => opts.Ignore())
            .ForMember(dto => dto.GroupName, opts => opts.Ignore())
            .AfterMap((source, destination) =>
            {
                var ids = source.Groups.Select(x => x.Id).ToList();
                var names = source.Groups.Select(x => x.Name).ToList();
                destination.GroupId = string.Join(",", ids);
                destination.GroupName = string.Join(",", names);
            });

            CreateMap<AssignUserPermission, UserDocument>();
            CreateMap<UserDocument, AssignUserPermissionViewModel>();
            CreateMap<AssignGroupPermission, GroupDocument>();
            CreateMap<GroupDocument, AssignGroupPermissionViewModel>();

            CreateMap<User, AddUserRoleViewModel>()
            .ForMember(dto => dto.UserId, opts => opts.MapFrom(s => s.Id))
            .ForMember(dto => dto.UserName, opts => opts.MapFrom(s => s.UserName))
            .ForMember(dto => dto.RoleId, opts => opts.MapFrom(s => s.Roles.FirstOrDefault().Id))
            .ForMember(dto => dto.RoleName, opts => opts.MapFrom(s => s.Roles.FirstOrDefault().Name));

            CreateMap<GetAllAccountsQuery, GetAllAccountsParameter>();

            CreateMap<User, GetAllAccountsViewModel>();
        }

        private static string FormatSize(long size)
        {
            return size < 100 ? $"{size} bytes" : $"{(size / 1024).ToString("#.##")} KB";
        }

    }
}