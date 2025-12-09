namespace LeadPipe.Infrastructure.Interfaces.Translate;

public interface IVoToEntity<TVo, TEntity>
{
    TEntity Translate(TVo s);
}

public interface IVoToEntity<TVo, TEntity1, TEntity2>
{
    (TEntity1, TEntity2) Translate(TVo s);
}
