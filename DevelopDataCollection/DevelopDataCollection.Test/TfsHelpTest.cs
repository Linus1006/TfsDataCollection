using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DevelopDataCollection.Service;
using Microsoft.TeamFoundation.VersionControl.Client;
using DevelopDataCollection.Utility;
using Dapper;
using DevelopDataCollection.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace DevelopDataCollection.Test
{
    public class TfsHelpTest
    {

        [Fact(DisplayName = "測試檔案的修改紀錄(以大消金目錄開始找)沒有change紀錄!!!")]
        public void TestGetAllChangesets()
        {
            string path = "$/C150-us/Consumer/NCS/";
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");

            var changesets = tfsHelper.GetAllChangesets(path);
            var ncsFirstChangeId = changesets.OrderBy(n => n.ChangesetId).FirstOrDefault()?.ChangesetId;
            Assert.NotEmpty(changesets);
            Assert.Equal(3098, ncsFirstChangeId);

        }

        [Fact(DisplayName = "測試檔案的修改的詳細紀錄含change")]
        public void TestGetAllChangesetsDetail()
        {
            string path = "$/C150-us/Consumer/NCS/";
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var changesets = tfsHelper.GetAllChangesetsDetail(path, 100);

            Assert.NotEmpty(changesets);
            Assert.Equal(100, changesets.Count());

        }

        [Fact(DisplayName = "測試檔案於特定時間的Commit紀錄")]
        public void TestGetIntervalChangesets()
        {
            string path = "$/C150-us/Consumer/NCS/Branches/NCS.2/Consumer/NES/ESUN.Consumer.NES.Service/_Services/GuJaHandler.cs";
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");

            var changesets = tfsHelper.SearchChangesets(path, new DateTime(2018, 8, 1), new DateTime(2018, 8, 29));
            Assert.Equal(14, changesets.Count());
        }

        [Fact(DisplayName = "測試檔案於特定人的Commit紀錄")]
        public void TestGetUserChangesets()
        {
            string path = "$/C150-us/Consumer/NCS/";
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");

            var changesets = tfsHelper.SearchChangesets(path, new DateTime(2015, 1, 1), new DateTime(2018, 8, 29), "葉柏廷11964");
            Assert.Equal(94, changesets.Count());

        }

        [Theory(DisplayName = "測試檔案於特定的Commit變更的檔案List")]
        [InlineData(31284)]
        [InlineData(40421)]
        [InlineData(40508)]
        [InlineData(71126)]
        public void TestGetChanges(int value)
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var changes = tfsHelper.GetChanges(value);

            Assert.NotEmpty(changes);

        }

        [Fact(DisplayName = "測試單檔的變更歷程並找特定變更的前一版本")]
        public void TestItemChangsets()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");

            var a = tfsHelper.GetBeforeChangesetId(@"$/C150-us/Consumer/NCS/Branches/credit/ESUN.NCS.Contract/__ESUN.Platform/_Services/IAuthorizeHandler.cs", 21403);
            var b = tfsHelper.GetBeforeChangesetId(@"$/C150-us/Consumer/NCS/Branches/ALA/Consumer/MTM/ESUN.Consumer.MTM.Service/SystemNoticeMail.cs", 71279);
            var c = tfsHelper.GetBeforeChangesetId(@"$/C150-us/Consumer/NCS/Branches/credit/ESUN.NCS.Service/__ESUN.Platform/AuthorizeHandler.cs", 18685);
            var d = tfsHelper.GetBeforeChangesetId(@"$/C150-us/Consumer/NCS/Branches/credit/ESUN.NCS.Web.Backend/Areas/Credit/Controllers/SecurityRoleController.cs", 18685);
            Assert.Equal(19076, a);
            Assert.Null(b);
            Assert.Equal(18665, c);
            Assert.Null(d);
            //特例處理: 40421有刪除檔
            var e = tfsHelper.GetBeforeChangesetId(@"$/C150-us/Consumer/NCS/Trunk/Consumer/NCS/ESUN.Consumer.NCS.Contract/_Models/_Enums/CodeType.cs", 71126);
            Assert.Null(e);

        }

        [Fact(DisplayName = "測試比較單一檔案兩個版本的差異")]
        public void TestGetCodeCompare()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            string path = "$/C150-us/Consumer/NCS/Branches/credit/ESUN.NCS.Contract/__ESUN.Platform/_Services/IAuthorizeHandler.cs";
            var compareText = tfsHelper.GetCodeCompare(path, 19076, 19014);
            Assert.NotEmpty(compareText);
            var compareResult = tfsHelper.VssCodeCompareResult(compareText);
            Assert.Equal(3, compareResult.Change);
            Assert.Equal(8, compareResult.Insert);
            Assert.Equal(0, compareResult.Delete);

            compareResult = tfsHelper.VssCodeCompareResult(tfsHelper.GetCodeCompare(path, 25951, 19014));
            Assert.Equal(5, compareResult.Delete);

        }

        [Fact(DisplayName = "測試比較單一檔案新增的差異")]
        public void TestGetCodeCompareAdd()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            string path = "$/C150-us/Consumer/NCS/Branches/ALA/Consumer/MTM/ESUN.Consumer.MTM.Contract/_Models/ALA/Request/RequesBatchLog.cs";
            var compareText = tfsHelper.GetCodeCompare(path, 71130);
            Assert.NotEmpty(compareText);
            var compareResult = tfsHelper.VssCodeCompareResult(compareText);
            Assert.Equal(0, compareResult.Change);
            Assert.Equal(49, compareResult.Insert);
            Assert.Equal(0, compareResult.Delete);

            path = @"$/C150-us/Consumer/NCS/Branches/ALA/Consumer/MTM/ESUN.Consumer.MTM.Repository/Factory/LogFactory.cs";
            compareResult = tfsHelper.VssCodeCompareResult(tfsHelper.GetCodeCompare(path, 71130));
            compareText = tfsHelper.GetCodeCompare(path, 71130);
            compareResult = tfsHelper.VssCodeCompareResult(compareText);
            Assert.Equal(12, compareResult.Insert);

        }

        [Fact(DisplayName = "測試單一變更集")]
        public void TestTfsCommitData()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");

            var changesets = new List<Changeset>();
            changesets.Add(tfsHelper.SearchChangeset(71319));
            var datas = TfsDataCollection.GetChangesetInfo(tfsHelper, changesets);

            Assert.NotEmpty(datas);
            Assert.Equal(200, datas.First().InsertLines);
        }

        [Fact(DisplayName = "測試最新50筆資料收集")]
        public void TestTfsCommitDatas()
        {
            string path = "$/C150-us/Consumer/NCS/";
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var changesets = tfsHelper.GetAllChangesetsDetail(path, 50).OrderBy(n => n.ChangesetId);
            var datas = TfsDataCollection.GetChangesetInfo(tfsHelper, changesets);

            Assert.Equal(50, datas.Count());
        }

        [Fact(DisplayName = "測試取得Build資料")]
        public void TestTfsGetBuil()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var builds = tfsHelper.GetBuilds("C150-us-大消金");
            Assert.NotEmpty(builds);
        }

        [Fact(DisplayName = "測試取得Build Log")]
        public void TestTfsGetBuilLog()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var builds = tfsHelper.GetBuilds("C150-us-大消金").Where(n => n.StartTime > DateTime.Now.AddDays(-2));

            var a = builds.Select(n =>
            {
                return TfsDataCollection.GetBuildFile(n?.LogLocation);
            });

            Assert.Contains<int?>(711,a);
        }

    }
}
