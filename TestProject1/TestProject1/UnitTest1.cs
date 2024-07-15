using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Reddit;
using Reddit.Models;
using Reddit.Repositories;
using System.Drawing.Printing;

namespace TestProject1
{
    public class PostsRepositoryTests
    {
        private ApplicationDbContext CreateContext()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Posts.AddRange(
                new Post { Id = 1, Title = "First Post", Content = "First post content", Upvotes = 10, Downvotes = 2 }, 
                new Post { Id = 2, Title = "Second Post", Content = "Second post content", Upvotes = 5, Downvotes = 1 },
                new Post { Id = 3, Title = "Third Post", Content = "Third post content", Upvotes = 20, Downvotes = 5 } 
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public void Test1()
        {

        }

        [Fact]
        public async Task GetPosts_ReturnsPagedPosts()
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: 1, pageSize: 2, searchTerm: null, SortTerm: null);

            Assert.Equal(2, pagedPosts.Items.Count);
            Assert.Equal(3, pagedPosts.TotalCount);
        }

        [Fact]
        public async Task GetPosts_SearchTermFiltersPosts()
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: 1, pageSize: 10, searchTerm: "Second", SortTerm: null);

            Assert.Single(pagedPosts.Items);
            Assert.Equal("Second Post", pagedPosts.Items.First().Title);
        }

        [Fact]
        public async Task GetPosts_SortTermSortsPosts()
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: 1, pageSize: 10, searchTerm: null, SortTerm: "popular", isAscending: false);

            Assert.Equal(3, pagedPosts.Items.Count);
            Assert.Equal("Third Post", pagedPosts.Items.First().Title);
        }

        [Fact]
        public async Task GetPosts_PositivitySortsPosts()
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: 1, pageSize: 10, searchTerm: null, SortTerm: "positivity", isAscending: false);

            Assert.Equal(3, pagedPosts.Items.Count);
            Assert.Equal("First Post", pagedPosts.Items.First().Title);
        }
        [Fact]
        public async Task GetPosts_InvalidPage_ThrowsArgumentOutOfRangeException()
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                repository.GetPosts(page: 0, pageSize: 10, searchTerm: null, SortTerm: null));

            Assert.Equal("page", exception.ParamName);
        }

        [Fact]
        public async Task GetPosts_InvalidPageSize_ThrowsArgumentOutOfRangeException()
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                repository.GetPosts(page: 1, pageSize: 0, searchTerm: null, SortTerm: null));

            Assert.Equal("pageSize", exception.ParamName);
        }

        [Theory]
        [InlineData(1,1)]
        [InlineData(2,1)]
        [InlineData(3,1)]
        public async Task HasNextPage(int page,int pageSize )
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: page, pageSize: pageSize, searchTerm: null, SortTerm: null);

            Assert.True(pagedPosts != null);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task HasPreviousPage(int page)
        {
            Assert.True(page > 1);
        }

        [Theory]
        [InlineData(1,1)]
        [InlineData(1,2)]
        [InlineData(1,3)]
        [InlineData(2,1)]
        [InlineData(3,1)]
        public async Task ListIsEmpty(int page, int pageSize)
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: page, pageSize: pageSize, searchTerm: null, SortTerm: null);

            Assert.True(pagedPosts == null);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task CountIsLargerThenPageSize(int pageSize)
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: 1, pageSize: pageSize, searchTerm: null, SortTerm: null);

            Assert.True(pagedPosts.Items.Count>pageSize);
        }

        [Theory]
        [InlineData(1,1)]
        [InlineData(-1,-1)]
        public async Task InvalidPageorPageSize(int page,int pageSize)
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: page, pageSize: pageSize, searchTerm: null, SortTerm: null);

            Assert.True(page>0 || pageSize>0);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(4, 1)]
        public async Task OutOfRange(int page,int pageSize)
        {
            using var context = CreateContext();
            var repository = new PostsRepository(context);
            var pagedPosts = await repository.GetPosts(page: page, pageSize: pageSize, searchTerm: null, SortTerm: null);

            Assert.True(pagedPosts.Items.Count/pageSize>0);
        }


    }
}