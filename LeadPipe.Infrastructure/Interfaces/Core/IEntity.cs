using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Interfaces.Core;

public interface IEntity
{
    long Id { get; set; }
}
public interface IHasUnixMatchDate
{
    long UnixMatchDate { get; set; }
}
public interface IPhoneDateIdEntity: IEntity, IPhoneEntity
{
    long UnixDate { get; set; }
}
public interface IPhoneEntity
{
    PhoneNumber PhoneNumber { get; set; }

}