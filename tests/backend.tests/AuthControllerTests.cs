using backend.Controllers;
using backend.Db.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;


namespace backend.tests;

public class AuthControllerTests
{
    // helpers om Usermanager en SignInmanager te mocken

    private Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>> (
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<User>>().Object,
            new List<IUserValidator<User>>(),
            new List<IPasswordValidator<User>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object
        );
    }

    private Mock<SignInManager<User>> CreateSignInManagerMock(UserManager<User> userManager)
    {
        return new Mock<SignInManager<User>> (
            userManager,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<User>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<User>>().Object
        );
    }


    // Login tests
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenEmailNotFound()
    {
        // arrange
        var userManagerMock = CreateUserManagerMock();
        var signInMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock
            .Setup(x => x.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((User?)null);

        var controller = new AuthController(userManagerMock.Object, signInMock.Object);

        var dto = new LoginDto
        {
            Email = "missing@example.com",
            Password = "password"
        };

        // Act
        var result = await controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordInvalid()
    {
        // Arrange
        var fakeUser = new User
        {
            Email = "test@example.com",
            Name = "Tester"
        };

        var userManagerMock = CreateUserManagerMock();
        var signInMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock
            .Setup(x => x.FindByEmailAsync(fakeUser.Email))
            .ReturnsAsync(fakeUser);

        signInMock
            .Setup(x => x.CheckPasswordSignInAsync(fakeUser, "wrong", false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var controller = new AuthController(userManagerMock.Object, signInMock.Object);

        var dto = new LoginDto
        {
            Email = fakeUser.Email,
            Password = "wrong"
        };

        // Act
        var result = await controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsValid()
    {
       // Arrange
        var fakeUser = new User
        {
            Email = "test@example.com",
            Name = "Tester"
        };

        var userManagerMock = CreateUserManagerMock();
        var signInMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock
            .Setup(x => x.FindByEmailAsync(fakeUser.Email))
            .ReturnsAsync(fakeUser);

        signInMock
            .Setup(x => x.CheckPasswordSignInAsync(fakeUser, "correct", false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = new AuthController(userManagerMock.Object, signInMock.Object);

        var dto = new LoginDto
        {
            Email = fakeUser.Email,
            Password = "correct"
        };

        // Act
        var result = await controller.Login(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Welcome back", ok.Value!.ToString()); 
    }

    //register tests

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenNameOrEmailMissing()
    {
        // Arrange
        var controller = new AuthController(
            CreateUserManagerMock().Object,
            CreateSignInManagerMock(CreateUserManagerMock().Object).Object
        );

        var dto = new RegisterDto
        {
            Name = "",
            Email = "",
            Password = "Password123!"
        };

        // Act
        var result = await controller.Register(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var fakeUser = new User { Email = "exists@example.com" };

        var userManagerMock = CreateUserManagerMock();
        var signInMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock
            .Setup(x => x.FindByEmailAsync(fakeUser.Email))
            .ReturnsAsync(fakeUser);

        var controller = new AuthController(userManagerMock.Object, signInMock.Object);

        var dto = new RegisterDto
        {
            Name = "New User",
            Email = fakeUser.Email,
            Password = "Password123!"
        };

        // Act
        var result = await controller.Register(dto);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenUserCreatedSuccesfully()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var signInMock = CreateSignInManagerMock(userManagerMock.Object);

        userManagerMock
            .Setup(x => x.FindByEmailAsync("new@example.com"))
            .ReturnsAsync((User?)null);

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "buyer"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AuthController(userManagerMock.Object, signInMock.Object);

        var dto = new RegisterDto
        {
            Name = "New User",
            Email = "new@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await controller.Register(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("registered successfully", ok.Value!.ToString());
    }
}