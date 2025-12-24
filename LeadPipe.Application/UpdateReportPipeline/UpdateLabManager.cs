using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateLabManager : IUpdateManager<Plumbing> { }
internal sealed class UpdateLabManager([FromKeyedServices(Source.Lab)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLabManager { }
