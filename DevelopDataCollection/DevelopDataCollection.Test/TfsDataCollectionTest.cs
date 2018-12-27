using DevelopDataCollection.Models;
using DevelopDataCollection.Service;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace DevelopDataCollection.Test
{
    public class TfsDataCollectionTest
    {

        [Fact(DisplayName = "測試單一資料塞DB")]
        public void TestInsertChangesetInfo2Db()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            string path = "$/C150-us/Consumer/NCS/";
            //單筆測試
            var changesets = new List<Changeset>();
            changesets.Add(tfsHelper.SearchChangeset(71543));

            //取得相關變更集資料
            var datas = TfsDataCollection.GetChangesetInfo(tfsHelper, changesets);
            //塞資料庫           
            TfsDataCollection.SaveChangesetInfos(datas);
        }

        [Fact(DisplayName = "測試平行將開發資料資料塞DB-2018/09/04 18:00")]
        public void TestPipelineInsertChangesetInfo2Db()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            string path = "$/C150-us/Consumer/NCS/";
            //測試50筆
            //var changesets = tfsHelper.GetAllChangesetsDetail(path, 50).OrderBy(n => n.ChangesetId);
            //測試全部
            //var changesets = tfsHelper.GetAllChangesetsDetail(path);
            var changesets = tfsHelper.GetAllChangesets(path).OrderBy(n => n.ChangesetId); //減少一次取得的資料量

            Thread t0 = new Thread(InsertChangesetInfoToDb);
            Thread t1 = new Thread(InsertChangesetInfoToDb);
            Thread t2 = new Thread(InsertChangesetInfoToDb);
            Thread t3 = new Thread(InsertChangesetInfoToDb);
            Thread t4 = new Thread(InsertChangesetInfoToDb);
            Thread t5 = new Thread(InsertChangesetInfoToDb);
            Thread t6 = new Thread(InsertChangesetInfoToDb);
            Thread t7 = new Thread(InsertChangesetInfoToDb);
            Thread t8 = new Thread(InsertChangesetInfoToDb);

            //切變更集群
            var changesets0 = changesets.Where(n => n.ChangesetId < 10000);
            var changesets1 = changesets.Where(n => n.ChangesetId >= 10000 && n.ChangesetId < 20000);
            var changesets2 = changesets.Where(n => n.ChangesetId >= 20000 && n.ChangesetId < 30000);
            var changesets3 = changesets.Where(n => n.ChangesetId >= 30000 && n.ChangesetId < 40000);
            var changesets4 = changesets.Where(n => n.ChangesetId >= 40000 && n.ChangesetId < 50000);
            var changesets5 = changesets.Where(n => n.ChangesetId >= 50000 && n.ChangesetId < 60000);
            var changesets6 = changesets.Where(n => n.ChangesetId >= 60000 && n.ChangesetId < 70000);
            var changesets7 = changesets.Where(n => n.ChangesetId >= 70000 );

            //t0.Start(changesets0);
            //t1.Start(changesets1);
            //t2.Start(changesets2);
            //t3.Start(changesets3);
            //t4.Start(changesets4);
            //t5.Start(changesets5);
            //t6.Start(changesets6);
            //t7.Start(changesets7);

            //t0.Join();
            //t1.Join();
            //t2.Join();
            //t3.Join();
            //t4.Join();
            //t5.Join();
            //t6.Join();
            //t7.Join();
        }

        private static void InsertChangesetInfoToDb(object param)
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var changesets = param as IEnumerable<Changeset>;

            //取得相關變更集資料
            var datas = TfsDataCollection.GetChangesetInfo(tfsHelper, changesets);
            //塞資料庫
            TfsDataCollection.SaveChangesetInfos(datas);
        }


        [Fact(DisplayName = "測試取得Build並轉換type")]
        public void TestIBuildDetailToBuildInfo()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var builds = tfsHelper.GetBuilds("C150-us-大消金").Where(n => n.StartTime > DateTime.Now.AddDays(-2));
            var buildInfos = TfsDataCollection.IBuildDetailToBuildInfo(builds);
            Assert.NotNull(buildInfos.First().Uri);
        }

        [Fact(DisplayName = "測試每日取得當日build資料並塞db")]
        public void TestInsertBuildInfoToDb()
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var builds = tfsHelper.GetBuilds("C150-us-大消金").Where(n => n.StartTime < new DateTime(2018, 9, 5)).OrderBy(m => m.StartTime);
            var buildInfos = TfsDataCollection.IBuildDetailToBuildInfo(builds);
            TfsDataCollection.SaveBuildInfos(buildInfos);

        }

    }
}
