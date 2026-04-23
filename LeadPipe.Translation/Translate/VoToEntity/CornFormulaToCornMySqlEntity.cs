using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

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