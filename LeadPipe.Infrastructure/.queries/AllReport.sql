/*All Report*/
with sand as (
    /* Rank subscriptions per customer by stored Pacific calendar date (DateAddedDate).
       Same-day subscriptions share a sandRank (date grain) — matches MySQL's dadd_day,
       so same-day siblings never count as "prior" to each other.
       activeEnd: cancel in unix seconds; NULL cancel = still active = max sentinel.
       (0000-00-00 / bad-date sentinels were already nulled at ingestion in SandwichToSandEntity.) */
    select *,
        coalesce(s.unixcanceldate, 9223372036854775807) as activeEnd,
        dense_rank() over (partition by s.custardid order by s.DateAddedDate) as sandRank
    from sandentities s
), classified as (
    /* priorMaxActiveEnd = latest active-end among STRICTLY EARLIER ranks.
       RANGE ... 1 PRECEDING over the integer sandRank includes only rows whose
       sandRank <= current-1, so same-day siblings (equal sandRank) are excluded.
       This is the exact-parity equivalent of MySQL's prev.dadd_day < curr.dadd_day. */
    select *,
        max(activeEnd) over (
            partition by custardid
            order by sandRank
            range between unbounded preceding and 1 preceding
        ) as priorMaxActiveEnd
    from sand
), sub_status as (
    /* new_acquisition: no earlier subscription (sandRank = 1)
       upgrade        : an earlier sub was still active at this one's start  (MySQL prior_active)
       winback        : an earlier sub existed but had ended                 (MySQL prior_ever only) */
    select *,
        case
            when priorMaxActiveEnd is not null and priorMaxActiveEnd >= unixdate then 'upgrade'
            when sandRank > 1                                                    then 'winback'
            else 'new_acquisition'
        end as status
    from classified
), rankedPlumbing as (
    select *, dense_rank() over (partition by phonenumber order by date asc) as plumbingRank
    from plumbingentities
    where phonenumber > 0
)

-- ============================================================
-- LINE-BY-LINE REPORT (active select)
-- ============================================================
-- /*
select
    p.phonenumber                                                          AS `Phone Number`,
    p.date                                                                 AS `Date of Message`,
    p.contents                                                             AS `Message Contents`,
    p.source                                                               AS `Message Source`,
    -- The plumbing is before the sand
    CASE WHEN p.unixdate < s.unixdate THEN 1 ELSE 0 END                    AS `IM Lead`,
    1                                                                      AS `Potential Sales Lead`,
    c.id                                                                   AS `Customer ID`,
    s.status                                                               AS `Customer Status`,
    s.active                                                               AS `Subscription is Active`,
    c.date                                                                 AS `Customer record start date`,
    c.unixcanceldate                                                       AS `Customer unix cxl date`,
    s.id                                                                   AS `Subscription Id`,
    s.complete                                                             AS `Completed Initial`,
    s.value                                                                AS `Contract Value`,
    s.date                                                                 AS `Subscription Start Date`,
    s.DateAddedDate                                                        AS `Subscription Added Date`,
    s.unixcanceldate                                                       AS `Subscription unix cxl date`,
    s.type                                                                 AS `Service Type`,
    -- The plumbing is before the sand and the sand is complete
    CASE WHEN p.unixdate < s.unixdate AND s.complete = 1 THEN 1 ELSE 0 END AS `Sale`,
    CASE
        WHEN instr(p.metadata, 'Emails:') > 0
            THEN substr(p.metadata, instr(p.metadata, 'Emails:') + 7)
        WHEN instr(p.metadata, 'ID: ') > 0
            THEN substr(p.metadata, instr(p.metadata, 'ID: ') + 4)
        WHEN instr(p.metadata, 'Id: ') > 0
            THEN substr(p.metadata, instr(p.metadata, 'Id: ') + 4)
        ELSE p.metadata
    END                                                                    AS `Metadata`,
    -- For debugging
    p.id                                                                   AS `PlumbingId`,
    s.sandRank, p.plumbingRank, s.status
FROM rankedPlumbing AS p
LEFT JOIN custardentities AS c ON p.phonenumber IN (c.phonenumber, c.phonenumber2)
RIGHT JOIN sub_status AS s ON s.custardid = c.id -- Right join: keep subs even with no touch
WHERE p.plumbingRank = 1
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
left join custardentities c on p.phonenumber in (c.phonenumber, c.phonenumber2)
right join sub_status  s on s.custardid = c.id
where p.plumbingRank = 1
  and p.unixdate < s.unixdate
  and s.complete  = 1
group by year, month, source, status
order by year, month, source, status;
-- */