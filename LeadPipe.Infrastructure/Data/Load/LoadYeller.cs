using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Load;

[SourceKey(Source.Yeller)]
public sealed class LoadYeller(IRepository<PlumbingEntity> repo, IEntityToVo<PlumbingEntity, Plumbing> eToVo) :
    LoadData<Plumbing, PlumbingEntity>(repo, eToVo, Source.Yeller)
{ }
