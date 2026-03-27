using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Core.Interfaces;
using Core.Services;
using Domain.Entities;
using Core.DTOs.Auth;
using Microsoft.VisualBasic;
using Core.DTOs.Message;

namespace Tests.Unit.Services;

public class AuthServiceTests : TestBase
{
    private readonly Mock<IEmailQueue> _mockEmailQueue;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<AuthService>> _mockLogger;

    public AuthServiceTests()
    { 
        _mockEmailQueue = new Mock<IEmailQueue>();
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockConfig.Setup(c => c["jwt:key"]).Returns("super_tajni_kljuc_od_32_karaktera_minimum");
    }

    [Theory]
    [InlineData("Nikolassss", "pogresna_lozinka")]
    [InlineData("Nepostojeci", "nikola123")]
    [InlineData("", "")]
    public async Task LoginAsync_WithInvalidCredentials_ShouldThrowArgumentException(string username, string password)
    {
        using var _context = GetDbContext();
        var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword("nikola123");
        var testUser = new User
        {
            Username = "Nikolassss",
            FirstName = "Nikola",
            LastName = "Nikolic",
            Age = 30,
            Email = "nnikola2409@gmail.com",
            Password = hashedPassword
        };
        _context._users.Add(testUser);
        await _context.SaveChangesAsync();

        var authService = new AuthService(
            _context,
            _mockEmailQueue.Object,
            _mockConfig.Object,
            _mockLogger.Object
        );

        var loginData = new LoginData( username, password );

        Func<Task> act = async () => await authService.LoginAsync(loginData, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
             .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnUserWithAccessToken()
    {
        using var _context = GetDbContext();
        var password = "TacnaLozinka123";
        var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        var newUser = new User
        {
            Username = "Nikolalala",
            FirstName = "Nikola",
            LastName = "Nikolic",
            Age = 30,
            Email = "nikola@test.com",
            Password = hashedPassword,
        };

        _context._users.Add(newUser);
        await _context.SaveChangesAsync();

        var authService = new AuthService(
            _context,
            _mockEmailQueue.Object,
            _mockConfig.Object,
            _mockLogger.Object
        );

        var loginData = new LoginData(newUser.Username, password);

        var result = await authService.LoginAsync(loginData, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data.Username.Should().Be("Nikolalala");
        result.Data.AccessToken.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_PasswordsDoNotMatch_ShouldThrowArgumentException()
    {
        using var _context = GetDbContext();

        var newUser = new RegisterData
        (
            "Nikolalala",
            "Nikola",
            "Nikolic",
            30,
            "nikola@test.com",
            "nekaLozinka123",
            "RazlicitaLozinka"
        );

        var authService = new AuthService(_context, _mockEmailQueue.Object, _mockConfig.Object, _mockLogger.Object);

        Func<Task> act = async () => await authService.RegisterAsync(newUser, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Passwords do not match.");
    }

    [Theory]
    [InlineData("Nikola12345", "nikola123@gmail.com")]
    [InlineData("nikolassssss", "nikola@gmail.com")]
    public async Task RegisterAsync_UserAlreadyExists_ShouldThrowArgumentException(string username, string email)
    {
        using var _context = GetDbContext();

        _context._users.Add(new User { Username = "Nikola12345", FirstName = "Nikola", LastName = "Nikolic", Age = 30, Email = "nikola@gmail.com", Password = "nikola123" });

        await _context.SaveChangesAsync();

        var request = new RegisterData(username, "Nikola", "Nikolic", 32, email, "nikola123", "nikola123");

        var authService = new AuthService(_context, _mockEmailQueue.Object, _mockConfig.Object, _mockLogger.Object);

        Func<Task> act = async () => await authService.RegisterAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_EmailQueueFails_ShouldStillRegisterUser()
    {
        using var _context = GetDbContext();

        _mockEmailQueue.Setup(x => x.QueueEmail(It.IsAny<EmailMessage>())).Throws(new Exception("Server down"));

        var authService = new AuthService(_context, _mockEmailQueue.Object, _mockConfig.Object, _mockLogger.Object);

        var request = new RegisterData("Nikolassss", "Nikola", "Nikolic", 32, "nikola123@gmail.com", "nikola123", "nikola123");

        var result = await authService.RegisterAsync(request, CancellationToken.None);

        result.Success.Should().BeTrue();
        _context._users.Any(u => u.Username == "Nikolassss").Should().BeTrue();

        _mockLogger.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Email queue is currently not working")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
        Times.Once);
    }
}