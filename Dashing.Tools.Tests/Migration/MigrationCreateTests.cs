﻿namespace Dashing.Tools.Tests.Migration {
    using System.Collections.Generic;
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.Migration;
    using Dashing.Tools.Tests.TestDomain;

    using Xunit;

    public class MigrationCreateTests {
        [Fact]
        public void CreateTableWorks() {
            var migrator = MakeMigrator();
            var config = new SimpleClassConfig();
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(new IMap[] { }, config.Maps, null, null, new string[0], out warnings, out errors);
            Assert.Equal(
                "create table [SimpleClasses] ([SimpleClassId] int not null identity(1,1) primary key, [Name] nvarchar(255) null, [CreatedDate] datetime not null default (current_timestamp));\r\n",
                script);
        }

        [Fact]
        public void SelfReferencingWorks() {
            var migrator = MakeMigrator();
            var config = new MutableConfiguration(ConnectionString);
            config.Add<Category>();
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(new IMap[] { }, config.Maps, null, null, new string[0], out warnings, out errors);
            Assert.Equal(@"create table [Categories] ([CategoryId] int not null identity(1,1) primary key, [ParentId] int null, [Name] nvarchar(255) null);
alter table [Categories] add constraint fk_Category_Category_Parent foreign key ([ParentId]) references [Categories]([CategoryId]);
create index [idx_Category_Parent] on [Categories] ([ParentId]);
", script);
        }

        [Fact]
        public void PairWorks() {
            var migrator = MakeMigrator();
            var config = new MutableConfiguration(ConnectionString);
            config.Add<Pair>();
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(new IMap[] { }, config.Maps, null, null, new string[0], out warnings, out errors);
            Assert.Equal(@"create table [Pairs] ([PairId] int not null identity(1,1) primary key, [ReferencesId] int null, [ReferencedById] int null);
alter table [Pairs] add constraint fk_Pair_Pair_References foreign key ([ReferencesId]) references [Pairs]([PairId]);
alter table [Pairs] add constraint fk_Pair_Pair_ReferencedBy foreign key ([ReferencedById]) references [Pairs]([PairId]);
create index [idx_Pair_References] on [Pairs] ([ReferencesId]);
create index [idx_Pair_ReferencedBy] on [Pairs] ([ReferencedById]);
", script);
        }

        [Fact]
        public void OneToOneWorks() {
            var migrator = MakeMigrator();
            var config = new MutableConfiguration(ConnectionString);
            config.Add<OneToOneLeft>();
            config.Add<OneToOneRight>();
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(new IMap[] { }, config.Maps, null, null, new string[0], out warnings, out errors);
            Assert.Equal(@"create table [OneToOneLefts] ([OneToOneLeftId] int not null identity(1,1) primary key, [RightId] int null, [Name] nvarchar(255) null);
create table [OneToOneRights] ([OneToOneRightId] int not null identity(1,1) primary key, [LeftId] int null, [Name] nvarchar(255) null);
alter table [OneToOneLefts] add constraint fk_OneToOneLeft_OneToOneRight_Right foreign key ([RightId]) references [OneToOneRights]([OneToOneRightId]);
alter table [OneToOneRights] add constraint fk_OneToOneRight_OneToOneLeft_Left foreign key ([LeftId]) references [OneToOneLefts]([OneToOneLeftId]);
create index [idx_OneToOneLeft_Right] on [OneToOneLefts] ([RightId]);
create index [idx_OneToOneRight_Left] on [OneToOneRights] ([LeftId]);
",
                script);
        }

        [Fact]
        public void ComplexDomainBuilds() {
            var migrator = MakeMigrator();
            var config = new MutableConfiguration(ConnectionString);
            config.AddNamespaceOf<Post>();
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(new IMap[] { }, config.Maps, null, null, new string[0], out warnings, out errors);
            Assert.Equal(@"create table [Blogs] ([BlogId] int not null identity(1,1) primary key, [Title] nvarchar(255) null, [CreateDate] datetime not null default (current_timestamp), [Description] nvarchar(255) null);
create table [Categories] ([CategoryId] int not null identity(1,1) primary key, [ParentId] int null, [Name] nvarchar(255) null);
create table [Comments] ([CommentId] int not null identity(1,1) primary key, [Content] nvarchar(255) null, [PostId] int null, [UserId] int null, [CommentDate] datetime not null default (current_timestamp));
create table [Likes] ([LikeId] int not null identity(1,1) primary key, [UserId] int null, [CommentId] int null);
create table [OneToOneLefts] ([OneToOneLeftId] int not null identity(1,1) primary key, [RightId] int null, [Name] nvarchar(255) null);
create table [OneToOneRights] ([OneToOneRightId] int not null identity(1,1) primary key, [LeftId] int null, [Name] nvarchar(255) null);
create table [Pairs] ([PairId] int not null identity(1,1) primary key, [ReferencesId] int null, [ReferencedById] int null);
create table [Posts] ([PostId] int not null identity(1,1) primary key, [Title] nvarchar(255) null, [Content] nvarchar(255) null, [Rating] decimal(18,10) not null default (0), [AuthorId] int null, [BlogId] int null, [DoNotMap] bit not null default (0));
create table [PostTags] ([PostTagId] int not null identity(1,1) primary key, [PostId] int null, [TagId] int null);
create table [SimpleClasses] ([SimpleClassId] int not null identity(1,1) primary key, [Name] nvarchar(255) null, [CreatedDate] datetime not null default (current_timestamp));
create table [Tags] ([TagId] int not null identity(1,1) primary key, [Content] nvarchar(255) null);
create table [Users] ([UserId] int not null identity(1,1) primary key, [Username] nvarchar(255) null, [EmailAddress] nvarchar(255) null, [Password] nvarchar(255) null, [IsEnabled] bit not null default (0), [HeightInMeters] decimal(18,10) not null default (0));
alter table [Categories] add constraint fk_Category_Category_Parent foreign key ([ParentId]) references [Categories]([CategoryId]);
alter table [Comments] add constraint fk_Comment_Post_Post foreign key ([PostId]) references [Posts]([PostId]);
alter table [Comments] add constraint fk_Comment_User_User foreign key ([UserId]) references [Users]([UserId]);
alter table [Likes] add constraint fk_Like_User_User foreign key ([UserId]) references [Users]([UserId]);
alter table [Likes] add constraint fk_Like_Comment_Comment foreign key ([CommentId]) references [Comments]([CommentId]);
alter table [OneToOneLefts] add constraint fk_OneToOneLeft_OneToOneRight_Right foreign key ([RightId]) references [OneToOneRights]([OneToOneRightId]);
alter table [OneToOneRights] add constraint fk_OneToOneRight_OneToOneLeft_Left foreign key ([LeftId]) references [OneToOneLefts]([OneToOneLeftId]);
alter table [Pairs] add constraint fk_Pair_Pair_References foreign key ([ReferencesId]) references [Pairs]([PairId]);
alter table [Pairs] add constraint fk_Pair_Pair_ReferencedBy foreign key ([ReferencedById]) references [Pairs]([PairId]);
alter table [Posts] add constraint fk_Post_User_Author foreign key ([AuthorId]) references [Users]([UserId]);
alter table [Posts] add constraint fk_Post_Blog_Blog foreign key ([BlogId]) references [Blogs]([BlogId]);
alter table [PostTags] add constraint fk_PostTag_Post_Post foreign key ([PostId]) references [Posts]([PostId]);
alter table [PostTags] add constraint fk_PostTag_Tag_Tag foreign key ([TagId]) references [Tags]([TagId]);
create index [idx_Category_Parent] on [Categories] ([ParentId]);
create index [idx_Comment_Post] on [Comments] ([PostId]);
create index [idx_Comment_User] on [Comments] ([UserId]);
create index [idx_Like_User] on [Likes] ([UserId]);
create index [idx_Like_Comment] on [Likes] ([CommentId]);
create index [idx_OneToOneLeft_Right] on [OneToOneLefts] ([RightId]);
create index [idx_OneToOneRight_Left] on [OneToOneRights] ([LeftId]);
create index [idx_Pair_References] on [Pairs] ([ReferencesId]);
create index [idx_Pair_ReferencedBy] on [Pairs] ([ReferencedById]);
create index [idx_Post_Author] on [Posts] ([AuthorId]);
create index [idx_Post_Blog] on [Posts] ([BlogId]);
create index [idx_PostTag_Post] on [PostTags] ([PostId]);
create index [idx_PostTag_Tag] on [PostTags] ([TagId]);
",
                script);
        }

        private static Migrator MakeMigrator() {
            var migrator = new Migrator(
                new CreateTableWriter(new SqlServerDialect()),
                new DropTableWriter(new SqlServerDialect()),
                new AlterTableWriter(new SqlServerDialect()));
            return migrator;
        }

        private static ConnectionStringSettings ConnectionString {
            get {
                return new ConnectionStringSettings("DefaultDb", string.Empty, "System.Data.SqlClient");
            }
        }

        private class SimpleClassConfig : DefaultConfiguration {
            public SimpleClassConfig()
                : base(ConnectionString) {
                this.Add<SimpleClass>();
            }
        }
    }
}