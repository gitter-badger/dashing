﻿namespace Dashing.Tests.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Tests.Annotations;
    using Dashing.Tests.TestDomain;
    using Dashing.Tests.TestDomain.OneToOne;

    using Moq;

    using Xunit;

    public class DefaultMapperTests {
        private readonly Mock<IConvention> mockConvention = new Mock<IConvention>();

        private readonly Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();

        private const string ExampleString = "foo";

        private const string UserId = "UserId";

        private const string Username = "Username";

        private const byte ExampleByte = 128;

        private const ushort ExampleUshort = 1024;

        [Fact]
        public void ConstructorThrowsOnNullConvention() {
            Assert.Throws<ArgumentNullException>(() => new DefaultMapper(null));
        }

        [Fact]
        public void DelegatesTableNameToConvention() {
            this.mockConvention.Setup(m => m.TableFor(typeof(User))).Returns(ExampleString).Verifiable();
            var target = this.MakeTarget();

            var map = target.MapFor<User>(this.mockConfiguration.Object);

            Assert.NotNull(map);
            Assert.Equal(ExampleString, map.Table);
            this.mockConvention.Verify();
        }

        [Fact]
        public void DelegatesSchemaNameToConvention() {
            this.mockConvention.Setup(m => m.SchemaFor(typeof(User))).Returns(ExampleString).Verifiable();
            var target = this.MakeTarget();

            var map = target.MapFor<User>(this.mockConfiguration.Object);

            Assert.NotNull(map);
            Assert.Equal(ExampleString, map.Schema);
            this.mockConvention.Verify();
        }

        [Fact]
        public void DelegatesPrimaryKeyNameToConvention() {
            this.mockConvention.Setup(m => m.PrimaryKeyFor(typeof(User), It.IsAny<IEnumerable<string>>())).Returns(Username).Verifiable();
            var target = this.MakeTarget();

            var map = target.MapFor<User>(this.mockConfiguration.Object);

            Assert.NotNull(map);
            Assert.NotNull(map.PrimaryKey);
            Assert.Equal(Username, map.PrimaryKey.Name);
            this.mockConvention.Verify();
        }

        [Fact]
        public void OnlyPassesGoodPrimaryKeyCandidatesToConvention() {
            // assemble
            IEnumerable<string> capturedProperties = null;
            this.mockConvention.Setup(m => m.PrimaryKeyFor(typeof(UserWithDodgyProperties), It.IsAny<IEnumerable<string>>()))
                .Callback<Type, IEnumerable<string>>((t, s) => capturedProperties = s)
                .Returns(Username);
            var target = this.MakeTarget();

            // act
            target.MapFor<UserWithDodgyProperties>(this.mockConfiguration.Object);

            // assert
            Assert.NotNull(capturedProperties);
            var cp = capturedProperties as string[] ?? capturedProperties.ToArray();
            Assert.Equal(2, cp.Length);
            Assert.Contains("UserWithDodgyPropertiesId", cp);
            Assert.Contains("GoodButNotTheId", cp);
        }

        private class UserWithDodgyProperties {
            [UsedImplicitly]
            private int backingDropBox;

            [UsedImplicitly]
            public int UserWithDodgyPropertiesId { get; set; }

            [UsedImplicitly]
            public User GoodButNotTheId { get; set; }

            [UsedImplicitly]
            public int Id
            {
                get
                {
                    return 0;
                }
            }

            [UsedImplicitly]
            public int DropBox
            {
                set
                {
                    this.backingDropBox = value;
                }
            }

            [UsedImplicitly]
            public IList<User> CollectionProperty { get; set; }
        }

        [Fact]
        public void DelegatesPrimaryKeyAutoGenerationToConvention() {
            this.mockConvention.Setup(m => m.PrimaryKeyFor(typeof(User), It.IsAny<IEnumerable<string>>())).Returns(UserId).Verifiable();
            this.mockConvention.Setup(m => m.IsPrimaryKeyAutoGenerated(It.IsAny<IColumn>())).Returns(true).Verifiable();
            var target = this.MakeTarget();

            var map = target.MapFor<User>(this.mockConfiguration.Object);

            Assert.NotNull(map);
            Assert.NotNull(map.PrimaryKey);
            Assert.True(map.PrimaryKey.IsAutoGenerated);
            this.mockConvention.Verify();
        }

        [Fact]
        public void StringLengthDelegatesToConvention() {
            this.mockConvention.Setup(m => m.StringLengthFor(typeof(User), "Username")).Returns(ExampleUshort).Verifiable();
            var target = this.MakeTarget();

            var map = target.MapFor<User>(this.mockConfiguration.Object);

            Assert.NotNull(map);
            var property = map.Property(u => u.Username);
            Assert.NotNull(property);
            Assert.Equal(ExampleUshort, property.Length);
            this.mockConvention.Verify();
        }

        [Fact]
        public void DecimalPrecisionDelegatesToConvention() {
            this.mockConvention.Setup(m => m.DecimalPrecisionFor(typeof(User), "HeightInMeters")).Returns(ExampleByte).Verifiable();
            var target = this.MakeTarget();

            var map = target.MapFor<User>(this.mockConfiguration.Object);

            Assert.NotNull(map);
            var property = map.Property(u => u.HeightInMeters);
            Assert.NotNull(property);
            Assert.Equal(ExampleByte, property.Precision);
            this.mockConvention.Verify();
        }

        [Fact]
        public void DecimalScaleDelegatesToConvention() {
            this.mockConvention.Setup(m => m.DecimalScaleFor(typeof(User), "HeightInMeters")).Returns(ExampleByte).Verifiable();
            var target = this.MakeTarget();

            var map = target.MapFor<User>(this.mockConfiguration.Object);

            Assert.NotNull(map);
            var property = map.Property(u => u.HeightInMeters);
            Assert.NotNull(property);
            Assert.Equal(ExampleByte, property.Scale);
            this.mockConvention.Verify();
        }

        [Fact]
        public void PrimaryKeyIsFlaggedAsSuch() {
            this.mockConvention.Setup(m => m.PrimaryKeyFor(typeof(User), It.IsAny<IEnumerable<string>>())).Returns(Username).Verifiable();

            var map = this.Map<User>();
            Assert.True(map.PrimaryKey.IsPrimaryKey);
        }

        [Fact]
        public void ColumnTypeIsSet() {
            var property = this.MapAndGetProperty<Post, string>(u => u.Content);
            Assert.Equal(typeof(string), property.Type);
        }

        [Fact]
        public void ColumnNameIsSet() {
            var property = this.MapAndGetProperty<Post, string>(u => u.Content);
            Assert.Equal("Content", property.Name);
        }

        [Fact]
        public void NullableColumnIsNullable() {
            var property = this.MapAndGetProperty<Post, Blog>(u => u.Blog);
            Assert.True(property.IsNullable);
        }

        [Fact]
        public void NullableIntColumnIsNullable() {
            var property = this.MapAndGetProperty<NullableMappingsTestClass, int?>(u => u.NullableInt);
            Assert.True(property.IsNullable);
        }

        [Fact]
        public void NotNullableIntColumnIsNotNullable() {
            var property = this.MapAndGetProperty<NullableMappingsTestClass, int>(u => u.NotNullableInt);
            Assert.False(property.IsNullable);
        }

        [Fact]
        public void NullableDecimalColumnIsNullable() {
            var property = this.MapAndGetProperty<NullableMappingsTestClass, decimal?>(u => u.NullableDecimal);
            Assert.True(property.IsNullable);
        }

        [Fact]
        public void NotNullableDecimalColumnIsNotNullable() {
            var property = this.MapAndGetProperty<NullableMappingsTestClass, decimal>(u => u.NotNullableDecimal);
            Assert.False(property.IsNullable);
        }

        [Fact]
        public void StringColumnIsNullable() {
            var property = this.MapAndGetProperty<Post, string>(u => u.Content);
            Assert.True(property.IsNullable);
        }

        [Fact]
        public void NotNullableColumnIsNotNullable() {
            var property = this.MapAndGetProperty<Post, decimal>(u => u.Rating);
            Assert.False(property.IsNullable);
        }

        [Fact]
        public void NonEntityColumnDbTypeIsSet() {
            var property = this.MapAndGetProperty<Post, string>(u => u.Content);
            Assert.Equal(DbType.String, property.DbType);
        }

        [Fact]
        public void NonEntityColumnDbNameIsSet() {
            var property = this.MapAndGetProperty<Post, string>(u => u.Content);
            Assert.Equal("Content", property.DbName);
        }

        [Fact]
        public void NonEntityColumnRelationshipIsNone() {
            var property = this.MapAndGetProperty<Post, string>(u => u.Content);
            Assert.Equal(RelationshipType.None, property.Relationship);
        }

        [Fact]
        public void EntityColumnDbNameIsSet() {
            var property = this.MapAndGetProperty<Post, User>(p => p.Author);
            Assert.Equal("AuthorId", property.DbName);
        }

        [Fact]
        public void EntityColumnRelationshipIsManyToOne() {
            var property = this.MapAndGetProperty<Post, User>(u => u.Author);
            Assert.Equal(RelationshipType.ManyToOne, property.Relationship);
        }

        [Fact]
        public void EntityCollectionColumnRelationshipIsOneToMany() {
            var property = this.MapAndGetProperty<Post, IList<Comment>>(p => p.Comments);
            Assert.Equal(RelationshipType.OneToMany, property.Relationship);
        }

        [Fact]
        public void EntityOneToOneMappedToManyToOneIfFirst() {
            var mapper = this.MakeTarget();
            this.mockConfiguration.Setup(c => c.HasMap(typeof(OneToOneRight))).Returns(false);
            var map = mapper.MapFor<OneToOneLeft>(this.mockConfiguration.Object);
            Assert.Equal(RelationshipType.ManyToOne, map.Columns.First(c => c.Key == "Right").Value.Relationship);
        }

        [Fact]
        public void EntityOneToOneMappedToOneToOneIfSecond() {
            var mapper = this.MakeTarget();
            var mapResult = new Map<OneToOneRight>();
            mapResult.Columns.Add("OneToOneRightId", new Column<int> { Name = "OneToOneRightId" });
            mapResult.Columns.Add("Left", new Column<OneToOneLeft> { Name = "Left", Relationship = RelationshipType.ManyToOne });
            this.mockConfiguration.Setup(c => c.HasMap(typeof(OneToOneRight))).Returns(true);
            this.mockConfiguration.Setup(c => c.GetMap(typeof(OneToOneRight))).Returns(mapResult);
            var map = mapper.MapFor<OneToOneLeft>(this.mockConfiguration.Object);
            Assert.Equal(RelationshipType.OneToOne, map.Columns.First(c => c.Key == "Right").Value.Relationship);
        }

        [Fact]
        public void EntityOneToOneFixedIfMappedFirst() {
            var mapper = this.MakeTarget();
            var manyToOneColumnToFix = new Column<OneToOneLeft> { Name = "Left", Relationship = RelationshipType.ManyToOne };
            var mapResult = new Map<OneToOneRight>();
            mapResult.Columns.Add("OneToOneRightId", new Column<int> { Name = "OneToOneRightId" });
            mapResult.Columns.Add("Left", manyToOneColumnToFix);
            this.mockConfiguration.Setup(c => c.HasMap(typeof(OneToOneRight))).Returns(true);
            this.mockConfiguration.Setup(c => c.GetMap(typeof(OneToOneRight))).Returns(mapResult);

            var map = mapper.MapFor<OneToOneLeft>(this.mockConfiguration.Object);
            Assert.Equal(RelationshipType.OneToOne, manyToOneColumnToFix.Relationship);
        }

        private DefaultMapper MakeTarget() {
            return new DefaultMapper(this.mockConvention.Object);
        }

        private Column<TProperty> MapAndGetProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
            var map = this.Map<T>();
            var property = map.Property(propertyExpression);
            Assert.NotNull(property);
            return property;
        }

        private IMap<T> Map<T>() {
            var target = this.MakeTarget();
            var map = target.MapFor<T>(this.mockConfiguration.Object);
            Assert.NotNull(map);
            return map;
        }

        private class NullableMappingsTestClass {
            public virtual int? NullableInt { get; set; }

            public virtual int NotNullableInt { get; set; }

            public virtual decimal? NullableDecimal { get; set; }

            public virtual decimal NotNullableDecimal { get; set; }
        }
    }
}