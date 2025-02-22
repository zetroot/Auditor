﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.IO;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.FileSystem
{
    [CollectionDefinition(Name)]
    public sealed class FileSystemFixture : ICollectionFixture<FileSystemFixture>, IDisposable
    {
        public const string Name = nameof(FileSystemFixture);

        private readonly TempFileSystem temp;

        public FileSystemFixture()
        {
            temp = TempFileSystem.Create();
            PhysicalRoot = PhysicalFileSystem.ForDirectory(temp.Root);
            MemoryRoot = MemoryFileSystem.ForDirectory(AbsolutePath.Root.FullName);
            AllRoots = new[] { PhysicalRoot, MemoryRoot };

            InitializeAsync(PhysicalRoot).GetAwaiter().GetResult();
            InitializeAsync(MemoryRoot).GetAwaiter().GetResult();
        }

        public IDirectory PhysicalRoot { get; }

        public IDirectory MemoryRoot { get; }

        public IReadOnlyList<IDirectory> AllRoots { get; }

        public void Dispose()
        {
            temp.Dispose();
        }

        private static async Task InitializeAsync(IDirectory root)
        {
            // A --→ A1 -→ a10.txt
            //   |-→ A2 -→ a20.txt
            //   |-→ A3 -→ a30.txt
            //
            // B --→ B1 -→ b10.txt
            //   |-→ B2 --→ b20.txt
            //          |-→ b21.txt
            //
            // C -→ c0.txt

            await CreateFileAsync(root, Path.Combine("A", "A1", "a10.txt")).ConfigureAwait(false);
            await CreateFileAsync(root, Path.Combine("A", "A2", "a20.txt")).ConfigureAwait(false);
            await CreateFileAsync(root, Path.Combine("A", "A3", "a30.txt")).ConfigureAwait(false);
            await CreateFileAsync(root, Path.Combine("B", "B1", "b10.txt")).ConfigureAwait(false);
            await CreateFileAsync(root, Path.Combine("B", "B2", "b20.txt")).ConfigureAwait(false);
            await CreateFileAsync(root, Path.Combine("B", "B2", "b21.txt")).ConfigureAwait(false);
            await CreateFileAsync(root, Path.Combine("C", "c0.txt")).ConfigureAwait(false);
        }

        private static async ValueTask<IWritableFile> CreateFileAsync(IDirectory root, string relativeFilePath)
        {
            var fileParentDirectoryName = Path.GetDirectoryName(relativeFilePath);
            var fileName = Path.GetFileName(relativeFilePath);
            var directory = root;

            if (fileParentDirectoryName != null)
            {
                directory = root.GetDirectory(fileParentDirectoryName);
            }

            var file = directory.GetFile(fileName);
            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);

            var writableFile = await file.RequestWriteAccessAsync().ConfigureAwait(false);
            writableFile = AssertEx.NotNull(writableFile);

            var fileStream = await writableFile.OpenForWriteAsync().ConfigureAwait(false);
            await using (fileStream.ConfigureAwait(false))
            {
                var fileWriter = new StreamWriter(fileStream);
                await using (fileWriter.ConfigureAwait(false))
                {
                    await fileWriter.WriteAsync(file.FullName).ConfigureAwait(false);
                }
            }

            fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.True(fileExists);

            return writableFile;
        }
    }
}
