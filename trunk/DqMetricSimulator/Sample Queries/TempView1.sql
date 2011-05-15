USE [DBLP]
GO

/****** Object:  View [dbo].[v_Temp1]    Script Date: 04/10/2011 17:56:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_Temp1] AS
SELECT p.Id Id1, r.Id Id2, p.title Paper, r.conference, r.title Proceeding, r.publisher, r.[year], dq.LoockupValue, dq.[Delay]
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


