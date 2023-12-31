CREATE TABLE TB_Diary
(
	ID int IDENTITY(1,1),
	CategoryID int not null,
	EventInformation varchar(max),
	RecordDate datetime
)

GO

CREATE TABLE TB_Category
(
	ID int IDENTITY(1,1),
	CategName varchar(80) not null
)

GO

ALTER TABLE TB_DIARY ADD CONSTRAINT PK_DIARY PRIMARY KEY (ID DESC)
ALTER TABLE TB_Category ADD CONSTRAINT PK_CATEGORY PRIMARY KEY (ID DESC)

GO

CREATE NONCLUSTERED INDEX IDX_DIARY ON [dbo].[TB_Diary]
(
	[CategoryID]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO