using System.IO.Abstractions;
using Ninject;
using Ninject.Activation;
using SSRSMigrate.Bundler;
using SSRSMigrate.SSRS.Repository;
using SSRSMigrate.ReportServer2010;
using SSRSMigrate.ReportServer2005;
using Ninject.Modules;
using System.Net;
using log4net;
using Ninject.Extensions.Logging;
using SSRSMigrate.SSRS.Test;
using SSRSMigrate.SSRS.Writer;
using SSRSMigrate.SSRS.Reader;
using SSRSMigrate.Exporter.Writer;
using SSRSMigrate.Exporter;
using SSRSMigrate.DataMapper;
using SSRSMigrate.Forms;
using SSRSMigrate.ScriptEngine;
using SSRSMigrate.SSRS.Validators;
using SSRSMigrate.Wrappers;

namespace SSRSMigrate
{
    public class ReportServerRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            // Bind source IReportServerRepository
            this.Bind<IReportServerRepository>()
               .ToProvider<SourceReportServer2005RepositoryProvider>()
               .Named("2005-SRC");

            this.Bind<IReportServerRepository>()
               .ToProvider<SourceReportServer2010RepositoryProvider>()
               .Named("2010-SRC");

            // Bind destination IReportServerRepository
            this.Bind<IReportServerRepository>()
               .ToProvider<DestinationReportServer2005RepositoryProvider>()
               .Named("2005-DEST");

            this.Bind<IReportServerRepository>()
               .ToProvider<DestinationReportServer2010RepositoryProvider>()
               .Named("2010-DEST");

            // Bind IReportServerPathValidator
            this.Bind<IReportServerPathValidator>().To<ReportServerPathValidator>()
                .WithPropertyValue("InvalidPathChars", c => Properties.Settings.Default.SSRSInvalidPathChars)
                .WithPropertyValue("InvalidNameChars", c => Properties.Settings.Default.SSRSInvalidNameChars);

            // Bind IReportServerReader
            this.Bind<IReportServerReader>()
                .To<ReportServerReader>()
                .Named("2005-SRC")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2005-SRC"));

            this.Bind<IReportServerReader>()
                .To<ReportServerReader>()
                .Named("2010-SRC")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2010-SRC"));

            this.Bind<IReportServerReader>()
                .To<ReportServerReader>()
                .Named("2005-DEST")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2005-DEST"));

            this.Bind<IReportServerReader>()
                .To<ReportServerReader>()
                .Named("2010-DEST")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2010-DEST"));

            // Bind IReportServerWriter
            this.Bind<IReportServerWriter>()
                .To<ReportServerWriter>()
                .Named("2005-DEST")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2005-DEST"));

            this.Bind<IReportServerWriter>()
                .To<ReportServerWriter>()
                .Named("2010-DEST")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2010-DEST"));

            // Bind IReportServerTester
            this.Bind<IReportServerTester>()
                .To<ReportServerTester>()
                .Named("2005-SRC")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2005-SRC"));

            this.Bind<IReportServerTester>()
                .To<ReportServerTester>()
                .Named("2010-SRC")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2010-SRC"));

            this.Bind<IReportServerTester>()
                .To<ReportServerTester>()
                .Named("2005-DEST")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2005-DEST"));

            this.Bind<IReportServerTester>()
                .To<ReportServerTester>()
                .Named("2010-DEST")
                .WithConstructorArgument("repository", c => c.Kernel.Get<IReportServerRepository>("2010-DEST"));


            // Bind IFileSystem
            this.Bind<IFileSystem>().To<FileSystem>();

            // Bind ISerializeWrapper
            this.Bind<ISerializeWrapper>().To<JsonConvertWrapper>();

            // Bind IExportWriter
            this.Bind<IExportWriter>().To<FileExportWriter>().WhenInjectedExactlyInto<DataSourceItemExporter>();
            this.Bind<IExportWriter>().To<FileExportWriter>().WhenInjectedExactlyInto<ReportItemExporter>();
            this.Bind<IExportWriter>().To<FolderExportWriter>().WhenInjectedExactlyInto<FolderItemExporter>();

