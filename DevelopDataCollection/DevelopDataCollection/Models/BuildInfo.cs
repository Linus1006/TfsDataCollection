using Microsoft.TeamFoundation.Build.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevelopDataCollection.Models
{
    public class BuildInfo
    {
        public Guid Uid { get; set; }
        public BuildPhaseStatus TestStatus { get; set; }
        public string DropLocation { get; set; }
        public string LabelName { get; set; }
        public string LogLocation { get; set; }
        public string Quality { get; set; }
        public BuildPhaseStatus CompilationStatus { get; set; }
        public BuildStatus Status { get; set; }
        public string BuildControllerUri { get; set; }
        public string BuildDefinitionUri { get; set; }
        public bool BuildFinished { get; set; }
        public string LastChangedByDisplayName { get; set; }
        public string BuildNumber { get; set; }
        public string RequestedFor { get; set; }
        public string RequestedBy { get; set; }
        public long? ContainerId { get; set; }
        public string TeamProject { get; set; }
        public DateTime LastChangedOn { get; set; }
        public string Uri { get; set; }
        public DateTime StartTime { get; set; }
        public string SourceGetVersion { get; set; }
        public BuildReason Reason { get; set; }
        public DateTime FinishTime { get; set; }
        public string LastChangedBy { get; set; }
        public int? BuildFile { get; set; }

    }

    /*
     CREATE TABLE DevelopData.dbo.BuildInfo( 
        [Uid] [uniqueidentifier] NOT NULL, --Guid
        [TestStatus] [int], --此組建的測試階段的狀態
        [DropLocation] [nvarchar](MAX), --檔案位置
        [LabelName] [nvarchar](500), --標籤的名稱。
        [LogLocation] [nvarchar](MAX), --Log位置
        [Quality] [nvarchar](500), --此組建的品質
        [CompilationStatus] [int] NOT NULL, --此組建的編譯階段的狀態
        [Status] [int] NOT NULL, --組建的整體狀態
        [BuildControllerUri] [nvarchar](500), --此組建之組建控制器的 URI
        [BuildDefinitionUri] [nvarchar](500), --此組建的組建定義的 URI
        [BuildFinished] [bit], --指出是否已完成組建
        [LastChangedByDisplayName] [nvarchar](64), --取得最後一個使用者變更組建的顯示名稱
        [BuildNumber] [nvarchar](64), --此組建的編號
        [RequestedFor] [nvarchar](64), --這個組建的使用者
        [RequestedBy] [nvarchar](64), --要求的這個組建的對象的使用者。
        [ContainerId] [BIGINT], --取得與此 BuildDetail 相關聯容器 ContainerId
        [TeamProject] [nvarchar](500), --擁有此組建之 team 專案。
        [LastChangedOn] [datetime] NOT NULL, --取得日期和時間的這個組建的前次變更
        [Uri] [nvarchar](500), --此組建的 URI。
        [StartTime] [datetime] NOT NULL, --此組建實際啟動的時間
        [SourceGetVersion] [nvarchar](500), --此組建的已擷取其來源的版本規格
        [Reason] [int] NOT NULL, --組建所存在的原因。 如需有關使用這個屬性，請參閱 指定組建觸發程序和原因
        [FinishTime] [datetime] NOT NULL, --此組建完成的時間
        [LastChangedBy] [nvarchar](64), --最後一個使用者變更此組建
        [buildFile] [int] --Build的檔案數量
        )

        CREATE INDEX Uid on DevelopData.dbo.BuildInfo(Uid) 

    */

}