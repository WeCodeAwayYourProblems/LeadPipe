namespace LeadPipe.Infrastructure.Interfaces.Translate;

public interface IVoToDto<TVo, TDto>
{
    TDto Translate(TVo data);
}
