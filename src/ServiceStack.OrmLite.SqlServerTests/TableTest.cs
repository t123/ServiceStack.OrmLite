using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite.SqlServerTests
{
    [TestFixture]
    public class TableTest : OrmLiteTestBase
    {
        public class TableWithOnlyNonAutoIncrementingPrimaryKey
        {
            public int Id { get; set; }
        }

        public class TableWithOnlyAutoIncrementingPrimaryKey
        {
            [AutoIncrement]
            public int Id { get; set; }
        }

        [Test]
        public void Can_insert_into_table_with_only_autoincrementing_primary_key()
        {
            using(var db = ConnectionString.OpenDbConnection())
            {
                db.CreateTable<TableWithOnlyAutoIncrementingPrimaryKey>(true);

                Assert.DoesNotThrow(
                    () =>
                    {
                        db.Save(new TableWithOnlyAutoIncrementingPrimaryKey() { Id = 1 });
                    }
                    );
            }
        }

        [Test]
        public void Can_insert_into_table_with_only_nonautoincrementing_primary_key()
        {
            using(var db = ConnectionString.OpenDbConnection())
            {
                db.CreateTable<TableWithOnlyNonAutoIncrementingPrimaryKey>(true);

                Assert.DoesNotThrow(
                    () =>
                    {
                        db.Save(new TableWithOnlyNonAutoIncrementingPrimaryKey());
                    }
                );
            }
        }
    }
}
