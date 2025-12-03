namespace LeadPipe.Infrastructure.Translate;

public interface IDtoToVo<TDto, TVo>
{
    TDto Translate(TVo data);
}
