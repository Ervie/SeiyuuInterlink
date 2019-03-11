﻿using AutoMapper;
using SeiyuuMoe.Contracts.Dtos;
using SeiyuuMoe.Data.Model;

namespace SeiyuuMoe.BusinessServices.Mapper.Profiles
{
	class SeiyuuProfile : Profile
	{
		public SeiyuuProfile()
		{
			CreateMap<Seiyuu, SeiyuuSearchEntryDto>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
				.ForMember(dest => dest.MalId, opt => opt.MapFrom(src => src.MalId))
				.ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
		}
	}
}