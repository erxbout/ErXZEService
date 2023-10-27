using ErXZEService.Services.Log;
using ErXZEService.Services.SQL;
using ErXZEService.UnitTest.SQL.Entities;
using ErXZEService.UnitTest.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace ErXZEService.UnitTest.SQL
{
    [TestClass]
    public class SQLiteSessionTests
    {
        private static string DatabaseName = "zSelectRecursiveDatabase";

        [TestMethod]
        public void SelectRecursive_OneToOne()
        {
            FullCleanup();
            //Arrange
            var logger = new Mock<ILogger>();
            var entity = new MySelectRecursiveTestEntity();
            var oneToOneEntity = new MySelectRecursiveTestOneToOneEntity() { Text = "Test one to one" };

            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                session.Save(oneToOneEntity);

                entity.MySelectRecursiveTestEntityId = oneToOneEntity.Id;
                session.Save(entity);
            }

            //Act
            MySelectRecursiveTestEntity selectedEntity = null;
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                selectedEntity = session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == entity.Id);
                session.SelectRecursive(selectedEntity);
            }

            //Assert
            Assert.AreEqual(oneToOneEntity.Text, selectedEntity.OneToOneEntity.Text);
        }

        [TestMethod]
        public void SelectRecursive_OneToMany()
        {
            FullCleanup();
            //Arrange
            var logger = new Mock<ILogger>();
            var entity = new MySelectRecursiveTestEntity();
            var oneToManyEntities = new List<MySelectRecursiveTestOneToManyEntity>()
                {
                    new MySelectRecursiveTestOneToManyEntity()
                    {
                        Text = "Test one to many"
                    },

                    new MySelectRecursiveTestOneToManyEntity()
                    {
                        Text = "second Test one to many"
                    }
                };

            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                session.Save(entity);

                oneToManyEntities.ForEach(x => x.MySelectRecursiveTestEntityId = entity.Id);

                session.Save(oneToManyEntities);
            }

            //Act
            MySelectRecursiveTestEntity selectedEntity = null;
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                selectedEntity = session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == entity.Id);
                session.SelectRecursive(selectedEntity);
                session.SelectRecursive(selectedEntity);
            }

            //Assert
            Assert.AreEqual(oneToManyEntities.Count, selectedEntity.OneToManyEntities.Count);
            Assert.AreEqual(oneToManyEntities[0].Text, selectedEntity.OneToManyEntities[0].Text);
            Assert.AreEqual(oneToManyEntities[1].Text, selectedEntity.OneToManyEntities[1].Text);
        }

        [TestMethod]
        public void SaveAndSelectRecursive()
        {
            FullCleanup();
            //Arrange
            var logger = new Mock<ILogger>();
            var entity = NewMySelectRecursiveTestEntity();

            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                session.SaveRecursive(entity);
            }

            //Act
            MySelectRecursiveTestEntity selectedEntity = null;
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                selectedEntity = session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == entity.Id);
                session.SelectRecursive(selectedEntity);
            }

            //Assert
            Assert.AreEqual(entity.Id, selectedEntity.Id);
            Assert.AreEqual(entity.Changed, selectedEntity.Changed);

            Assert.AreEqual(entity.OneToManyEntities.Count, selectedEntity.OneToManyEntities.Count);
            Assert.AreEqual(entity.OneToManyEntities[0].Text, selectedEntity.OneToManyEntities[0].Text);
            Assert.AreEqual(entity.OneToManyEntities[1].Text, selectedEntity.OneToManyEntities[1].Text);
        }

        [TestMethod]
        public void SaveRecursive_SelectMany()
        {
            FullCleanup();
            //Arrange
            var logger = new Mock<ILogger>();
            var entity = NewMySelectRecursiveTestEntity();

            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                session.SaveRecursive(entity);
            }

            //Act
            MySelectRecursiveTestEntity selectedEntity = null;
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                selectedEntity = session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == entity.Id);

                selectedEntity.OneToManyEntities = session.SelectMany<MySelectRecursiveTestOneToManyEntity>(x => x.MySelectRecursiveTestEntityId == selectedEntity.Id);
            }

            //Assert
            Assert.AreEqual(entity.Id, selectedEntity.Id);
            Assert.AreEqual(entity.Changed, selectedEntity.Changed);

            Assert.AreEqual(entity.OneToManyEntities.Count, selectedEntity.OneToManyEntities.Count);
            Assert.AreEqual(entity.OneToManyEntities[0].Text, selectedEntity.OneToManyEntities[0].Text);
            Assert.AreEqual(entity.OneToManyEntities[1].Text, selectedEntity.OneToManyEntities[1].Text);
        }

        [TestMethod]
        public void SaveEntityMultipleTimes_ShouldNotCreateMultipleEntities()
        {
            FullCleanup();
            //Arrange
            var logger = new Mock<ILogger>();
            var entity = NewMySelectRecursiveTestEntity();
            var duplicateKeyOneToOneEntity = new MySelectRecursiveTestOneToOneEntity()
            {
                Text = "My second entity"
            };
            
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                session.SaveRecursive(entity);
                session.SaveRecursive(entity);
                session.Save(entity.OneToManyEntities);
                session.Save(entity);
            }

            //Act
            MySelectRecursiveTestEntity selectedEntity = null;
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                selectedEntity = session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == entity.Id);

                selectedEntity.OneToManyEntities = session.SelectMany<MySelectRecursiveTestOneToManyEntity>(x => x.MySelectRecursiveTestEntityId == selectedEntity.Id);
            }

            //Assert
            Assert.AreEqual(entity.Id, selectedEntity.Id);
            Assert.AreEqual(entity.Changed, selectedEntity.Changed);

            Assert.AreEqual(entity.OneToManyEntities.Count, selectedEntity.OneToManyEntities.Count);
            Assert.AreEqual(entity.OneToManyEntities[0].Text, selectedEntity.OneToManyEntities[0].Text);
            Assert.AreEqual(entity.OneToManyEntities[1].Text, selectedEntity.OneToManyEntities[1].Text);
        }

        [TestMethod]
        public void SqliteSessionCtor()
        {
            FullCleanup();
            //Arrange
            var logger = new Mock<ILogger>();
            var entity = NewMySelectRecursiveTestEntity();

            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                session.SaveRecursive(entity);
                session.SaveRecursive(entity);
                session.Save(entity.OneToManyEntities);
                session.Save(entity);

                using (var newSession = new SQLiteSession(logger.Object))
                using (var newSessionTwo = new SQLiteSession(logger.Object))
                {
                    newSession.Save(entity);
                }
            }

            //Act
            MySelectRecursiveTestEntity selectedEntity = null;
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                selectedEntity = session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == entity.Id);

                selectedEntity.OneToManyEntities = session.SelectMany<MySelectRecursiveTestOneToManyEntity>(x => x.MySelectRecursiveTestEntityId == selectedEntity.Id);
            }

            //Assert
            Assert.AreEqual(entity.Id, selectedEntity.Id);
            Assert.AreEqual(entity.Changed, selectedEntity.Changed);

            Assert.AreEqual(entity.OneToManyEntities.Count, selectedEntity.OneToManyEntities.Count);
            Assert.AreEqual(entity.OneToManyEntities[0].Text, selectedEntity.OneToManyEntities[0].Text);
            Assert.AreEqual(entity.OneToManyEntities[1].Text, selectedEntity.OneToManyEntities[1].Text);
        }

        [TestMethod]
        public void IllegalOperations()
        {
            FullCleanup();
            //Arrange
            var logger = new Mock<ILogger>();
            var entity = NewMySelectRecursiveTestEntity();
            var illegalMultipleAttributesEntity = new IllegalTestMulitpleAttributesEntity();
            var illegalOneToManyEntity = new IllegalTestOneToManyListIsNotInstancedEntity();
            var illegalTestEntityHasNoTableAttributeEntity = new IllegalTestEntityHasNoTableAttributeEntity();

            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                session.Save(illegalTestEntityHasNoTableAttributeEntity);
                session.SaveRecursive(entity);
            }

            //Act
            MySelectRecursiveTestEntity selectedEntity = null;
            using (var session = new SQLiteSession(DatabaseName, logger.Object))
            {
                Assert.ThrowsException<InvalidOperationException>(() => session.SelectRecursive(illegalMultipleAttributesEntity));
                Assert.ThrowsException<InvalidOperationException>(() => session.SelectRecursive(illegalOneToManyEntity));
                Assert.ThrowsException<InvalidOperationException>(() => session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == 1000));

                selectedEntity = session.SelectSingle<MySelectRecursiveTestEntity>(x => x.Id == entity.Id);

                var selectEverything = session.SelectMany<MySelectRecursiveTestEntity>();

                selectedEntity.OneToManyEntities = session.SelectMany<MySelectRecursiveTestOneToManyEntity>(x => x.MySelectRecursiveTestEntityId == selectedEntity.Id);
            }

            //Assert
            Assert.AreEqual(entity.Id, selectedEntity.Id);
            Assert.AreEqual(entity.Changed, selectedEntity.Changed);

            Assert.AreEqual(entity.OneToManyEntities.Count, selectedEntity.OneToManyEntities.Count);
            Assert.AreEqual(entity.OneToManyEntities[0].Text, selectedEntity.OneToManyEntities[0].Text);
            Assert.AreEqual(entity.OneToManyEntities[1].Text, selectedEntity.OneToManyEntities[1].Text);
        }

        private MySelectRecursiveTestEntity NewMySelectRecursiveTestEntity()
        {
            return new MySelectRecursiveTestEntity()
            {
                OneToManyEntities = new List<MySelectRecursiveTestOneToManyEntity>()
                {
                    new MySelectRecursiveTestOneToManyEntity()
                    {
                        Text = "Test one to many"
                    },

                    new MySelectRecursiveTestOneToManyEntity()
                    {
                        Text = "second Test one to many"
                    }
                },
                OneToOneEntity = new MySelectRecursiveTestOneToOneEntity()
                {
                    Text = "My one to one entity"
                }
            };
        }

        [ClassCleanup]
        public static void FullCleanup()
        {
            FileCleaner.Cleanup(new List<string>
            {
                DatabaseName
            });
        }
    }
}
