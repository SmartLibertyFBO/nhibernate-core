﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NHibernate.Driver;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1869
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		private Keyword _keyword;

		protected override bool AppliesTo(Engine.ISessionFactoryImplementor factory)
		{
		   return factory.ConnectionProvider.Driver.SupportsMultipleQueries;
		}

		protected override void OnTearDown()
		{
			using (var session = Sfi.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from NodeKeyword");
				session.Delete("from Keyword");

				transaction.Commit();
			}
		}

		[Test]
		public async Task TestAsync()
		{
			using (var session = Sfi.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				_keyword = new Keyword();
				await (session.SaveAsync(_keyword));

				var nodeKeyword = new NodeKeyword();
				nodeKeyword.NodeId = 1;
				nodeKeyword.Keyword = _keyword;
				await (session.SaveAsync(nodeKeyword));

				await (transaction.CommitAsync());
			}

			using (var session = Sfi.OpenSession())
			{
				//If uncomment the line below the test will pass
				//GetResult(session);
				var result = await (GetResultAsync(session));
				Assert.That(result, Has.Count.EqualTo(2));
				Assert.That(result[0], Has.Count.EqualTo(1));
				Assert.That(result[1], Has.Count.EqualTo(1));
			}
		}

		private Task<IList> GetResultAsync(ISession session, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var query1 = session.CreateQuery("from NodeKeyword nk");
				var query2 = session.CreateQuery("from NodeKeyword nk");

				var multi = session.CreateMultiQuery();
				multi.Add(query1).Add(query2);
				return multi.ListAsync(cancellationToken);
			}
			catch (System.Exception ex)
			{
				return Task.FromException<IList>(ex);
			}
		}
	}
}
