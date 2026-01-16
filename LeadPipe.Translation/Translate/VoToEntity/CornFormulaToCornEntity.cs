using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal sealed class CornFormulaToCornEntity : IVoToEntity<CornFormula, CornEntity>
{
    public CornEntity Translate(CornFormula c)
    {
        var result = new CornEntity()
        {
            Id = c.Id,
            PhoneNumber = c.PhoneNumber.Number,
            Date = c.Date.UtcDateTime,
            UnixDate = c.Date.ToUnixTimeSeconds(),
            Payload = c.PayLoad,
            MetaData = c.MetaData,
            Source = c.Source
        };
        return result;
    }
}
internal sealed class CornFormulaToCornMySqlEntity : IVoToEntity<CornFormula, CornMySqlEntity>
{

    public CornMySqlEntity Translate(CornFormula data)
    {
        // format form and referring
        List<string> split = [.. data.MetaData.Split(CornMySqlEntityTranslationHelper.Delimiter)];
        string form = string.Empty;
        string referring = string.Empty;
        foreach (var s in split)
        {
            if (s.Contains(CornMySqlEntityTranslationHelper.Form, StringComparison.InvariantCultureIgnoreCase))
                form = s.Replace(CornMySqlEntityTranslationHelper.FormValue, string.Empty);
            if (s.Contains(CornMySqlEntityTranslationHelper.Referring, StringComparison.InvariantCultureIgnoreCase))
                referring = s.Replace(CornMySqlEntityTranslationHelper.ReferringValue, string.Empty);
        }

        CornMySqlEntity result = new()
        {
            id = (int)data.Id,
            phoneNumber = data.PhoneNumber.Number.ToString(),
            timestamp = data.Date.UtcDateTime,
            comments = data.PayLoad,
            form = form,
            referringURL = referring
        };
        return result;
    }
}