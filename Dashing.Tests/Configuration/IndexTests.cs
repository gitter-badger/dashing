﻿namespace Dashing.Tests.Configuration {
    using System;
    using System.Configuration;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class IndexTests {
        [Fact]
        public void IndexExtensionWorks() {
            var config = NeedToDash.Configure(new ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient"));
            config.Setup<Blog>();
            config.Setup<User>();
            config.Setup<Post>().Index(p => new { p.Rating, p.Title });
            Assert.Equal(1, config.GetMap<Post>().Indexes.First(i => i.Columns.Count == 2).Columns.Count(c => c.Name == "Rating"));
            Assert.Equal(1, config.GetMap<Post>().Indexes.First(i => i.Columns.Count == 2).Columns.Count(c => c.Name == "Title"));
        }

        [Fact]
        public void ThrowOnIndexingIgnoredColumn() {
            var config = NeedToDash.Configure(new ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient"));
            config.Setup<Post>().Property(p => p.Title).Ignore();
            Assert.Throws<InvalidOperationException>(() => config.Setup<Post>().Index(p => p.Title));
        }

        [Fact]
        public void ThrowOnOddIndexExpression() {
            var config = NeedToDash.Configure(new ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient"));
            Assert.Throws<InvalidOperationException>(() => config.Setup<Post>().Index(p => new { Blah = p.Title }));
        }

        [Fact]
        public void UniqueAddedCorrectly() {
            var config = NeedToDash.Configure(new ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient"));
            config.Setup<Blog>();
            config.Setup<User>();
            config.Setup<Post>().Index(p => new { p.Rating, p.Title }, true);
            Assert.True(config.GetMap<Post>().Indexes.First(i => i.Columns.Count == 2).IsUnique);
        }
    }
}