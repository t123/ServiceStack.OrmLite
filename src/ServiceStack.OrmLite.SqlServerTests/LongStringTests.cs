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
    public class UnicodeStringTest : OrmLiteTestBase
    {
        public class ModelWithIdAndLongString
        {
            public int Id { get; set; }

            [StringLength(4001)]
            public string LongString { get; set; }
        }

        class ComplexType
        {
            public int Id { get; set; }

            [StringLength(250)]
            public IList<string> StringList { get; set; }
            [StringLength(4000)]

            public Dictionary<Guid, string> Dictionary { get; set; }
            public IList<ComplexSubType> SubTypes { get; set; }

            internal class ComplexSubType
            {
                public int Field1 { get; set; }
                public string Field2 { get; set; }
                public Guid Field3 { get; set; }
                public DateTime Field4 { get; set; }
            }
        }

        [Test]
        public void Can_store_complex_type_as_unicode()
        {
            using(var con = ConnectionString.OpenDbConnection())
            {
                const string unicodeString = "日本語 äÄöÖüÜß ıIiİüÜğĞşŞöÖçÇ åÅäÄöÖ";
                OrmLiteConfig.DialectProvider.UseUnicode = true;
                ComplexType complexType = new ComplexType
                    {
                        StringList = new List<string>() { unicodeString },
                        Dictionary = new Dictionary<Guid, string> { { Guid.Empty, unicodeString } },
                        Id = 1,
                        SubTypes = new List<ComplexType.ComplexSubType>()
                            {
                                new ComplexType.ComplexSubType()
                                    {
                                        Field1 = 1,
                                        Field2 = unicodeString,
                                        Field3 = Guid.Empty,
                                        Field4 = new DateTime(2000, 01, 01, 0, 0, 0)
                                    }
                            }
                    };

                con.CreateTable<ComplexType>(true);
                con.Save(complexType);

                var result = con.GetById<ComplexType>(1);
                con.DropTable<ComplexType>();

                Assert.That(result.StringList[0] == complexType.StringList[0]);
                Assert.That(result.Dictionary[Guid.Empty] == complexType.Dictionary[Guid.Empty]);
                Assert.That(result.SubTypes[0].Field1 == complexType.SubTypes[0].Field1);
                Assert.That(result.SubTypes[0].Field2 == complexType.SubTypes[0].Field2);
                Assert.That(result.SubTypes[0].Field3 == complexType.SubTypes[0].Field3);
                Assert.That(result.SubTypes[0].Field4 == complexType.SubTypes[0].Field4);
            }
        }

        [Test]
        public void Complex_types_should_use_max_for_undefined_string_length()
        {
            OrmLiteConfig.DialectProvider.UseUnicode = true;
            var createTableSql = OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(ComplexType));
            Console.WriteLine("createTableSql: " + createTableSql);
            Assert.That(createTableSql.ToUpperInvariant().Contains("NVARCHAR(250)"), Is.True);
            Assert.That(createTableSql.ToUpperInvariant().Contains("NVARCHAR(4000)"), Is.True);
            Assert.That(createTableSql.ToUpperInvariant().Contains("NVARCHAR(MAX)"), Is.True);
        }

        [Test]
        public void Unicode_table_should_have_max_string_length_of_4000()
        {
            OrmLiteConfig.DialectProvider.UseUnicode = true;
            var createTableSql = OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(ModelWithIdAndName));
            Console.WriteLine("createTableSql: " + createTableSql);
            Assert.That(createTableSql.Contains("NVARCHAR(4000)"), Is.True);
        }

        [Test]
        public void Can_create_table_with_string_longer_than_4000_for_unicode()
        {
            using(var con = ConnectionString.OpenDbConnection())
            {
                OrmLiteConfig.DialectProvider.UseUnicode = true;

                Assert.DoesNotThrow(
                    () =>
                    {
                        con.CreateTable<ModelWithIdAndLongString>(true);
                    }
                );
            }
        }

        [Test]
        public void Can_store_unicode_string()
        {
            using(var con = ConnectionString.OpenDbConnection())
            {
                OrmLiteConfig.DialectProvider.UseUnicode = true;
                ModelWithIdAndName model = new ModelWithIdAndName()
                {
                    Id = 1,
                    Name = "日本語 äÄöÖüÜß ıIiİüÜğĞşŞöÖçÇ åÅäÄöÖ"
                };

                con.CreateTable<ModelWithIdAndName>(true);
                con.Save(model);

                var result = con.GetById<ModelWithIdAndName>(1);
                con.DropTable<ModelWithIdAndName>();

                Console.WriteLine("Inserted: {0}, Retrieved: {1}", model.Name, result.Name);
                Assert.That(model.Name.Equals(result.Name));
            }
        }

        [Test]
        public void Can_store_string_longer_than_4000_characters()
        {
            using(var con = ConnectionString.OpenDbConnection())
            {
                string longString = "";

                while(longString.Length < 9000)
                {
                    longString += "日本語 äÄöÖüÜß ıIiİüÜğĞşŞöÖçÇ åÅäÄöÖ";
                }

                longString = longString.Substring(0, 8500);

                Assert.That(longString.Length == 8500);

                ModelWithIdAndLongString model = new ModelWithIdAndLongString()
                {
                    Id = 1,
                    LongString = longString
                };

                con.CreateTable<ModelWithIdAndLongString>(true);
                con.Save(model);

                var result = con.GetById<ModelWithIdAndLongString>(1);
                con.DropTable<ModelWithIdAndLongString>();

                Console.WriteLine("Inserted: {0}, Retrieved: {1}", model.LongString, result.LongString);
                Assert.That(model.LongString.Equals(result.LongString));
            }
        }
    }

    [TestFixture]
    public class NonUnicodeStringTests : OrmLiteTestBase
    {
        public class ModelWithIdAndLongString
        {
            public int Id { get; set; }

            [StringLength(9000)]
            public string LongString { get; set; }
        }

        class ComplexType
        {
            public int Id { get; set; }

            [StringLength(250)]
            public IList<string> StringList { get; set; }
            [StringLength(8000)]
            public Dictionary<Guid, string> Dictionary { get; set; }
            public IList<ComplexSubType> SubTypes { get; set; }

            internal class ComplexSubType
            {
                public int Field1 { get; set; }
                public string Field2 { get; set; }
                public Guid Field3 { get; set; }
                public DateTime Field4 { get; set; }
            }
        }

        [Test]
        public void Can_create_table_with_string_longer_than_8000()
        {
            using(var con = ConnectionString.OpenDbConnection())
            {
                Assert.DoesNotThrow(
                    () =>
                    {
                        con.CreateTable<ModelWithIdAndLongString>(true);
                    }
                );
            }
        }

        [Test]
        public void Can_store_string_longer_than_8000_characters()
        {
            using(var con = ConnectionString.OpenDbConnection())
            {
                string longString = "";

                while(longString.Length < 9000)
                {
                    longString += "abcdefghijklmnopqrstuvwxyz";
                }

                longString = longString.Substring(0, 8500);

                Assert.That(longString.Length == 8500);

                ModelWithIdAndLongString model = new ModelWithIdAndLongString()
                {
                    Id = 1,
                    LongString = longString
                };

                con.CreateTable<ModelWithIdAndLongString>(true);
                con.Save(model);

                var result = con.GetById<ModelWithIdAndLongString>(1);
                con.DropTable<ModelWithIdAndLongString>();

                Console.WriteLine("Inserted: {0}, Retrieved: {1}", model.LongString, result.LongString);
                Assert.That(model.LongString.Equals(result.LongString));
            }
        }

        [Test]
        public void Complex_Types_Should_Use_Max_For_Undefined_String_Length()
        {
            var createTableSql = OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(ComplexType));
            Console.WriteLine("createTableSql: " + createTableSql);
            Assert.That(createTableSql.ToUpperInvariant().Contains("VARCHAR(250)"), Is.True);
            Assert.That(createTableSql.ToUpperInvariant().Contains("VARCHAR(8000)"), Is.True);
            Assert.That(createTableSql.ToUpperInvariant().Contains("VARCHAR(MAX)"), Is.True);
        }
    }
}
