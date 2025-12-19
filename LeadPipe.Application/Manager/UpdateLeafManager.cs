using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IUpdateLeafManager : IUpdateManager<Plumbing> { }
public sealed class UpdateLeafManager([FromKeyedServices(Source.Leaf)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLeafManager { }
