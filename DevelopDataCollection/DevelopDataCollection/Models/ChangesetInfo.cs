using Microsoft.TeamFoundation.VersionControl.Client;
using System;

namespace DevelopDataCollection.Models
{
    public class ChangesetInfo
    {
        public int ChangesetId { get; set; }
        public string Committer { get; set; }
        public string CommitterDisplayName { get; set; }        
        public string Comment { get; set; }
        public DateTime CreationDate { get; set; }
        public ChangeType ChangeType { get; set; }
        public int ChangeFiles { get; set; }
        public int ChangeLines { get; set; }
        public int InsertLines { get; set; }
        public int DeleteLines { get; set; }

    }

    /*
     CREATE TABLE DevelopData.dbo.ChangesetInfo( 
        [ChangesetId] [int] NOT NULL, --變更集序號 Index
        [Committer] [nvarchar](64) NOT NULL, --Commit者oant帳號
        [CommitterDisplayName] [nvarchar](32) NOT NULL, --變更者姓名
        [Comment] [nvarchar](MAX), --變更說明
        [CreationDate] [datetime] NOT NULL, --日期
        [ChangeType] [int] NOT NULL, --變更種類 [Flag]
        [ChangeFiles] [int] NOT NULL, --變更種類
        [ChangeLines] [int] NOT NULL, --變更行數
        [InsertLines] [int] NOT NULL, --新增行數
        [DeleteLines] [int] NOT NULL, --刪除行數

        )

        CREATE INDEX ChangesetId on DevelopData.dbo.ChangesetInfo(ChangesetId) 

    */
}