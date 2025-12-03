namespace LeadPipe.Infrastructure.Translate;

public interface IEntityToVo<TEntity, TVo>
{
    TVo Translate(TEntity entity);
}