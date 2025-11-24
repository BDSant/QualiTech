using AutoMapper;
using OsLog.Application.DTOs.Clientes;
using OsLog.Domain.Entities;

namespace OsLog.Application.Mapping;

public class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        // Cliente -> DTO / CreateDto -> Cliente
        //CreateMap<Cliente, ClienteDto>();
        //CreateMap<ClienteCreateDto, Cliente>();
        //CreateMap<ClienteCreateDto, Cliente>(MemberList.Source);
        CreateMap<Cliente, ClienteDto>().ReverseMap();
    }
}
