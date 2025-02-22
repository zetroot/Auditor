﻿using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Description;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage.Collections;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections
{
    public abstract class CollectionTest
    {
        protected const string KnownId = nameof(KnownId);

        private readonly Collection<Community> collection;

        internal CollectionTest(Collection<Community> collection)
        {
            this.collection = collection;
        }

        [Fact]
        public void ShouldHaveName()
        {
            // Act
            var collectionName = collection.Name;

            // Assert
            Assert.NotEmpty(collectionName);
        }

        [Fact]
        public void ShouldHaveType()
        {
            // Act
            var collectionType = collection.CollectionType;

            // Assert
            Assert.Equal(typeof(Community), collectionType);
        }

        [Fact]
        public async Task ShouldLoadSingle()
        {
            // Act
            var document = await collection.LoadAsync(KnownId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(document);
            Assert.Equal(KnownId, document?.Id);
        }

        [Fact]
        public async Task ShouldReturnDefaultWhenLoadNotExistingSingle()
        {
            // Arrange
            const string unknownId = "unknown";

            // Act
            var document = await collection.LoadAsync(unknownId).ConfigureAwait(false);

            // Assert
            Assert.Null(document);
        }

        [Fact]
        public async Task ShouldLoadMany()
        {
            // Act
            var documentList = await collection.QueryAsync().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.True(documentList.Count > 1);
            var documents = AssertEx.ItemNotNull(documentList);
            Assert.All(documents, doc => Assert.StartsWith(KnownId, AssertEx.NotNull(doc.Id)));
        }

        [Fact]
        public async Task ShouldSkipWhenCantDeserialize()
        {
            // Arrange
            var badCollection = CreateCollectionWithBadSerializer();

            // Act
            var documents = await badCollection.QueryAsync().ToListAsync().ConfigureAwait(false);

            // Assert
            var maybeDocument = Assert.Single(documents);
            var document = AssertEx.NotNull(maybeDocument);
            Assert.StartsWith(KnownId, AssertEx.NotNull(document.Id));
        }

        internal abstract Collection<Community> CreateCollectionWithBadSerializer();

        protected static IDocumentSerializer<Community> CreateSerializer()
        {
            var serializerFactory = new DocumentSerializerFactory(ModelRegistry.Instance);
            return serializerFactory.Create<Community>();
        }
    }
}
