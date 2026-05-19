/*All Report*/
select 
    p.phonenumber as `Phone Number`, p.date as `Date of Message`, p.contents as `Message Contents`, p.source as `Message Source`,
    1 as `IM Lead`, 1 as `Sales Lead`, c.id as `Customer ID`, s.active as `Subscription is Active`, c.date as `Customer record start date`, c.unixcanceldate as `Customer cancel date Unix`,
    s.id as `Subscription Id`, s.complete as `Completed Initial`, s.value as `Contract Value`, s.date as `Subscription Start Date`, s.unixcanceldate as `Subscription cancel date Unix`,
    s.type as `Service Type`, 
    CASE 
        WHEN instr(p.metadata, 'Emails:') > 0 
        THEN substr(p.metadata, instr(p.metadata, 'Emails:') + 7)
        ELSE NULL
    END AS Email,
/*For debugging*/
    p.id as PlumbingId, 
from plumbingentities p
left join custardentities c on p.phonenumber in (c.phonenumber, c.phonenumber2)
left join sandentities s on s.custardid = c.id
where p.phonenumber > 0
order by p.id asc;