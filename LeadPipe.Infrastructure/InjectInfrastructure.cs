using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Load;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Data.Transform;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Factory;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Service;
using LeadPipe.Infrastructure.Service.Report;
using LeadPipe.Infrastructure.Service.Update;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LeadPipe.Infrastructure;

public static class InjectInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IInfrastructureSettings settings, IConfiguration config)
    {
        // Format: services.AddScoped<Interface, Class>();

        // *****************************************
        #region ADD DATA

        // Data Persistence
        services.AddScoped<IDataPersistence<CaliperEntity>, CaliperEntityPersistence>();
        services.AddScoped<IDataPersistence<CustardEntity>, CustardEntityPersistence>();
        services.AddScoped<IDataPersistence<PlumbingCaliperLink>, PlumbingCaliperLinkPersistence>();
        services.AddScoped<IDataPersistence<PlumbingEntity>, PlumbingPersistence>();
        services.AddScoped<IDataPersistence<SandCaliperLink>, SandCaliperLinkPersistence>();
        services.AddScoped<IDataPersistence<SandEntity>, SandEntityPersistence>();
        services.AddScoped<IDataPersistence<SandPlumbingLink>, SandPlumbingLinkPersistence>();
        services.AddScoped<IDataPersistence<Caliper>, CaliperPersistence>();
        services.AddScoped<IDataPersistence<Sandwich>, SandwichPersistence>();
        services.AddScoped<IDataPersistence<Custard>, CustardPersistence>();

        // Transformers
        services.AddScoped<ITransform<Plumbing, ReportYeller>, TransformYellerReport>();
        services.AddScoped<ITransform<Plumbing, ReportPlumbing>, TransformPlumbingReport>();

        // Loaders
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadCalli>(Source.Calli);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLab>(Source.Lab);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeaf>(Source.Leaf);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLeased>(Source.Leased);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadLibacion>(Source.Libacion);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadPan>(Source.Pan);
        services.AddKeyedScoped<ILoadData<Plumbing>, LoadYeller>(Source.Yeller);

        // Data Sources
        services.AddScoped<IDataSourceAsync<LabDto>, LabDataSource>();
        services.AddScoped<IDataSourceAsync<LeafDto>, LeafDataSource>();
        services.AddScoped<IDataSourceAsync<YellerDto>, YellerDataSource>();
        services.AddScoped<IDataSourceAsync<CalliDto>, CalliFileDataSource>();
        services.AddScoped<IDataSourceAsync<LeasedDto>, LeasedFileDataSource>();
        services.AddScoped<IDataSourceAsync<LibacionDto>, LibacionFileDataSource>();
        services.AddScoped<IDataSourceAsync<PanDto>, PanFileDataSource>();
        services.AddScoped<IDataSourceAsync<CaliperMySqlEntity>, CaliperMySqlDataSource>();
        services.AddScoped<IDataSourceAsync<SandMySqlEntity>, SandMySqlDataSource>();
        services.AddScoped<IDataSourceAsync<CustardMySqlEntity>, CustardMySqlDataSource>();

        #endregion
        // *****************************************

        // *****************************************
        #region ADD SERVICES

        // Keyed update services
        services.AddKeyedScoped<IUpdateService<Plumbing>, CalliUpdateFromFileService>(Source.Calli);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LabUpdateService>(Source.Lab);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LibacionUpdateService>(Source.Libacion);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeafUpdateService>(Source.Leaf);
        services.AddKeyedScoped<IUpdateService<Plumbing>, LeasedUpdateFromFileService>(Source.Leased);
        services.AddKeyedScoped<IUpdateService<Plumbing>, PanUpdateFromFileService>(Source.Pan);
        services.AddKeyedScoped<IUpdateService<Plumbing>, YellerUpdateService>(Source.Yeller);
        services.AddKeyedScoped<IUpdateService<Plumbing>, DummyUpdateService>(Source.Test);
        services.AddKeyedScoped<IUpdateService<Plumbing>, DummyUpdateService2>(Source.Test2);

        // Nonkeyed update services
        services.AddScoped<IUpdateService<Caliper>, CalipersUpdateService>();
        services.AddScoped<IUpdateService<Sandwich>, SandwichUpdateService>();
        services.AddScoped<IUpdateService<Custard>, CustardUpdateService>();

        // Keyed Report services
        services.AddKeyedScoped<IReportService<Plumbing>, CalliReportService>(Source.Calli);
        services.AddKeyedScoped<IReportService<Plumbing>, LabReportService>(Source.Lab);
        services.AddKeyedScoped<IReportService<Plumbing>, LeafReportService>(Source.Leaf);
        services.AddKeyedScoped<IReportService<Plumbing>, LeasedReportService>(Source.Leased);
        services.AddKeyedScoped<IReportService<Plumbing>, LibacionReportService>(Source.Libacion);
        services.AddKeyedScoped<IReportService<Plumbing>, PanReportService>(Source.Pan);
        services.AddKeyedScoped<IReportService<Plumbing>, ReportBothYeller>(Source.Yeller);

        // Special Report Services
        services.AddKeyedScoped<IReportService<Plumbing>, YellerClientReportService>(Schedule.Daily);
        services.AddKeyedScoped<IReportService<Plumbing>, YellerCsvReportService>(Schedule.TwoDays);

        // Factories for keyed services
        services.AddScoped<IReportSourceFactory, ReportSourceFactory>();
        services.AddScoped<IUpdateFactory, UpdateSourceFactory>();

        // Report services
        services.AddKeyedScoped<IReport<ReportPlumbing>, CalliReporter>(Source.Calli);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LabReporter>(Source.Lab);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LeasedReporter>(Source.Leased);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LeafReporter>(Source.Leaf);
        services.AddKeyedScoped<IReport<ReportPlumbing>, LibacionReporter>(Source.Libacion);
        services.AddKeyedScoped<IReport<ReportPlumbing>, PanReporter>(Source.Pan);
        services.AddKeyedScoped<IReport<ReportPlumbing>, YellerCsvReporter>(Source.Yeller);
        //services.AddScoped<IReport<ReportYeller>, YellerClientReporter>();
        services.AddScoped<IReport<ReportYeller>, YellerJsonReporter>();

        // Scoped services
        services.AddScoped<ICsvRwService, CsvRwService>();
        services.AddScoped<IFileRWService, FileConversionService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IJsonRwService, JsonRwService>();
        services.AddScoped<ILabService, LabService>();
        services.AddScoped<ILeafService, LeafClientService>();
        services.AddScoped<IEntityAssociationService, EntityAssociationService>();
        services.AddScoped<IYellerService, YellerClientService>();

        services.AddScoped<ISyncGate, SyncGate>();

        #endregion
        // *****************************************

        // *****************************************
        #region ADD CLIENTS

        bool useTestClientsGlobal = config.GetValue("HttpClients:UseTestClients", false);
        string accept = "application/json";

        // Add Leaf Client
        RegisterHttpClient(settings.LeafName, settings.LeafBase, accept, settings.LeafToken, services, useTestClientsGlobal, new BaseTestHttpMessageHandler(_leafDto));

        // Add Lab Client
        if (string.IsNullOrWhiteSpace(settings.LabAccept))
            throw new Exception($"{nameof(settings.LabAccept)} cannot be null");
        RegisterHttpClient(settings.LabName, settings.LabBase, settings.LabAccept, settings.LabToken, services, useTestClientsGlobal, new BaseTestHttpMessageHandler(_labDto));

        // Add Yeller Client
        bool useYellerGetterTestClient = config.GetValue("HttpClients:Yeller:Getter:UseTestClients", useTestClientsGlobal);

        RegisterHttpClient(settings.YellerGetterName, settings.YellerBase, accept, settings.YellerToken, services, useYellerGetterTestClient, new YellerTestHttpMessageHandler(_yellerHelperDto, _yellerDto, settings));

        // Add Second Yeller Client
        bool useYellerReporterTestClient = config.GetValue("HttpClients:Yeller:Reporter:UseTestClients", useTestClientsGlobal);
        RegisterHttpClient(settings.YellerReporterName, settings.YellerBase, accept, settings.YellerToken, services, useYellerReporterTestClient, new YellerTestHttpMessageHandler(_yellerHelperDto, _yellerDto, settings));

        #endregion
        // *****************************************

        return services;
    }

    private static void RegisterHttpClient(
        string? clientName,
        string? baseUri,
        string acceptType,
        Token? token,
        IServiceCollection services,
        bool useTestClients,
        BaseTestHttpMessageHandler handler)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            throw new Exception($"{nameof(clientName)} cannot be null");
        if (string.IsNullOrWhiteSpace(baseUri))
            throw new Exception($"{nameof(baseUri)} cannot be null");
        services.AddHttpClient(clientName, c =>
        {
            c.BaseAddress = new Uri(baseUri);
            c.DefaultRequestHeaders.Add("Accept", acceptType);

            if (!useTestClients)
            {
                if (token is null)
                    throw new Exception($"{nameof(Token)} cannot be null");

                c.DefaultRequestHeaders.Authorization = new(token.Token_type, token.Access_token);
            }
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            return useTestClients
                ? handler
                : new HttpClientHandler()
                {
                    AutomaticDecompression =
                        DecompressionMethods.GZip |
                        DecompressionMethods.Deflate
                };
        });
    }

    // Marker base class for DI-compatible test HTTP handlers
    public class BaseTestHttpMessageHandler(string content) : DelegatingHandler
    {
        private readonly string _content = content;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Return empty success response
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_content) // Or whatever shape your API expects
            };
            return Task.FromResult(response);
        }
    }

    public class YellerTestHttpMessageHandler(string prelim, string final, IYellerSettings settings) : BaseTestHttpMessageHandler(final)
    {
        private readonly IYellerSettings _settings = settings;
        private readonly string _prelim = prelim;
        private readonly string _final = final;
        private int _page = 0;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
        {
            bool isPrelim = request.RequestUri!.AbsolutePath.Contains(_settings.YellerPrelimEndpoint1!)
                && request.RequestUri!.AbsolutePath.Contains(_settings.YellerPrelimEndpoint2!);

            string content;
            if (isPrelim)
            {
                content = _page switch
                {
                    0 => JsonSerializer.Serialize(new YellerHelperDto() { lead_ids = ["id3", "id2"], has_more = true }, _options),
                    1 => JsonSerializer.Serialize(new YellerHelperDto() { lead_ids = ["id1"], has_more = true }, _options),
                    _ => _prelim,
                };
                _page++;
            }
            else content = _final;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private static readonly DateTime _creationDate = DateTime.UtcNow;
    private static readonly DateTime _modificationDate = DateTime.UtcNow;

    private static readonly string _leafDto = JsonSerializer.Serialize(new LeafDto()
    {
        uuid = "12345",
        profile = "profile",
        category = "category",
        read = true,
        spam = false,
        state = "state",
        channels = ["channel1"],
        creation = _creationDate,
        modification = _modificationDate,
        isCallRequest = false,
        tags = ["tag1", "tag2"],
        prospect = new Prospect
        {
            first_name = "firstName",
            last_name = "lastName",
            cellphone = "5555555555",
            creation = _creationDate,
            modification = _modificationDate,
            consent = true,
            blocked = false,
        },
        messages = [new Message {
                    creation = _creationDate,
                    modification = _modificationDate,
                    sent = _creationDate,
                    message = "This is a message"
                }]
    }, _options);

    private static readonly string _labDto = JsonSerializer.Serialize<List<LabDto>>([new LabDto()
    {
        PhoneNumber = 5555555555,
        Date = _creationDate
    }], _options);

    private static readonly string _yellerDto = JsonSerializer.Serialize(new YellerDto()
    {
        id = "id",
        business_id = "business_id",
        conversation_id = "conversation_id",
        temporary_email_address = "temporary email address",
        temporary_email_address_expiry = _creationDate + TimeSpan.FromDays(7),
        temporary_phone_number = "temporaryPhoneNumber",
        time_created = _creationDate,
        last_event_time = _modificationDate,
        user = "User",
        project = new Project
        {
            survey_answers = [new SurveyAnswer {
                question_text = "What's your phone number",
                answer_text = ["I won't give it", "fine it's 5555555555"]
            }]
        },
    }, _options);

    private static readonly string _yellerHelperDto = JsonSerializer.Serialize(new YellerHelperDto()
    {
        lead_ids = [],
        has_more = false
    }, _options);
}
