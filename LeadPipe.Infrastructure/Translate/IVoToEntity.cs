namespace LeadPipe.Infrastructure.Translate;

public interface IVoToEntity<TVo, TEntity>
{
    TEntity Translate(TVo s);
}