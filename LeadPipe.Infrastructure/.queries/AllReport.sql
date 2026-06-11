with sand as (
    select *,
        coalesce(s.unixcanceldate, 9223372036854775807)                        AS activeEnd,
        dense_rank() over (partition by s.custardid order by s.DateAddedDate)  AS sandRank
    from sandentities s
), classified as (
    select *,
        max(activeEnd) over (
            partition by custardid
            order by sandRank
            range between unbounded preceding and 1 preceding
        )                                                                      AS priorMaxActiveEnd
    from sand
), sub_status as (
    select *,
        case
            when priorMaxActiveEnd is not null and priorMaxActiveEnd >= unixdate then 'upgrade'
            when sandRank > 1                                                    then 'winback'
            else 'new_acquisition'
        end                                                                    AS status
    from classified
), rankedPlumbing as (
    select *,
        dense_rank() over (partition by phonenumber order by date asc)         AS plumbingRank
    from plumbingentities
    where phonenumber > 0
), cust_norm as (
    -- tall: one row per (customer, phone). Equijoin target, replaces IN (phone, phone2).
    select id as customerid, phonenumber  as phone10 from custardentities where phonenumber  > 0
    union
    select id as customerid, phonenumber2 as phone10 from custardentities where phonenumber2 > 0
)

-- /*
select
    p.phonenumber                                                              AS `Phone Number`,
    p.date                                                                     AS `Date of Message`,
    p.contents                                                                 AS `Message Contents`,
    p.source                                                                   AS `Message Source`,
    CASE WHEN p.unixdate < s.unixdate THEN 1 ELSE 0 END                        AS `IM Lead`,
    1                                                                          AS `Potential Sales Lead`,
    cn.customerid                                                              AS `Customer ID`,
    s.status                                                                   AS `Customer Status`,
    s.active                                                                   AS `Subscription is Active`,
    s.id                                                                       AS `Subscription Id`,
    s.complete                                                                 AS `Completed Initial`,
    s.value                                                                    AS `Contract Value`,
    s.date                                                                     AS `Subscription Start Date`,
    s.DateAddedDate                                                            AS `Subscription Added Date`,
    s.unixcanceldate                                                           AS `Subscription unix cxl date`,
    s.type                                                                     AS `Service Type`,
    CASE WHEN p.unixdate < s.unixdate AND s.complete = 1 THEN 1 ELSE 0 END     AS `Sale`,
    CASE
        WHEN instr(p.metadata, 'Emails:') > 0 THEN substr(p.metadata, instr(p.metadata, 'Emails:') + 7)
        WHEN instr(p.metadata, 'ID: ')    > 0 THEN substr(p.metadata, instr(p.metadata, 'ID: ') + 4)
        WHEN instr(p.metadata, 'Id: ')    > 0 THEN substr(p.metadata, instr(p.metadata, 'Id: ') + 4)
        ELSE p.metadata
    END                                                                        AS `Metadata`,
    -- For debugging
    p.id                                                                       AS `PlumbingId`,
    s.sandRank, p.plumbingRank, s.status
FROM rankedPlumbing AS p
JOIN cust_norm     AS cn ON cn.phone10  = p.phonenumber      -- equijoin, index-eligible
JOIN sub_status    AS s  ON s.custardid = cn.customerid
where p.plumbingRank = 1
    and s.complete   = 1
    and p.unixdate < s.unixdate
ORDER BY p.id ASC;
-- */

-- ============================================================
-- TOGGLE: total sales + contract value (uncomment to run; comment out the report above)
-- ============================================================
/*
select count(*) as total_sales, printf('%.2f', sum(s.value)) as total_contract_value
from rankedPlumbing p
left join custardentities c on p.phonenumber in (c.phonenumber, c.phonenumber2)
right join sub_status  s on s.custardid = c.id
where p.plumbingRank = 1
  and p.unixdate < s.unixdate           -- touch strictly before sale
  and s.complete  = 1;                  -- "Sale" condition
group by 
-- */

-- ============================================================
-- TOGGLE: by year, month, source, status  (touch-date grain = p.date)
-- ============================================================
 /*
select
    strftime('%Y', p.date)              as year,
    strftime('%m', p.date)              as month,
    p.source                            as source,
    s.status                            as status,
    count(*)                            as total_sales,
    printf('%.2f', sum(s.value))        as total_contract_value
from rankedPlumbing p
join cust_norm  cn ON cn.phone10  = p.phonenumber
join sub_status s  ON s.custardid = cn.customerid
where p.plumbingRank = 1
  and s.complete     = 1
  and p.unixdate < s.unixdate
group by year, month, source, status
order by year, month, source, status;
-- */