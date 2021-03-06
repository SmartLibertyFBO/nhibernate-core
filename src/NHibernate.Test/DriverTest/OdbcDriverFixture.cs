using System;
using System.Collections;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Engine;
using NUnit.Framework;

namespace NHibernate.Test.DriverTest
{
	[TestFixture]
	public class OdbcDriverFixture : TestCase
	{
		protected override string MappingsAssembly => "NHibernate.Test";

		protected override IList Mappings => new[] { "DriverTest.MultiTypeEntity.hbm.xml" };

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is MsSql2000Dialect;
		}

		protected override bool AppliesTo(ISessionFactoryImplementor factory)
		{
			return factory.ConnectionProvider.Driver is OdbcDriver;
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.CreateQuery("delete from MultiTypeEntity").ExecuteUpdate();
				t.Commit();
			}
		}

		[Test]
		public void Crud()
		{
			// Should use default dimension for CRUD op because the mapping does not 
			// have dimensions specified.
			object savedId;
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				savedId = s.Save(
					new MultiTypeEntity
					{
						StringProp = "a",
						StringClob = "a",
						BinaryBlob = new byte[] { 1, 2, 3 },
						Binary = new byte[] { 4, 5, 6 },
						Currency = 123.4m,
						Double = 123.5d,
						Decimal = 789.5m,
						DecimalHighScale = 1234567890.0123456789m
					});
				t.Commit();
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var m = s.Get<MultiTypeEntity>(savedId);

				Assert.That(m.StringProp, Is.EqualTo("a"), "StringProp");
				Assert.That(m.StringClob, Is.EqualTo("a"), "StringClob");
				Assert.That(m.BinaryBlob, Is.EqualTo(new byte[] { 1, 2, 3 }), "BinaryBlob");
				Assert.That(m.Binary, Is.EqualTo(new byte[] { 4, 5, 6 }), "BinaryBlob");
				Assert.That(m.Currency, Is.EqualTo(123.4m), "Currency");
				Assert.That(m.Double, Is.EqualTo(123.5d).Within(0.0001d), "Double");
				Assert.That(m.Decimal, Is.EqualTo(789.5m), "Decimal");
				Assert.That(m.DecimalHighScale, Is.EqualTo(1234567890.0123456789m), "DecimalHighScale");

				m.StringProp = "b";
				m.StringClob = "b";
				m.BinaryBlob = new byte[] { 4, 5, 6 };
				m.Binary = new byte[] { 7, 8, 9 };
				m.Currency = 456.78m;
				m.Double = 987.6d;
				m.Decimal = 1323456.45m;
				m.DecimalHighScale = 9876543210.0123456789m;
				t.Commit();
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var m = s.Load<MultiTypeEntity>(savedId);

				Assert.That(m.StringProp, Is.EqualTo("b"), "StringProp");
				Assert.That(m.StringClob, Is.EqualTo("b"), "StringClob");
				Assert.That(m.BinaryBlob, Is.EqualTo(new byte[] { 4, 5, 6 }), "BinaryBlob");
				Assert.That(m.Binary, Is.EqualTo(new byte[] { 7, 8, 9 }), "BinaryBlob");
				Assert.That(m.Currency, Is.EqualTo(456.78m), "Currency");
				Assert.That(m.Double, Is.EqualTo(987.6d).Within(0.0001d), "Double");
				Assert.That(m.Decimal, Is.EqualTo(1323456.45m), "Decimal");
				Assert.That(m.DecimalHighScale, Is.EqualTo(9876543210.0123456789m), "DecimalHighScale");

				t.Commit();
			}
		}
	}
}
