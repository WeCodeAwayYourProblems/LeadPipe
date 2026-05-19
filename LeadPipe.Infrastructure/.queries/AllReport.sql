/*All Report*/
select 
    p.phonenumber AS `Phone Number`, 
    p.date AS `Date of Message`, 
    p.contents AS `Message Contents`, 
    p.source AS `Message Source`,
    if(p.unixdate < s.unixdate AND p.unixdate < c.unixdate, 1, 0) AS `IM Lead`, 
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
    IF(p.unixdate < s.unixdate AND p.unixdate < c.unixdate AND s.active = 1, 1, 0) AS `Sale`,
    CASE 
        WHEN instr(p.metadata, 'Emails:') > 0 
            THEN substr(p.metadata, instr(p.metadata, 'Emails:') + 7)
        WHEN instr(p.metadata, 'ID: ') > 0 
            THEN substr(p.metadata, instr(p.metadata, 'ID: ') + 4)
        ELSE NULL
    END AS `Unique Item`,

/*For debugging*/
    p.id AS `PlumbingId`
FROM plumbingentities AS p
LEFT JOIN custardentities AS c ON p.phonenumber IN (c.phonenumber, c.phonenumber2)
LEFT JOIN sandentities AS s ON s.custardid = c.id AND s.active = 1
WHERE p.phonenumber > 0
ORDER BY p.id ASC;