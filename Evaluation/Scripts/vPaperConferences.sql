USE [DBLP]
GO

/****** Object:  View [dbo].[v_Temp1]    Script Date: 05/24/2011 23:07:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].vPaperConference AS
SELECT ROW_NUMBER() OVER (ORDER BY p.Id) PaperId, ROW_NUMBER() OVER (ORDER BY r.Id) ConferenceId, p.title Paper, r.conference, r.title Proceeding, r.publisher, r.[year], dq.LoockupValue M1, dq.[Delay] M1_Delay
FROM papers p
INNER JOIN Raw_Links l
ON p.Id = l.[From]
INNER JOIN proceedings r
ON l.[To] = r.Id
AND l.[link-type]='in-proceedings'
INNER JOIN dbo.GetDQMetric() dq
ON dq.MetricId = 1 AND dq.LoockupKey = dbo.GetMetricLoockupId(p.Id, r.Id, null, null, null)
--WHERE 
--dq.LoockupValue=0 --AND
--r.conference = 'Information Hiding'


--delete from SimulatedMetricLoockup
GO


