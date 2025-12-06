using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingPersistence(IPlumbingRepository repo) : Persistence<IPlumbingRepository, PlumbingEntity>(repo), IDataPersistence<PlumbingEntity> { }
