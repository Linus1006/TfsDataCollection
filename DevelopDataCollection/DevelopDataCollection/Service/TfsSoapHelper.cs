using DevelopDataCollection.Models;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DevelopDataCollection.Service
{
    public class TfsSoapHelper
    {
        string _tfsUri;
        string _project;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="tfsUri">tfsURI</param>
        /// <param name="project"></param>
        public TfsSoapHelper(string tfsUri, string project)
        {
            _project = project;
            _tfsUri = tfsUri;
        }

        /// <summary>
        /// 取得路徑下的全部的變更集(沒有change細項 需使用GetChanges查詢 較省單次時間)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<Changeset> GetAllChangesets(string path)
        {
            using (TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(this._tfsUri)))
            {
                VersionControlServer vcServer = tpc.GetService<VersionControlServer>();
                return vcServer.QueryHistory(path, RecursionType.Full).ToList();
            }
        }

        /// <summary>
        /// 取得路徑下的全部的詳細變更集(含changesets)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public List<Changeset> GetAllChangesetsDetail(string path = @"$/", int maxCount = Int32.MaxValue)
        {
            using (TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(this._tfsUri)))
            {
                VersionControlServer vcServer = tpc.GetService<VersionControlServer>();

                var history = vcServer.QueryHistory(
                    path,
                    VersionSpec.Latest,
                    0,
                    RecursionType.Full,
                    null,
                    null,
                    null,
                    maxCount,
                    true,
                    false) as IEnumerable<Changeset>;

                return history.ToList();
            }
        }

        /// <summary>
        /// 取得特定時間或使用者的commit紀錄
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startInterval"></param>
        /// <param name="endInterval"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<Changeset> SearchChangesets(string path, DateTime startInterval, DateTime endInterval, string username = null)
        {
            using (TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(this._tfsUri)))
            {
                VersionControlServer vcServer = tpc.GetService<VersionControlServer>();
                DateVersionSpec versionFrom = new DateVersionSpec(startInterval);
                VersionSpec versionTo = new DateVersionSpec(endInterval);

                var history = vcServer.QueryHistory(
                    path,
                    VersionSpec.Latest,
                    0,
                    RecursionType.Full,
                    username,
                    versionFrom,
                    versionTo,
                    Int32.MaxValue,
                    true,
                    false) as IEnumerable<Changeset>;

                return history.ToList();
            }
        }

        /// <summary>
        /// 取得特定變更集
        /// </summary>
        /// <param name="path"></param>
        /// <param name="changesetId"></param>
        /// <returns></returns>
        public Changeset SearchChangeset(int changesetId)
        {
            using (TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(this._tfsUri)))
            {
                var vcServer = tpc.GetService<VersionControlServer>();
                return vcServer.GetChangeset(changesetId);            
            }
        }

        /// <summary>
        /// 取得變更的項目
        /// </summary>
        /// <param name="changesetId"></param>
        /// <returns></returns>
        public List<Change> GetChanges(int changesetId)
        {
            using (TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(this._tfsUri)))
            {
                var vcServer = tpc.GetService<VersionControlServer>();
                var changeset = vcServer.GetChangeset(changesetId);
                return changeset.Changes.ToList();
            }
        }


        /// <summary>
        /// 取得前一版本的變更集
        /// </summary>
        /// <param name="itemPath">物件路徑,傳入前請先確認為檔案</param>
        /// <param name="changesetId"></param>
        /// <returns>null表示無效</returns>
        public int? GetBeforeChangesetId(string itemPath, int changesetId)
        {
            var changesets = this.GetAllChangesetsDetail(itemPath);
            
            var endChangeset = changesets.Where(n => n.ChangesetId == changesetId).First();
            var startChangesets = changesets.Where(n => n.ChangesetId < changesetId).OrderByDescending(m => m.ChangesetId);
            bool endChangesetIsEdit = (endChangeset.Changes.First().ChangeType & ChangeType.Edit) == ChangeType.Edit;
            bool endChangesetIsAdd = (endChangeset.Changes.First().ChangeType & ChangeType.Add) == ChangeType.Add;

            if (endChangesetIsEdit && !endChangesetIsAdd)   //防呆而已..
            {
                foreach (var changeset in startChangesets)
                {
                    var changeType = changeset.Changes.First().ChangeType;
                    if (changeType >= ChangeType.Rename)
                        continue;
                    return changeset.ChangesetId;
                }
            }
            return null;
        }

        /// <summary>
        /// 比較單一檔案兩個版本的差異(新增也可以)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="afterChangesetId"></param>
        /// <param name="beforeChangesetId"></param>
        public string GetCodeCompare(string path, int afterChangesetId, int beforeChangesetId = 0)
        {

            using (var projectCollection = new TfsTeamProjectCollection(new Uri(this._tfsUri)))
            {
                projectCollection.EnsureAuthenticated();
                var vcServer = (VersionControlServer)projectCollection.GetService(typeof(VersionControlServer));
                                
                var after = new DiffItemVersionedFile(vcServer, path, VersionSpec.ParseSingleSpec(afterChangesetId.ToString(), null));
                IDiffItem before;
                if (beforeChangesetId != 0)
                    before = new DiffItemVersionedFile(vcServer, path, VersionSpec.ParseSingleSpec(beforeChangesetId.ToString(), null));
                else
                    before = new DiffItemLocalFile("", Encoding.UTF8.CodePage, DateTime.Now, false);

                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    var options = new DiffOptions();
                    options.Flags = DiffOptionFlags.EnablePreambleHandling;
                    options.OutputType = DiffOutputType.VSS;
                    options.TargetEncoding = Encoding.UTF8;
                    options.SourceEncoding = Encoding.UTF8;
                    options.StreamWriter = writer;
                    Difference.DiffFiles(vcServer, before, after, options, path, true);
                    writer.Flush();

                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        /// <summary>
        /// 回傳code compare結果
        /// </summary>
        /// <param name="codeCompareText"></param>
        /// <returns></returns>
        public CodeCompare VssCodeCompareResult(string codeCompareText)
        {
            int change = 0;
            int delete = 0;
            int insert = 0;

            foreach (Match match in Regex.Matches(codeCompareText, "Change:")) change++;
            foreach (Match match in Regex.Matches(codeCompareText, "Del:")) delete++;
            foreach (Match match in Regex.Matches(codeCompareText, "Ins:")) insert++;

            return new CodeCompare() { Delete = delete, Change = change, Insert = insert };
        }

        /// <summary>
        /// 取得Build詳細資料
        /// </summary>
        /// <param name="definitionName">組件定義</param>
        /// <returns></returns>
        public List<IBuildDetail> GetBuilds(string definitionName)
        {
            using (TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(this._tfsUri)))
            {
                IBuildServer buildServer = tpc.GetService<IBuildServer>();
                var details = buildServer.QueryBuilds("C150-us", $@"{definitionName}*") as IEnumerable<IBuildDetail>;
                return details.ToList();
            }

        }

        
    }



}