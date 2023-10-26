SSRSMigrate [![Github All Releases](https://img.shields.io/github/downloads/jpann/SSRSMigrate/total.svg)]() ![GitHub Releases](https://img.shields.io/github/downloads/jpann/SSRSMigrate/latest/total)
======== 

| Master  |      Develop      |
|----------|:-------------:|
| [![Build status](https://ci.appveyor.com/api/projects/status/bvj8gbpf8400t689/branch/master?svg=true)](https://ci.appveyor.com/project/jpann/ssrsmigrate/branch/master) |  [![Build status](https://ci.appveyor.com/api/projects/status/bvj8gbpf8400t689/branch/develop?svg=true)](https://ci.appveyor.com/project/jpann/ssrsmigrate/branch/develop)|
| ![](https://img.shields.io/appveyor/tests/jpann/SSRSMigrate/master.svg?style=flat) |    ![](https://img.shields.io/appveyor/tests/jpann/SSRSMigrate/develop.svg?style=flat)   |
| ![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/jpann/SSRSMigrate?sort=semver) | ![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/jpann/SSRSMigrate?include_prereleases) |



Goal of this project is to have a utility that will allow you to easily migrate reports from one Microsoft SQL Reporting Services (SSRS) server to another or from one root folder in SSRS to another folder.

This should make my job and those that I work with easier.

Requirements
-----
This application requires .NET Framework 4.5

Features
-----

#### Server-to-Server migration
This method directly migrates all folders, data sources and report items contained within the specified path on the source server to the specified path on the destination server. 

The path specified for the destination server will contain all items from the path on the source server. For example, if you migrate path `/SourceProject` from the source server to the path `/DestinationProject` on the destination server, Reports/Data Sources/Folders contained within `/SourceProject` will be created on the destination server under `/DestinationProject`, so your source report of `/SourceProject/My Report` will now be `/DestinationProject/My Report` on the destination server.

The report and data source definitions that are migrated will be updated to point to the new paths on the destination server during this process.

You cannot migrate items from a newer version of SQL Reporting Services to an older version of SQL Reporting Services.

##### Changing Data Sources
You can now change a Data Source properties (e.g. Connection String) during the `server-to-server` migration process. 
To do this, you must right click on the Data Source entry in the `Source Server` list and choose `Edit Data Source...`. 
The color of the Data Source's row will then change to indicate that the Data Source has been changed. Now when this Data Source is created 
on the new Report Server it will be created using these new properties.

#### Export to disk
This method exports all folders, data sources and report items contained within the specified path to the directory you set, while maintaining the path structure.

Data source items are serialized to JSON format, folders are exported to the file system as directories and report items are exported in the RDL format.

Currently, you cannot use this method as a form of migration but is more or less intended as a quick way to get a copy of your items for local editing.

#### Export to ZIP archive
This method exports all folders, data sources and report items contained within the specified directory to a ZIP archive that you set. 

The ZIP archive is created using a format specifically used by the 'Import from ZIP archive' method. The archive contains a list of every item exported, and an MD5 check sum of the exported item, that will be used to verify file integrity during the 'Import from ZIP archive' method. The ZIP archive also contains the version of SQL Reporting Services that the archive was created from, in order to prevent importing into an older version of SQL Reporting Services.

#### Import from ZIP archive
This method imports all folders, data sources and report items contained within the specified ZIP archive to the path specified on the destination server.

The path specified for the destination server will contain all items from the archive For example, if the archive was created by exporting the path `/SourceProject` and you import to the path `/DestinationProject` on the destination server, everything that is contained within `/SourceProject` will be created on the destination server under `/DestinationProject`, so your source report of `/SourceProject/My Report` will now be `/DestinationProject/My Report` on the destination server.

The report and data source definitions that are migrated will be updated to point to the new paths on the destination server during this process.

Python Plugins
-----
This project now supports executing a Python plugin using IronPython during the `server-to-server` migration process. This will allow you to execute methods defined in the Python script during various parts of the migration process. For example, when a ReportItem 
is migrated you can perform a query against a SQL Server database. 

All Python scripts need to use the `Plugin` class:

```
class Plugin:
    def __init__(self):
        pass

    def OnLoad(self):
        pass

    ...
```


#### Plugin Script Event Handlers

| Event Handler  |      Description      | Parameters |
|:----------|:-------------|:-------------|
|  `OnLoad` | Called when the script is loaded and the script engine is configured for this script. | |
|  `OnMigration_Start` | Called when the `server-to-server` migration starts. | <ul><li> `sourcePath` contains the source SSRS folder specified <li> `destPath` contains the destination folder specified in the migration.</ul> |
|  `OnMigration_Completed` | Called when the `server-to-server` migration completes. | <ul><li>`er` contains any exception object if an exception occurred during the migration, this will be `null` if no exception occurred. <li>`msg` contains any message created when the migration was completed. <li>`sourcePath` contains the source SSRS folder specified. <li>`destPath` contains the destination folder specified in the migration.</ul> |
|  `OnMigration_FolderItem` | Called after a Folder has been migrated. | <ul><li>`folderItem` contains the [FolderItem](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/SSRS/Item/FolderItem.cs) object for the folder that was migrated. <li>`status` contains the [MigrationStatus](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/Status/MigrationStatus.cs) object for that item's migration, that contains details about the migration.</ul> |
|  `OnMigration_DataSourceItem` | Called after a Data Source has been migrated. | <ul><li>`item` contains the [DataSourceItem](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/SSRS/Item/DataSourceItem.cs) object for the Data Source that was migrated. <li>`status` contains the [MigrationStatus](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/Status/MigrationStatus.cs) object for that item's migration, that contains details about the migration.</ul> |
|  `OnMigration_ReportItem` | Called after a report has been migrated. | <ul><li>`item` contains the [ReportItem](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/SSRS/Item/ReportItem.cs) object for the report that was migrated. <li>`status` contains the [MigrationStatus](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/Status/MigrationStatus.cs) object for that item's migration, that contains details about the migration.</ul> |


#### Plugin Script Objects

The Python script engine gives the script access to various methods and objects.

| Object  | Interface/Class  |      Description      | 
|:----------|:-------------|:-------------|
| `SSRSUtil` | `SSRSUtil`  |  The static [SSRSUtil](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/Utility/SSRSUtil.cs) class. |
| `FileSystem` | `IFileSystem`  | The current `IFileSystem` instance from the [System.IO.Abstractions](https://github.com/System-IO-Abstractions/System.IO.Abstractions) library. |
| `ReportServerReader` | `IReportServerReader`  |  The current [IReportServerReader](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/SSRS/Reader/IReportServerReader.cs) instance for the current report server connection. |
| `ReportServerWriter` | `IReportServerWriter`  |  The current [IReportServerWriter](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/SSRS/Writer/IReportServerWriter.cs) instance for the current report server connection. |
| `ReportServerRepository` | `IReportServerRepository`  |  The current [IReportServerRepository](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/SSRS/Repository/IReportServerRepository.cs) instance for the current report server connection. |
| `SQLUtil` | `SQLUtil` | The static [SQLUtil](https://github.com/jpann/SSRSMigrate/blob/master/SSRSMigrate/SSRSMigrate/Utility/SQLUtil.cs) class that lets you query a SQL database. See example script below for usage or view the class itself. |
| `Logger` | `ILogger` | The script engine's current `ILogger` [log4net](http://logging.apache.org/log4net/index.html) instance. This will write to `SSRSMigrate_Script.log`. |


#### Example Script

Below is an example script that runs an `UPDATE` SQL query, using the `SQLUtil` object's `ExecuteNonQuery` method each time a `ReportItem` is migrated.

```
import clr

class Plugin:
    def __init__(self):
        pass

    # SQLUtil requires that a delegate that returns the SQL Server's connection string is provided to it's Initialize method. 
    # This is the method that we will set that delegate to.
    def GetConnectionString(self):
        return "Data Source=(local);Initial Catalog=Database;Trusted_Connection=yes;Connect Timeout=0;"

    def OnLoad(self):
        # Called when the script is loaded. This is where you can do stuff like open a connection to a database.
        Logger.Info("OnLoad")

        SQLUtil.Initialize(self.GetConnectionString)

    def OnMigration_Start(self, sourcePath, destPath):
        Logger.Info("OnMigration_Start - %s" % (sourcePath, ))

        # ExecuteScalar is a generic method, so you need to provide the return type
        count = SQLUtil.ExecuteScalar[int]("SELECT COUNT(*) FROM Table")

        # String example:
        mystring = SQLUtil.ExecuteScalar[string]("SELECT TOP 1 CAST(MyColumn AS VARCHAR(50)) FROM Table")

        Logger.Info("Count: %s" % (count, ))

    def OnMigration_Completed(self, msg, sourcePath, destPath):
        Logger.Info("OnMigration_Completed - %s; migrated from %s to %s" % (msg, sourcePath, destPath))

    def OnMigration_FolderItem(self, item, status):
        Logger.Info("OnMigration_FolderItem - %s" % (item.Path, ))

    def OnMigration_DataSourceItem(self, item, status):
        Logger.Info("OnMigration_DataSourceItem - %s" % (item.Path, ))

    def OnMigration_ReportItem(self, item, status):
        Logger.Info("OnMigration_ReportItem - %s" % (item.Path, ))

        SQLUtil.ExecuteNonQuery("UPDATE Table SET Path='%s' WHERE Path='%s'" % (source.ToPath, source.FromPath))

```

Logging
-----
By default, logging is set to an information level, which will not include debug logging messages.

Normal log messages are written to the `SSRSMigrate.log` file and errors are written to `SSRSMigrateErrors.log`. 
Script logs are written to `SSRSMigrate_Script.log`.

**Enabling debug level logging**


You can enable debug level logging by editing the SSRSMigrate.exe.config file and changing the level `value` in the `root` tag to `DEBUG`.

 Default information level logging:
```
       <root>
          <level value="INFO" />
          <appender-ref ref="LogFileAppender" />
          <appender-ref ref="ErrorFileAppender" />
        </root>
 ```

 Change the `value` attribute to `DEBUG`:
```
      <root>
          <level value="DEBUG" />
          <appender-ref ref="LogFileAppender" />
          <appender-ref ref="ErrorFileAppender" />
        </root>
```