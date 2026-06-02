/*All Report*/
with sand as (
    /* sqlite doesn't have a timezone database, but since we're stripping the time out anyway, it doesn't actually matter
             * date() strips the time out of the datetime value*/
    select *, dense_rank() over (partition by custardid order by date(datetime(s.date, '-7 hours'))) as sandRank
    from sandentities s
), rankedPlumbing as (
    select *, dense_rank() over (partition by phonenumber order by date asc) as plumbingRank
    from plumbingentities
    where phonenumber > 0
)

select 
    p.phonenumber AS `Phone Number`, 
    p.date AS `Date of Message`, 
    p.contents AS `Message Contents`, 
    p.source AS `Message Source`,
    /*The plumbing is before the sand */
    CASE WHEN p.unixdate < s.unixdate /*AND p.unixdate < c.unixdate*/ THEN 1 ELSE 0 END AS `IM Lead`, 
    1 AS `Potential Sales Lead`, 
    c.id AS `Customer ID`, 
    s.active AS `Subscription is Active`, 
    c.date AS `Customer record start date`, 
    c.unixcanceldate AS `Customer unix cxl date`,
    s.id AS `Subscription Id`, 
    s.complete AS `Completed Initial`, 
    s.value AS `Contract Value`, 
    s.date AS `Subscription Start Date`, 
    s.unixcanceldate AS `Subscription unix cxl date`,
    s.type AS `Service Type`, 
    /*The plumbing is before the sand and the sand is complete */
    CASE WHEN p.unixdate < s.unixdate /*AND p.unixdate < c.unixdate*/ AND s.complete = 1 THEN 1 ELSE 0 END AS `Sale`,
    CASE 
        WHEN instr(p.metadata, 'Emails:') > 0 
            THEN substr(p.metadata, instr(p.metadata, 'Emails:') + 7)
        WHEN instr(p.metadata, 'ID: ') > 0 
            THEN substr(p.metadata, instr(p.metadata, 'ID: ') + 4)
        WHEN instr(p.metadata, 'Id: ') > 0 
            THEN substr(p.metadata, instr(p.metadata, 'Id: ') + 4)
        ELSE p.metadata
    END AS `Metadata`,

/*For debugging*/
    p.id AS `PlumbingId`,
    sandRank, p.plumbingRank
FROM rankedPlumbing AS p
LEFT JOIN custardentities AS c ON p.phonenumber IN (c.phonenumber, c.phonenumber2)
RIGHT JOIN sand AS s ON s.custardid = c.id /* Right join ensures that no customers without a subscription are shown*/
WHERE p.plumbingRank = 1
ORDER BY p.id ASC;
