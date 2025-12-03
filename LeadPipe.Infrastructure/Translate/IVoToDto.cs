namespace LeadPipe.Infrastructure.Translate;

public interface IVoToDto<TVo, TDto>
{
    TDto Translate(TVo data);
}