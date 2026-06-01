/*Yeller Corns*/
WITH rankedCorn AS (
    SELECT *, dense_rank() OVER (PARTITION BY phonenumber ORDER BY date ASC) AS cornRank
    FROM cornentities
    WHERE source like 'Sandbox' 
        /*and payload like '%yelp%'*/
),
sand AS (
    SELECT *,
        DENSE_RANK() OVER (
            PARTITION BY custardid
            ORDER BY date(datetime(date, '-7 hours')) ASC
        ) AS ranking
    FROM sandentities
)

select
	f.phonenumber as `Phone Number`,
	f.date as `Date of Message`,
	f.payload as `Message Contents`,
	f.metadata as `Message Source`,
	CASE WHEN f.unixdate < s.unixdate AND f.unixdate < c.unixdate THEN 1 ELSE 0 END AS `IM Lead`,
	1 as `Potential Sales Lead`,
	c.id as `Customer ID`, 
    s.active as `Subscription is Active`, 
    c.date as `Customer record start date`, 
    datetime(c.unixcanceldate / 1000, 'unixepoch') as `Customer unix cxl date`,
    s.id as `Subscription Id`, 
    s.complete as `Completed Initial`, 
    s.value as `Contract Value`, 
    s.date as `Subscription Start Date`, 
    datetime(s.unixcanceldate / 1000, 'unixepoch') as `Subscription unix cxl date`,
    s.type as `Service Type`,
    CASE WHEN f.unixdate < s.unixdate AND f.unixdate < c.unixdate AND s.complete = 1 THEN 1 ELSE 0 END AS `Sale`,

 /*For debugging*/
    f.id as `Corn Id`

from rankedCorn as f 
left join custardentities c on f.phonenumber in (c.phonenumber, c.phonenumber2)
left join sand s on s.custardid = c.id and `Ranking` = 1
where f.phonenumber > 0 and f.cornRank = 1;
