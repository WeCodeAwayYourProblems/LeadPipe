using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IUpdateLabManager : IUpdateManager<Plumbing> { }
public sealed class UpdateLabManager([FromKeyedServices(Source.Lab)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLabManager { }
