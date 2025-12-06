namespace LeadPipe.Infrastructure.Interfaces.Translate;

public interface IDtoToVo<TDto, TVo>
{
    TVo Translate(TDto data);
}
