using Core.DTOs.Conversation;
using Core.Interfaces;
using Core.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Unit.Services
{
    public class ConversationServiceTests : TestBase
    {
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;

        public ConversationServiceTests()
        {
            _mockCurrentUserService = new Mock<ICurrentUserService>();
        }                           

        [Fact]
        public async Task CreateConversation_NewPrivateChat_ShouldCreateAndReturnSuccess()
        {
            using var _context = GetDbContext();

            int myId = 1;
            int friendId = 2;

            _mockCurrentUserService.Setup(x => x.GetCurrentUserId()).Returns(myId);

            _context._users.Add(new User { Id = myId, Username = "Nikolassss", FirstName = "Nikola", LastName = "Nikolic", Age = 32, Email = "nikola@gmail.com", Password = "nikola123" });
            _context._users.Add(new User { Id = friendId, Username = "friend12345", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan@gmail.com", Password = "bogdan123" });

            await _context.SaveChangesAsync();

            var service = new ConversationService(_context, _mockCurrentUserService.Object);
            var request = new CreateConversationData(null, new List<int> { friendId });

            var result = await service.CreateConversation(request, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.Data.IsGroup.Should().BeFalse();
            result.Data.Title.Should().Be("Bogdan Bogdanovic");

            _context._participations.Count().Should().Be(2);
            _context._conversation.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateConversation_WithInvalidCurrentUserId_ShouldThrowUnauthorizedAccessException()
        {
            using var _context = GetDbContext();

            int myId = 0;
            int myFriendId = 1;

            _mockCurrentUserService.Setup(x => x.GetCurrentUserId()).Returns(myId);

            var service = new ConversationService(_context, _mockCurrentUserService.Object);
            var request = new CreateConversationData("Project", new List<int> { myFriendId });

            Func<Task> act = async () => await service.CreateConversation(request, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Unauthorized");
        }

        [Fact]
        public async Task CreateConversation_GropChat_ShouldReturnSuccess()
        {
            using var _context = GetDbContext();

            int myId = 1;
            List<int> myFriendIds = new List<int> { 2, 3, 4 };

            _context._users.Add(new User { Id = myId, Username = "Nikolasssss", FirstName = "Nikola", LastName = "Nikolic", Age = 32, Email = "nikola@gmail.com", Password = "nikola123" });

            _context._users.Add(new User { Id = myFriendIds[0], Username = "friend12345", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan1@gmail.com", Password = "bogdan123" });
            _context._users.Add(new User { Id = myFriendIds[1], Username = "friend123456", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan2@gmail.com", Password = "bogdan123" });
            _context._users.Add(new User { Id = myFriendIds[2], Username = "friend1234567", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan3@gmail.com", Password = "bogdan123" });
            await _context.SaveChangesAsync();

            _mockCurrentUserService.Setup(x => x.GetCurrentUserId()).Returns(myId);

            var service = new ConversationService(_context, _mockCurrentUserService.Object);
            var request = new CreateConversationData("Project X", myFriendIds);

            var result = await service.CreateConversation(request, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.Data.IsGroup.Should().BeTrue();
            result.Data.Title.Should().Be("Project X");

            _context._participations.Count().Should().Be(4);
            _context._conversation.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateConversation_WithUnexistingIds_ShouldReturnArgumentException()
        {
            using var _context = GetDbContext();

            int myId = 1;
            List<int> myFriendIds = new List<int> { 2, 3, 4 };

            _context._users.Add(new User { Id = myId, Username = "Nikolasssss", FirstName = "Nikola", LastName = "Nikolic", Age = 32, Email = "nikola@gmail.com", Password = "nikola123" });

            _context._users.Add(new User { Id = 7, Username = "friend12345", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan1@gmail.com", Password = "bogdan123" });
            _context._users.Add(new User { Id = 2, Username = "friend123456", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan2@gmail.com", Password = "bogdan123" });
            _context._users.Add(new User { Id = 3, Username = "friend1234567", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan3@gmail.com", Password = "bogdan123" });
            await _context.SaveChangesAsync();

            _mockCurrentUserService.Setup(x => x.GetCurrentUserId()).Returns(myId);

            var service = new ConversationService(_context, _mockCurrentUserService.Object);
            var request = new CreateConversationData("Project X", myFriendIds);

            Func<Task> act = async () => await service.CreateConversation(request, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("One or more participants do not exist");
        }

        [Fact]
        public async Task GetConversation_WithInvalidCurrentUserId_SholdThrowUnauthorizedAccessException()
        {
            using var _context = GetDbContext();
            int myId = 0;
            _mockCurrentUserService.Setup(x => x.GetCurrentUserId()).Returns(myId);

            _context._conversation.Add(new Conversation { Id = 1, Title = "Project" });

            var service = new ConversationService(_context, _mockCurrentUserService.Object);

            Func<Task> act = async () => await service.GetConversation(1, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Unauthorized");
        }

        [Fact]
        public async Task GetConversation_ValidId_ShouldReturnDetailsWithMessagesAndParticipants()
        {
            using var context = GetDbContext();
            int myId = 1;
            int friendId = 2;
            int conversationId = 10;

            _mockCurrentUserService.Setup(x => x.GetCurrentUserId()).Returns(myId);

            var me = new User { Id = myId, Username = "nikola123", FirstName = "Nikola", LastName = "Nikolic", Age = 30, Email="nikola@gmail.com", Password="nikola123" };
            var friend = new User { Id = friendId, Username = "bogdan123", FirstName = "Bogdan", LastName = "Bogdanovic", Age = 32, Email = "bogdan@gmail.com", Password = "bogdan123" };
            context._users.AddRange(me, friend);

            var conversation = new Conversation
            {
                Id = conversationId,
                AdminId = myId,
                IsGroup = false
            };
            context._conversation.Add(conversation);

            var participations = new List<Participation>
            {
                new Participation { ConversationId = conversationId, UserId = myId },
                new Participation { ConversationId = conversationId, UserId = friendId }
            };
            context._participations.AddRange(participations);

            var message = new Message
            {
                Id = 1,
                ConversationId = conversationId,
                AuthorId = friendId,
                Content = "Zdravo!",
                CreatedAt = DateTime.UtcNow,
                Author = friend
            };
            context._messages.Add(message);

            await context.SaveChangesAsync();

            var service = new ConversationService(context, _mockCurrentUserService.Object);

            var result = await service.GetConversation(conversationId, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(conversationId);

            result.Data.Messages.Should().HaveCount(1);
            result.Data.Messages.First().Content.Should().Be("Zdravo!");
            result.Data.Messages.First().AuthorUsername.Should().Be("bogdan123");

            result.Data.Participants.Should().HaveCount(2);
            result.Data.Participants.Select(p => p.Username).Should().Contain(new[] { "nikola123", "bogdan123" });
        }
    }
}
