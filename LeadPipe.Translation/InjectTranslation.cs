using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.DtoToVo;
using LeadPipe.Translation.Translate.EntityToReport;
using LeadPipe.Translation.Translate.EntityToVo;
using LeadPipe.Translation.Translate.VoToDto;
using LeadPipe.Translation.Translate.VoToEntity;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LeadPipe.Translation;

public static class InjectTranslation
{
    public static IServiceCollection AddTranslation(this IServiceCollection services, IInfrastructureSettings settings)
    {
        // Format: services.AddScoped<Interface, Class>();

        // Primitives
        services.AddScoped<IDateTimeTranslate, DateTimeTranslate>();

        // Translations 
        // IDtoToVo
        services.AddScoped<IDtoToVo<CalliDto, Plumbing>, CalliDtoToPlumbing>();
        services.AddScoped<IDtoToVo<LabDto, Plumbing>, LabDtoToPlumbing>();
        services.AddScoped<IDtoToVo<LeafDto, Plumbing>, LeafDtoToPlumbing>();
        services.AddScoped<IDtoToVo<YellerDto, Plumbing>, YellerDtoToPlumbing>();
        services.AddScoped<IDtoToVo<LeasedDto, Plumbing>, LeasedDtoToPlumbing>();
        services.AddScoped<IDtoToVo<PanDto, Plumbing>, PanDtoToPlumbing>();
        services.AddScoped<IDtoToVo<LibacionDto, Plumbing>, LibacionDtoToPlumbing>();

        // IEntityToVo
        services.AddScoped<IEntityToVo<CallEntity, Call>, CallEntityToCall>();
        services.AddScoped<IEntityToVo<SubsEntity, Sandwich>, SubsToSandwich>();
        services.AddScoped<IEntityToVo<PlumbingEntity, Plumbing>, PlumbingEntityToPlumbing>();
        services.AddScoped<IEntityToVo<CallMySqlEntity, Call>, CallMySqlEntityToCall>();
        services.AddScoped<IEntityToVo<SubMySqlEntity, Sandwich>, SubMySqlEntityToSandwich>();

        // IVoToDto
        services.AddScoped<IVoToDto<Plumbing, LabDto>, PlumbingToLabDto>();
        services.AddScoped<IVoToDto<Plumbing, LeafDto>, PlumbingToLeafDto>();
        services.AddScoped<IVoToDto<Plumbing, YellerDto>, PlumbingToYellerDto>();

        // IVoToEntity
        services.AddScoped<IVoToEntity<Call, CallEntity>, CallToCallEntity>();
        services.AddScoped<IVoToEntity<Plumbing, PlumbingEntity>, PlumbingToPlumbingEntity>();
        services.AddScoped<IVoToEntity<Sandwich, SubsEntity>, SandToSub>();

        // IEntityToReport
        services.AddScoped<IEntityToReport<SubsPlumbingLink, ReportPlumbing>, SubsPlumbingLinkToReportPlumbing>();
        services.AddScoped<IEntityToReport<SubsPlumbingLink, ReportYeller>, SubsPlumbingLinkToYellerReport>();

        return services;
    }
}
