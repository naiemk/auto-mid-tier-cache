SELECT tt.conference, tt.Good, tt.Total, 1.0*tt.Good/tt.Total "m(x)"
FROM
(select 
	conference,
	Good = (SELECT COUNT(*) FROM v_Temp1 t_1 WHERE t_1.conference = t.Conference AND LoockupValue = 1),
	Total = (SELECT COUNT(*) FROM v_Temp1 t_1 WHERE t_1.conference = t.Conference)
from v_Temp1 t
GROUP BY conference
) tt

select COUNT(DISTINCT [Conference]) from v_Temp1
