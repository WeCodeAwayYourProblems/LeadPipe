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
        services.AddScoped<IDtoToVo<LatherDto, Plumbing>, LatherDtoToPlumbing>();

        // IEntityToVo
        services.AddScoped<IEntityToVo<CaliperEntity, Caliper>, CaliperEntityToCaliper>();
        services.AddScoped<IVoToEntity<Custard, CustardEntity>, CustardToCustardEntity>();
        services.AddScoped<IEntityToVo<SandEntity, Sandwich>, SandEntityToSandwich>();
        services.AddScoped<IEntityToVo<PlumbingEntity, Plumbing>, PlumbingEntityToPlumbing>();

        services.AddScoped<IEntityToVo<CaliperMySqlEntity, Caliper>, CaliperMySqlEntityToCaliper>();
        services.AddScoped<IEntityToVo<SandMySqlEntity, Sandwich>, SandMySqlEntityToSandwich>();
        services.AddScoped<IEntityToVo<CustardMySqlEntity, Custard>, CustardMySqlEntityToCustard>();
        services.AddScoped<IEntityToVo<CornMySqlEntity, CornFormula>,CornMySqlEntityToCornFormula>();

        // IVoToDto
        services.AddScoped<IVoToDto<Plumbing, LabDto>, PlumbingToLabDto>();
        services.AddScoped<IVoToDto<Plumbing, LeafDto>, PlumbingToLeafDto>();
        services.AddScoped<IVoToDto<Plumbing, YellerDto>, PlumbingToYellerDto>();

        // IVoToEntity
        services.AddScoped<IVoToEntity<Caliper, CaliperEntity>, CaliperToCaliperEntity>();
        services.AddScoped<IVoToEntity<Plumbing, PlumbingEntity>, PlumbingToPlumbingEntity>();
        services.AddScoped<IVoToEntity<Sandwich, SandEntity>, SandwichToSandEntity>();
        services.AddScoped<IVoToEntity<CornFormula, CornEntity>, CornFormulaToCornEntity>();

        // IEntityToReport
        services.AddScoped<IEntityToReport<SandPlumbingLink, ReportPlumbing>, SandPlumbingLinkToReportPlumbing>();
        services.AddScoped<IEntityToReport<SandEntity, ReportYeller>, SandEntityToReportYeller>();
        services.AddScoped<IEntityToReport<PlumbingEntity, ReportYeller>, PlumbingEntityToReportYeller>();
        services.AddScoped<IEntityToReport<CaliperEntity, ReportYeller>, CaliperEntityToReportYeller>();
        services.AddScoped<IEntityToReport<CornEntity, ReportYeller>, CornEntityToReportYeller>();
        services.AddScoped<IEntityToReport<CustardPlumbingLink, ReportYeller>, CustardPlumbingLinkToReportYeller>();
        services.AddScoped<IEntityToReport<CustardCaliperLink, ReportYeller>, CustardCaliperLinkToReportYeller>();
        services.AddScoped<IEntityToReport<CustardCornLink, ReportYeller>, CustardCornLinkToReportYeller>();


        return services;
    }
}