            // Bind IItemExporter
            this.Bind(typeof(IItemExporter<>)).To(typeof(ReportItemExporter));
            this.Bind(typeof(IItemExporter<>)).To(typeof(FolderItemExporter));
            this.Bind(typeof(IItemExporter<>)).To(typeof(DataSourceItemExporter));

            // Bind IBundler
            this.Bind<IBundler>().To<ZipBundler>();

            // Bind IZipFileWrapper
            this.Bind<IZipFileWrapper>().To<ZipFileWrapper>();

            // Bind IZipFileReaderWrapper
            this.Bind<IZipFileReaderWrapper>().To<ZipFileReaderWrapper>();

            // Bind ICheckSumGenerator
            this.Bind<ICheckSumGenerator>().To<MD5CheckSumGenerator>();
            
            // Bind IBundleReader
            this.Bind<IBundleReader>().To<ZipBundleReader>()
                .WithConstructorArgument("fileName", GetImportZipFileName)
                .WithConstructorArgument("unpackDirectory", GetImportZipUnpackDirectory);

            // Bind DataSourceEditForm
            this.Bind<DataSourceEditForm>().To<DataSourceEditForm>();

            // Bind PythonEngine
            this.Bind<PythonEngine>().ToSelf()
                .Named("2005-DEST")
                .WithConstructorArgument("reportServerReader", c => c.Kernel.Get<IReportServerReader>("2005-DEST"))
                .WithConstructorArgument("reportServerWriter", c => c.Kernel.Get<IReportServerWriter>("2005-DEST"))
                .WithConstructorArgument("reportServerRepository", c => c.Kernel.Get<IReportServerRepository>("2005-DEST"))
                .WithConstructorArgument("scriptLogger", c => c.Kernel.Get<ILoggerFactory>().GetLogger("ScriptLogger"));

            this.Bind<PythonEngine>().ToSelf()
                .Named("2010-DEST")
                .WithConstructorArgument("reportServerReader", c => c.Kernel.Get<IReportServerReader>("2010-DEST"))
                .WithConstructorArgument("reportServerWriter", c => c.Kernel.Get<IReportServerWriter>("2010-DEST"))
                .WithConstructorArgument("reportServerRepository", c => c.Kernel.Get<IReportServerRepository>("2010-DEST"))
                .WithConstructorArgument("scriptLogger", c => c.Kernel.Get<ILoggerFactory>().GetLogger("ScriptLogger"));
        }

        private string GetImportZipFileName(IContext context)
        {
            return Properties.Settings.Default.ImportZipFileName;
        }

        private string GetImportZipUnpackDirectory(IContext context)
        {
            return Properties.Settings.Default.ImportZipUnpackDir;
        }
    }

    public class SourceReportServer2005RepositoryProvider : Provider<IReportServerRepository>
    {
        protected override IReportServerRepository CreateInstance(IContext context)
        {
            string url = Properties.Settings.Default.SrcWebServiceUrl;
            string version = Properties.Settings.Default.SrcVersion;
            bool defaultCred = Properties.Settings.Default.SrcDefaultCred;
            string username = Properties.Settings.Default.SrcUsername;
            string password = Properties.Settings.Default.SrcPassword;
            string domain = Properties.Settings.Default.SrcDomain;
            string path = Properties.Settings.Default.SrcPath;

            if (!url.EndsWith("reportservice2005.asmx"))
                if (url.EndsWith("/"))
                    url = url.Substring(0, url.Length - 1);

            url = string.Format("{0}/reportservice2005.asmx", url);

            ReportingService2005 service = new ReportingService2005();
            service.Url = url;

            if (defaultCred)
            {
                service.Credentials = CredentialCache.DefaultNetworkCredentials;
                service.PreAuthenticate = true;
                service.UseDefaultCredentials = true;
            }
            else
            {
                service.Credentials = new NetworkCredential(
                    username,
                    password,
                    domain);

                //service.UseDefaultCredentials = false;
            }

            return new ReportServer2005Repository(path, service, new ReportingService2005DataMapper());
        }
    }

    public class SourceReportServer2010RepositoryProvider : Provider<IReportServerRepository>
    {
        protected override IReportServerRepository CreateInstance(IContext context)
        {
            string url = Properties.Settings.Default.SrcWebServiceUrl;
            string version = Properties.Settings.Default.SrcVersion;
            bool defaultCred = Properties.Settings.Default.SrcDefaultCred;
            string username = Properties.Settings.Default.SrcUsername;
            string password = Properties.Settings.Default.SrcPassword;
            string domain = Properties.Settings.Default.SrcDomain;
            string path = Properties.Settings.Default.SrcPath;

            if (!url.EndsWith("reportservice2010.asmx"))
                if (url.EndsWith("/"))
                    url = url.Substring(0, url.Length - 1);

            url = string.Format("{0}/reportservice2010.asmx", url);

            ReportingService2010 service = new ReportingService2010();
            service.Url = url;

            if (defaultCred)
            {
                service.Credentials = CredentialCache.DefaultNetworkCredentials;
                service.PreAuthenticate = true;
                service.UseDefaultCredentials = true;
            }
            else
            {
                service.Credentials = new NetworkCredential(
                    username,
                    password,
                    domain);

                //service.UseDefaultCredentials = false;
            }

            return new ReportServer2010Repository(path, service, new ReportingService2010DataMapper());
        }
    }

    public class DestinationReportServer2005RepositoryProvider : Provider<IReportServerRepository>
    {
        protected override IReportServerRepository CreateInstance(IContext context)
        {
            string url = Properties.Settings.Default.DestWebServiceUrl;
            string version = Properties.Settings.Default.DestVersion;
            bool defaultCred = Properties.Settings.Default.DestDefaultCred;
            string username = Properties.Settings.Default.DestUsername;
            string password = Properties.Settings.Default.DestPassword;
            string domain = Properties.Settings.Default.DestDomain;
            string path = Properties.Settings.Default.DestPath;

            if (!url.EndsWith("reportservice2005.asmx"))
                if (url.EndsWith("/"))
                    url = url.Substring(0, url.Length - 1);

            url = string.Format("{0}/reportservice2005.asmx", url);

            ReportingService2005 service = new ReportingService2005();
            service.Url = url;

            if (defaultCred)
            {
                service.Credentials = CredentialCache.DefaultNetworkCredentials;
                service.PreAuthenticate = true;
                service.UseDefaultCredentials = true;
            }
            else
            {
                service.Credentials = new NetworkCredential(
                    username,
                    password,
                    domain);

                //service.UseDefaultCredentials = false;
            }

            return new ReportServer2005Repository(path, service, new ReportingService2005DataMapper());
        }
    }

    public class DestinationReportServer2010RepositoryProvider : Provider<IReportServerRepository>
    {
        protected override IReportServerRepository CreateInstance(IContext context)
        {
            string url = Properties.Settings.Default.DestWebServiceUrl;
            string version = Properties.Settings.Default.DestVersion;
            bool defaultCred = Properties.Settings.Default.DestDefaultCred;
            string username = Properties.Settings.Default.DestUsername;
            string password = Properties.Settings.Default.DestPassword;
            string domain = Properties.Settings.Default.DestDomain;
            string path = Properties.Settings.Default.DestPath;

            if (!url.EndsWith("reportservice2010.asmx"))
                if (url.EndsWith("/"))
                    url = url.Substring(0, url.Length - 1);

            url = string.Format("{0}/reportservice2010.asmx", url);

            ReportingService2010 service = new ReportingService2010();
            service.Url = url;

            if (defaultCred)
            {
                service.Credentials = CredentialCache.DefaultNetworkCredentials;
                service.PreAuthenticate = true;
                service.UseDefaultCredentials = true;
            }
            else
            {
                service.Credentials = new NetworkCredential(
                    username,
                    password,
                    domain);

                //service.UseDefaultCredentials = false;
            }

            return new ReportServer2010Repository(path, service, new ReportingService2010DataMapper());
        }
    }
}
