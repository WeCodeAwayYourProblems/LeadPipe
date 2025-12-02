using Google.Protobuf.WellKnownTypes;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Translate;
using System;

namespace LeadPipe.Translation.Translate;

internal class VoToEntity : IVoToEntity
{
    public SubsEntity Translate(Sandwich s)
    {
        var result = new SubsEntity()
        {
            Id = s.SubscriptionId,
            CustomerId = s.CustomerId,
            Date = new(s.Date.Ticks),
            UnixDate = s.Date.ToUnixTimeSeconds(),
            SubDate = new(s.SubDate.Ticks),
            UnixSubDate = s.SubDate.ToUnixTimeSeconds(),
            Number = s.Number.Number,
            Number2 = s.Number2.Number,
            CancelDate = new(s.CancelDate.Ticks),
            UnixCancelDate = s.CancelDate.ToUnixTimeSeconds(),
            SubCancelDate = new(s.SubCancelDate.Ticks),
            UnixSubCancelDate = s.SubCancelDate.ToUnixTimeSeconds(),
            Active = s.Active,
            SubActive = s.SubActive,
            Complete = s.Complete,
            Value = s.Value,
            Seller = s.Seller,
            Seller2 = s.Seller2,
            Seller3 = s.Seller3
        };
        return result;
    }

    public PlumbingEntity Translate(Plumbing plumbing)
    {
        var result = new PlumbingEntity()
        {
            PhoneNumber = plumbing.PhoneNumber.Number,
            Date = new(plumbing.Date.Ticks),
            UnixDate = plumbing.Date.ToUnixTimeSeconds(),
            Contents = plumbing.Contents,
            Source = plumbing.Source,
        };
        return result;
    }

    public CallEntity Translate(Call c)
    {
        var result = new CallEntity()
        {
            PhoneNumber = c.Number.Number,
            CallDate = new(c.Date.Ticks),
            UnixCallDate = c.Date.ToUnixTimeSeconds(),
            Note = c.Note,
            Source = c.Source,
            Duration = c.Duration.Seconds,
            Billable = c.Billable
        };
        return result;
    }
}
