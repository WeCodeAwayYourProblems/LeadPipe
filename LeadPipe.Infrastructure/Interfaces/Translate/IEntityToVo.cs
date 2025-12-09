namespace LeadPipe.Infrastructure.Interfaces.Translate;

public interface IEntityToVo<TEntity, TVo>
{
    TVo Translate(TEntity entity);
}
public interface IEntityToVo<TEntity1, TEntity2, TVo>
{
    TVo Translate(TEntity1 e1, TEntity2 e2);
}
