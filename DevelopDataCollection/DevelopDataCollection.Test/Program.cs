using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DevelopDataCollection.Service;


namespace DevelopDataCollection.Test
{
    class Program
    {
        /// <summary>
        /// 每日執行批次 紀錄build的資料
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var tfsHelper = new TfsSoapHelper(@"http://tfsa0w0p01:8080/tfs/C105", "C150-us");
            var builds = tfsHelper.GetBuilds("C150-us-大消金").Where(n => n.StartTime < DateTime.Now && n.StartTime >= DateTime.Now.AddDays(-1));
            var buildInfos = TfsDataCollection.IBuildDetailToBuildInfo(builds);
            TfsDataCollection.SaveBuildInfos(buildInfos);
        }
    }
}
