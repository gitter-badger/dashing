﻿namespace TopHat.Tests {
  using Moq;

  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class CUDTest : BaseQueryWriterTest {
    [Fact]
    public void InsertGivesGoodQuery() {
      var post = new Post { Title = "Hello" };
      this.GetTopHat().Insert(post);
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.Entity.Equals(post) && q.QueryType == QueryType.Insert)));
    }

    [Fact]
    public void UpdateGivesGoodQuery() {
      var post = new Post { Title = "Hello", PostId = 1 };
      this.GetTopHat().Update(post);
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.Entity.Equals(post) && q.QueryType == QueryType.Update)));
    }

    [Fact]
    public void DeleteEntityGivesGoodQuery() {
      var post = new Post { Title = "Hello", PostId = 1 };
      this.GetTopHat().Delete(post);
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.Entity.Equals(post) && q.QueryType == QueryType.Delete)));
    }

    [Fact]
    public void DeleteByIdGivesGoodQuery() {
      this.GetTopHat().Delete<Post>(1);
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.Entity.PostId == 1 && q.QueryType == QueryType.Delete)));
    }

    [Fact]
    public void WhereClauseUpdateExecutes() {
      this.GetTopHat().Delete<Post>().Where(p => p.PostId < 5);
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Delete && q.WhereClauses.Count == 1)));
    }

    [Fact]
    public void WhereClauseDeleteExecutes() {
      this.GetTopHat().Update<Post>().Where(p => p.PostId < 5);
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Update && q.WhereClauses.Count == 1)));
    }
  }
}