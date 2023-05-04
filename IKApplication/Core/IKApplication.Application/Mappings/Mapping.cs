﻿using AutoMapper;
using IKApplication.Application.DTOs.CompanyDTOs;
using IKApplication.Application.DTOs.UserDTOs;
using IKApplication.Application.VMs.CompanyVMs;
using IKApplication.Application.VMs.UserVMs;
using IKApplication.Domain.Entites;

namespace IKApplication.Application.Mappings
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Company, CompanyDTO>().ReverseMap();
            CreateMap<Company, CompanyVM>().ReverseMap();

            CreateMap<AppUser, AppUserUpdateDTO>().ReverseMap();
            CreateMap<AppUser, AppUserCreateDTO>().ReverseMap();
            CreateMap<AppUser, AppUserVM>().ReverseMap();
        }
    }
}