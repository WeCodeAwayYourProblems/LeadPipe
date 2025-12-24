using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateLeafManager : IUpdateManager<Plumbing> { }
internal sealed class UpdateLeafManager([FromKeyedServices(Source.Leaf)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLeafManager { }
