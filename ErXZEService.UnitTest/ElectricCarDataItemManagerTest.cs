using DryIoc;
using ErXZEService.Models;
using ErXZEService.Services;
using ErXZEService.Services.CarDataPersistence;
using ErXZEService.Services.CarDataPersistence.ElectricCarDataItemLoader;
using ErXZEService.Services.Events;
using ErXZEService.Services.Log;
using ErXZEService.Services.Paths;
using ErXZEService.Services.SQL;
using ErXZEService.UnitTest.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ErXZEService.UnitTest
{
    public class FakeLogger : ILogger
    {
        public void LogError(string message, Exception e = null)
        {
            Debug.WriteLine($"{DateTime.Now} [Error] {message}");
        }

        public void LogInformation(string message, Exception e = null)
        {
            Debug.WriteLine($"{DateTime.Now} [Info] {message}");
        }
    }

    [TestClass]
    public class ElectricCarDataItemManagerTest
    {
        private string TestDataSourcePath => "H:\\ErXBout";
        private static string ReverseEngineeredFile => "reverseEngineered.txt";
        
        [TestMethod]
        public void Import()
        {
            FileCleaner.Cleanup(new List<string>
            {
                StoragePathProvider.ImportFilePath,
                StoragePathProvider.DatabasePath,
                StoragePathProvider.DatabasePath + "-journal"
            });

            IoC.IoContainer = new Container();
            IoC.IoContainer.Register<ILogger, FakeLogger>(new SingletonReuse());
            IoC.IoContainer.Register<IEventService, EventService>(new SingletonReuse());

            var eventservice = IoC.Resolve<IEventService>();

            var logger = IoC.Resolve<ILogger>();
            var manager = new ElectricCarDataItemManager(logger);
            manager.ReverseEngineer = true;
            manager.Load(TestDataSourcePath);

            eventservice.Save();
            eventservice.LoadLatest();
        }

        [TestMethod]
        public void ImportSecondTime()
        {
            FileCleaner.Cleanup(new List<string>
            {
                StoragePathProvider.DatabasePath,
                StoragePathProvider.DatabasePath + "-journal"
            });
            var logger = new Mock<ILogger>();

            var manager = new ElectricCarDataItemManager(logger.Object);
            manager.Load(TestDataSourcePath);

            File.Move(StoragePathProvider.DatabasePath, StoragePathProvider.DatabasePath.Replace(".db", "_aftersecondImport.db"));
            FullCleanup();
        }

        [TestMethod]
        public void AppendDataItem()
        {
            var logger = new Mock<ILogger>();
            var toAppend =
                ("1;349;92;262;25;CM:7;ST:2;GR:1;Dt:01.08.2020 10.36.00;\r\n" +
                 "5;348;91;262;25;At:28;ST:2;GR:7;MxC:183;tbA:89;tbD:18;tbK:0;tbS:196;Dt:01.08.2020 10.40.00;\r\n" +
                 "6;347;91;261;25;MxC:186;tbA:128;tbD:21;tbK:0;tbS:218;Dt:01.08.2020 10.41.00;\r\n" +
                 "8;346;91;261;25;At:27;MxC:183;tbA:93;tbD:39;tbK:0;tbS:275;Dt:01.08.2020 10.43.00;")
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var manager = new ElectricCarDataItemManager(logger.Object);

            using (var session = new SQLiteSession(logger.Object))
                manager.Load(session);

            foreach (var append in toAppend)
                manager.Append(append);

            using (var session = new SQLiteSession(logger.Object))
                Assert.AreEqual(1, session.SelectMany<ElectricCarDataItem>().Count);
        }

        [TestMethod]
        public void TimestampReverseEngineering_ShouldNotMessUpImport()
        {
            FileCleaner.Cleanup(new List<string> { ReverseEngineeredFile });
            var logger = new Mock<ILogger>();
            //Arrange
            var originalManager = new ElectricCarDataItemManager(logger.Object);
            var reverseEngineerer = new ElectricCarDataItemManager(logger.Object) { ReverseEngineer = true };
            var reverseEngineered = new ElectricCarDataItemManager(logger.Object);
            var files = new List<string>() { ReverseEngineeredFile };

            var loader = new ElectricCarDataItemLoaderFromFiles(files);
            var testDataSourceLoader = new ElectricCarDataItemLoaderFromFiles(TestDataSourcePath, logger.Object);

            originalManager.ImportFromFilePath(TestDataSourcePath, testDataSourceLoader);
            reverseEngineerer.ImportFromFilePath(TestDataSourcePath, testDataSourceLoader);
            //Act
            reverseEngineered.ImportFromFilePath(TestDataSourcePath, loader);

            //Assert
            Assert.AreEqual(originalManager.TripItems.Count, reverseEngineered.TripItems.Count);
            Assert.AreEqual(originalManager.ChargeItems.Count, reverseEngineered.ChargeItems.Count);

            File.Move(StoragePathProvider.DatabasePath, StoragePathProvider.DatabasePath.Replace(".db", "_afterReverseEngineeringImport.db"));
        }

        //[ClassCleanup]
        public static void FullCleanup()
        {
            FileCleaner.Cleanup(new List<string>
            {
                ReverseEngineeredFile,
                StoragePathProvider.DataItemDatabasePath,
                StoragePathProvider.DatabasePath,
                StoragePathProvider.DatabasePath + "-journal"
            });
        }
    }
}
