using DevelopDataCollection.Models;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using DevelopDataCollection.Utility;
using Dapper;
using Microsoft.TeamFoundation.Build.Client;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace DevelopDataCollection.Service
{
    public class TfsDataCollection
    {
        //整合測試的程式修改
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static List<ChangesetInfo> GetChangesetInfo(TfsSoapHelper tfsHelper, IEnumerable<Changeset> changesets)
        {

            List<ChangesetInfo> datas = new List<ChangesetInfo>();

            foreach (var changeset in changesets)
            {
                try
                {
                    //每個Changeset
                    var data = new ChangesetInfo()
                    {
                        ChangesetId = changeset.ChangesetId,
                        Committer = changeset.Committer,
                        CommitterDisplayName = changeset.CommitterDisplayName,
                        Comment = changeset.Comment,
                        CreationDate = changeset.CreationDate,
                        ChangeFiles = 0,
                        ChangeLines = 0,
                        InsertLines = 0,
                        DeleteLines = 0
                    };

                    List<Change> changes = new List<Change>();
                    if (!changeset.Changes.Any())
                        changes = tfsHelper.GetChanges(changeset.ChangesetId);
                    else
                        changes = changeset.Changes.ToList();

                    foreach (var change in changes)
                    {

                        try
                        {
                            //每個change file 
                            var isFile = change.Item.ItemType == ItemType.File;
                            var isAdd = (change.ChangeType & ChangeType.Add) == ChangeType.Add;
                            var isEdit = (change.ChangeType & ChangeType.Edit) == ChangeType.Edit;

                            if (isFile)
                            {
                                data.ChangeFiles++;
                                data.ChangeType |= change.ChangeType;   //update ChangeType
                                if (isAdd && change.ChangeType < ChangeType.Rename)
                                {
                                    var text = tfsHelper.GetCodeCompare(change.Item.ServerItem, changeset.ChangesetId);
                                    var tempResult = tfsHelper.VssCodeCompareResult(text);
                                    data.ChangeLines += tempResult.Change;
                                    data.DeleteLines += tempResult.Delete;
                                    data.InsertLines += tempResult.Insert;

                                }
                                else if (isEdit && change.ChangeType < ChangeType.Rename)
                                {
                                    var before = tfsHelper.GetBeforeChangesetId(change.Item.ServerItem, changeset.ChangesetId);
                                    var text = tfsHelper.GetCodeCompare(change.Item.ServerItem, changeset.ChangesetId, (before == null) ? 0 : before.Value);
                                    var tempResult = tfsHelper.VssCodeCompareResult(text);
                                    data.ChangeLines += tempResult.Change;
                                    data.DeleteLines += tempResult.Delete;
                                    data.InsertLines += tempResult.Insert;
                                }
                                else
                                {
                                    //TODO 刪除 一堆阿沙布魯的
                                }

                            }
                            //TODO 非檔案 不做計算
                        }
                        catch(Exception e)
                        {
                            //Log處理
                            _logger.Trace($@"變更集:{changeset.ChangesetId} 目標:{change.Item.ServerItem} 計算失敗-{e.ToString()}");
                        }
                    }

                    datas.Add(data);
                    //Log處理
                    _logger.Trace($@"變更集:{changeset.ChangesetId}完成");
                }
                catch(Exception e)
                {
                    //Log處理
                    _logger.Trace($@"變更集:{changeset.ChangesetId}失敗-{e.ToString()}");
                }
            }

            return datas;

        }

        /// <summary>
        /// 將變更集詳細資料塞進DB
        /// </summary>
        /// <param name="changesetInfos"></param>
        public static void SaveChangesetInfos(IEnumerable<ChangesetInfo> changesetInfos)
        {

            var sqlStr = @"INSERT INTO [dbo].[ChangesetInfo]
                        ([ChangesetId]
                        ,[Committer]
                        ,[CommitterDisplayName]
                        ,[Comment]
                        ,[CreationDate]
                        ,[ChangeType]
                        ,[ChangeFiles]
                        ,[ChangeLines]
                        ,[InsertLines]
                        ,[DeleteLines])
                    VALUES
                        (@ChangesetId
                        ,@Committer
                        ,@CommitterDisplayName
                        ,@Comment
                        ,@CreationDate
                        ,@ChangeType
                        ,@ChangeFiles
                        ,@ChangeLines
                        ,@InsertLines
                        ,@DeleteLines)";

            using (var conn = DbHelper.OpenConnection("DevelopData"))
            {
                conn.Execute(sqlStr, changesetInfos);
            }
        }

        /// <summary>
        /// 將build詳細資料塞進DB
        /// </summary>
        /// <param name="buildInfos"></param>
        public static void SaveBuildInfos(IEnumerable<BuildInfo> buildInfos)
        {

            var sqlStr = @"INSERT INTO [dbo].[BuildInfo]
                        ([Uid]
                        ,[TestStatus]
                        ,[DropLocation]
                        ,[LabelName]
                        ,[LogLocation]
                        ,[Quality]
                        ,[CompilationStatus]
                        ,[Status]
                        ,[BuildControllerUri]
                        ,[BuildDefinitionUri]
                        ,[BuildFinished]
                        ,[LastChangedByDisplayName]
                        ,[BuildNumber]
                        ,[RequestedFor]
                        ,[RequestedBy]
                        ,[ContainerId]
                        ,[TeamProject]
                        ,[LastChangedOn]
                        ,[Uri]
                        ,[StartTime]
                        ,[SourceGetVersion]
                        ,[Reason]
                        ,[FinishTime]
                        ,[LastChangedBy]
                        ,[BuildFile])
                        VALUES
                        (
                        newid()
                        ,@TestStatus
                        ,@DropLocation
                        ,@LabelName
                        ,@LogLocation
                        ,@Quality
                        ,@CompilationStatus
                        ,@Status
                        ,@BuildControllerUri
                        ,@BuildDefinitionUri
                        ,@BuildFinished
                        ,@LastChangedByDisplayName
                        ,@BuildNumber
                        ,@RequestedFor
                        ,@RequestedBy
                        ,@ContainerId
                        ,@TeamProject
                        ,@LastChangedOn
                        ,@Uri
                        ,@StartTime
                        ,@SourceGetVersion
                        ,@Reason
                        ,@FinishTime
                        ,@LastChangedBy
                        ,@BuildFile
                        )";


            using (var conn = DbHelper.OpenConnection("DevelopData"))
            {
                conn.Execute(sqlStr, buildInfos);
            }
        }

        public static List<BuildInfo> IBuildDetailToBuildInfo(IEnumerable<IBuildDetail> builds){

            List<BuildInfo> buildInfos = new List<BuildInfo>();
            var typeIBuildDetail = builds.First().GetType();
            var typeBuildInfo = typeof(BuildInfo);
            var PropertyInfoList = typeBuildInfo.GetProperties();

            foreach (var build in builds)
            {
                var ob1 = Activator.CreateInstance(typeBuildInfo); // 使用類型實例化
                foreach (var propertyInfo in PropertyInfoList)
                {

                    PropertyInfo typePropertyInfoTarget = typeBuildInfo.GetProperty(propertyInfo.Name); // 依據屬性名稱取得屬性的中繼資料
                    PropertyInfo typePropertyInfoSource = typeIBuildDetail.GetProperty(propertyInfo.Name); // 依據屬性名稱取得source屬性的中繼資料
                    
                    if (propertyInfo.Name == "Uid") continue;
                    if (propertyInfo.Name == "Uri" || propertyInfo.Name == "BuildControllerUri" || propertyInfo.Name == "BuildDefinitionUri")
                    {
                        typePropertyInfoTarget.SetValue(ob1, typePropertyInfoSource.GetValue(build).ToString());
                    }
                    else if (propertyInfo.Name == "BuildFile")
                    {
                        typePropertyInfoSource = typeIBuildDetail.GetProperty("LogLocation");
                        int? buildFiles = TfsDataCollection.GetBuildFile(typePropertyInfoSource.GetValue(build)?.ToString());
                        typePropertyInfoTarget.SetValue(ob1, buildFiles);
                    }
                    else
                    {
                        typePropertyInfoTarget.SetValue(ob1, typePropertyInfoSource.GetValue(build)); // 使用欄位資訊設定 ob1 相對應的屬性
                    }
                }

                buildInfos.Add(ob1 as BuildInfo);
            }
            
            return buildInfos;

            //備用
            //var buildInfos = builds.Select(n =>
            //{
            //    var buildInfo = new BuildInfo()
            //    {
            //        TestStatus = n.TestStatus,
            //        DropLocation = n.DropLocation,
            //        LabelName = n.LabelName,
            //        LogLocation = n.LogLocation,
            //        Quality = n.Quality,
            //        CompilationStatus = n.CompilationStatus,
            //        Status = n.Status,
            //        BuildControllerUri = n.BuildControllerUri.ToString(),
            //        BuildDefinitionUri = n.BuildDefinitionUri.ToString(),
            //        BuildFinished = n.BuildFinished,
            //        LastChangedByDisplayName = n.LastChangedByDisplayName,
            //        BuildNumber = n.BuildNumber,
            //        RequestedFor = n.RequestedFor,
            //        RequestedBy = n.RequestedBy,
            //        ContainerId = n.ContainerId,
            //        TeamProject = n.TeamProject,
            //        LastChangedOn = n.LastChangedOn,
            //        Uri = n.Uri.ToString(),
            //        StartTime = n.StartTime,
            //        SourceGetVersion = n.SourceGetVersion,
            //        Reason = n.Reason,
            //        FinishTime = n.FinishTime,
            //        LastChangedBy = n.LastChangedBy
            //    };

            //    return buildInfo;

            //});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int? GetBuildFile(string path)
        {
            int buildFile = 0;

            if (path != null)
            {
                try
                {
                    StreamReader reader = File.OpenText(path);
                    String text = reader.ReadToEnd();
                    foreach (Match match in Regex.Matches(text, @"Copying file from \""C:\\Builds")) buildFile++;
                    return buildFile;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;            

        }

    }
}