
/****** Object:  Table [dbo].[DblpData]    Script Date: 03/05/2012 03:30:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DblpData](
	[id1] [bigint] NOT NULL,
	[id2] [bigint] NOT NULL,
	[year] [int] NULL,
	[venue] [nvarchar](150) NULL,
	[publisher] [nvarchar](200) NULL,
	[title] [nvarchar](700) NULL,
	[dq] [tinyint] NOT NULL,
 CONSTRAINT [PK_DblpData] PRIMARY KEY CLUSTERED 
(
	[id1] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[DblpData] ADD  CONSTRAINT [DF_DblpData_]  DEFAULT ((0)) FOR [dq]
GO



/****** Object:  StoredProcedure [dbo].[GenerateQueries]    Script Date: 03/05/2012 03:27:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[GenerateQueries]
	@d as float,
	@total as bigint
as
begin
	SET NOCOUNT ON;
	declare @rv table (year int, venue nvarchar(150), publisher nvarchar(200))
	declare @rvX table (year int, venue nvarchar(150), publisher nvarchar(200))
	declare @qCount bigint
	select @qCount = COUNT(*) from 
	(select count(*) cnt from DblpData
		group by year, venue, publisher) as source
	
	declare @sumSubSets bigint = 0, @countSubSets bigint = 0
	
	-- Select the first part from the unique queries
	insert into @rv 
	select distinct top (@total) [year], venue, publisher 
	from DblpData 
	
	-- Select the rest from y,v until new data covers d% of @rv
	declare @sumP int = 0
	declare c cursor forward_only
	for
	select year, venue, count(*) cnt from @rv group by [year], [venue] order by count(*) desc
		
	open c	
	declare @y int
	declare @v nvarchar(200)
	fetch next from c into @y, @v, @sumP
	
	while ((@@FETCH_STATUS = 0) and ((1.0*@sumSubSets/(1.0*@countSubSets+1.0*@total)) < @d))
	begin
		set @countSubSets = @countSubSets + 1
		set @sumSubSets = @sumSubSets + @sumP
		print @sumP
		print @sumSubsets
		print @countSubSets
		print ((1.0*@sumSubSets/(1.0*@countSubSets+1.0*@total)))
		print '---'
		insert into @rvX
		select @y, @v, null
		fetch next from c into @y, @v, @sumP
	end
	
	close c
	deallocate c
	
	insert into @rv
	select * from @rvX
	
	
	declare @cnt bigint
	declare @subs bigint
	select @cnt = COUNT(*) from @rv
	select @subs = COUNT(*) from 
		@rv r1 
		inner join @rv r2 on 
		r1.year = r2.year
		and r1.venue = r2.venue
		and r2.publisher IS NOT NULL
		and r1.publisher IS NULL
	select @subs "subset queries", @cnt "all queries", 1.0*@subs/@cnt "coverage"
	
	select [year], [venue], [publisher], 
		cost = case when publisher is null then
			(select count(*) from DblpData dd where dd.[year]= d.[year] and dd.[venue]=d.[venue])
		else
			(select count(*) from DblpData dd where dd.[year]= d.[year] and dd.[venue]=d.[venue] and dd.[publisher]=d.[publisher])
		end,
		popularity = 1
	from @rv d
end

GO


