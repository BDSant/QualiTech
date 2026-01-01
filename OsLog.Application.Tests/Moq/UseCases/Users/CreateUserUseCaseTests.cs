using Moq;
using OsLog.Application.DTOs.Users;
using OsLog.Application.Ports.Identity.Admin;
using OsLog.Application.UseCases.Users;
using System.Security.Claims;

namespace OsLog.Application.Tests.Moq.UseCases.Users
{
    public sealed class CreateUserUseCaseTests
    {
        private readonly Mock<IIdentityAdminGateway> _gateway = new(MockBehavior.Strict);

        private CreateUserUseCase CreateSut() => new(_gateway.Object);

        private static CreateUserRequest CreateValidRequest(
            string? email = null,
            string? userName = null,
            string? password = null,
            string? confirmPassword = null,
            IReadOnlyCollection<string>? roles = null,
            IReadOnlyCollection<ClaimDto>? claims = null)
        {
            var _email = email ?? $"user{Guid.NewGuid():N}@oslog.local";
            var _user = userName ?? $"user{Guid.NewGuid():N}";
            var _password = password ?? "User@1234";
            var _confirmPassWord = confirmPassword ?? _password;

            return new CreateUserRequest
            {
                UserName = _user,
                Email = _email,
                Password = _password,
                ConfirmPassword = _confirmPassWord,
                EmailConfirmed = true,
                Roles = roles ?? Array.Empty<string>(),
                Claims = claims ?? Array.Empty<ClaimDto>()
            };
        }

        private static bool RolesAreExactly(IEnumerable<string> roles, params string[] expected)
        {
            if (roles is null) return false;

            var set = new HashSet<string>(roles.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

            return set.SetEquals(expected);
        }

        private static bool ClaimsContainExactly(IEnumerable<Claim> claims,
            params (string type, string value)[] expected)
        {
            if (claims is null) return false;

            var list = claims.ToList();
            if (list.Count != expected.Length) return false;

            foreach (var (type, value) in expected)
            {
                if (!list.Any(c => string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase)
                                   && string.Equals(c.Value, value, StringComparison.Ordinal)))
                    return false;
            }

            return true;
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Request nulo - Deve retornar Validation")]
        public async Task ExecuteAsync_WhenRequestIsNull_ShouldReturnValidation()
        {
            var sut = CreateSut();

            var result = await sut.ExecuteAsync(null!, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.Validation, result.ErrorCode);
            Assert.NotEmpty(result.Errors);
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Campos obrigatórios ausentes - Deve retornar Validation")]
        public async Task ExecuteAsync_WhenMissingRequiredFields_ShouldReturnValidation()
        {
            var sut = CreateSut();

            var req = new CreateUserRequest
            {
                UserName = " ",
                Email = "",
                Password = " ",
                ConfirmPassword = " ",
                Roles = Array.Empty<string>(),
                Claims = Array.Empty<ClaimDto>()
            };

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.Validation, result.ErrorCode);
            Assert.Contains(result.Errors, e => e.Contains("UserName", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(result.Errors, e => e.Contains("Email", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(result.Errors, e => e.Contains("Password", StringComparison.OrdinalIgnoreCase));
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - ConfirmPassword divergente - Deve retornar Validation")]
        public async Task ExecuteAsync_WhenConfirmPasswordMismatch_ShouldReturnValidation()
        {
            var sut = CreateSut();

            var req = CreateValidRequest(password: "User@1234", confirmPassword: "Different@1234");

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.Validation, result.ErrorCode);
            Assert.Contains(result.Errors, e => e.Contains("ConfirmPassword", StringComparison.OrdinalIgnoreCase));
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Roles com valor inválido - Deve retornar Validation")]
        public async Task ExecuteAsync_WhenRolesContainInvalidEntry_ShouldReturnValidation()
        {
            var sut = CreateSut();

            var req = CreateValidRequest(roles: new[] { "Admin", "   " });

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.Validation, result.ErrorCode);
            Assert.Contains(result.Errors, e => e.Contains("Roles", StringComparison.OrdinalIgnoreCase));
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Claims com valor inválido - Deve retornar Validation")]
        public async Task ExecuteAsync_WhenClaimsContainInvalidEntry_ShouldReturnValidation()
        {
            var sut = CreateSut();

            var req = CreateValidRequest(claims: new[] { new ClaimDto { Type = "", Value = "1" } });

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.Validation, result.ErrorCode);
            Assert.Contains(result.Errors, e => e.Contains("Claims", StringComparison.OrdinalIgnoreCase));
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Email já existente - Deve retornar Conflict e não criar usuário")]
        public async Task ExecuteAsync_WhenEmailAlreadyExists_ShouldReturnConflict_AndNotCreateUser()
        {
            var req = CreateValidRequest(email: "exists@oslog.local");

            _gateway
                .Setup(x => x.GetUserByEmailAsync(req.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdentityUserData("1", "existing", req.Email, true));

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.Conflict, result.ErrorCode);

            _gateway.Verify(x => x.CreateUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _gateway.VerifyAll();
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Falha ao criar usuário no Identity - Deve retornar IdentityError")]
        public async Task ExecuteAsync_WhenCreateUserFails_ShouldReturnIdentityError()
        {
            var req = CreateValidRequest();

            _gateway
                .Setup(x => x.GetUserByEmailAsync(req.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IdentityUserData?)null);

            _gateway
                .Setup(x => x.CreateUserAsync(
                    req.UserName.Trim(),
                    req.Email.Trim(),
                    req.Password,
                    req.EmailConfirmed,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult<IdentityUserData>.Failure("Falha no Identity"));

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.IdentityError, result.ErrorCode);
            Assert.Contains("Falha no Identity", result.Errors);

            _gateway.VerifyAll();
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Falha ao garantir role - Deve retornar IdentityError e não adicionar roles/claims")]
        public async Task ExecuteAsync_WhenEnsureRoleFails_ShouldReturnIdentityError_AndNotAddRolesOrClaims()
        {
            var req = CreateValidRequest(roles: new[] { "Admin" });

            _gateway
                .Setup(x => x.GetUserByEmailAsync(req.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IdentityUserData?)null);

            var created = new IdentityUserData("10", req.UserName.Trim(), req.Email.Trim(), req.EmailConfirmed);

            _gateway
                .Setup(x => x.CreateUserAsync(
                    req.UserName.Trim(),
                    req.Email.Trim(),
                    req.Password,
                    req.EmailConfirmed,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult<IdentityUserData>.Success(created));

            _gateway
                .Setup(x => x.EnsureRoleExistsAsync("Admin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult.Failure("Erro ao garantir role"));

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.IdentityError, result.ErrorCode);

            _gateway.Verify(x => x.AddUserToRolesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
            _gateway.Verify(x => x.AddClaimsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Claim>>(), It.IsAny<CancellationToken>()), Times.Never);

            _gateway.VerifyAll();
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Falha ao adicionar roles - Deve retornar IdentityError")]
        public async Task ExecuteAsync_WhenAddRolesFails_ShouldReturnIdentityError()
        {
            var req = CreateValidRequest(roles: new[] { "Admin" });

            _gateway
                .Setup(x => x.GetUserByEmailAsync(req.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IdentityUserData?)null);

            var created = new IdentityUserData("10", req.UserName.Trim(), req.Email.Trim(), req.EmailConfirmed);

            _gateway
                .Setup(x => x.CreateUserAsync(
                    req.UserName.Trim(),
                    req.Email.Trim(),
                    req.Password,
                    req.EmailConfirmed,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult<IdentityUserData>.Success(created));

            _gateway
                .Setup(x => x.EnsureRoleExistsAsync("Admin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult.Success());

            _gateway
                .Setup(x => x.AddUserToRolesAsync(
                    created.Id,
                    It.Is<IEnumerable<string>>(roles => RolesAreExactly(roles, "Admin")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult.Failure("Erro ao adicionar role"));

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.IdentityError, result.ErrorCode);
            Assert.Contains("Erro ao adicionar role", result.Errors);

            _gateway.Verify(x => x.AddClaimsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Claim>>(), It.IsAny<CancellationToken>()), Times.Never);
            _gateway.VerifyAll();
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Falha ao adicionar claims - Deve retornar IdentityError")]
        public async Task ExecuteAsync_WhenAddClaimsFails_ShouldReturnIdentityError()
        {
            var req = CreateValidRequest(claims: new[]
            {
                new ClaimDto { Type = "empresa_id", Value = "1" }
            });

            _gateway
                .Setup(x => x.GetUserByEmailAsync(req.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IdentityUserData?)null);

            var created = new IdentityUserData("10", req.UserName.Trim(), req.Email.Trim(), req.EmailConfirmed);

            _gateway
                .Setup(x => x.CreateUserAsync(
                    req.UserName.Trim(),
                    req.Email.Trim(),
                    req.Password,
                    req.EmailConfirmed,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult<IdentityUserData>.Success(created));

            _gateway
                .Setup(x => x.AddClaimsAsync(
                    created.Id,
                    It.Is<IEnumerable<Claim>>(claims =>
                        claims != null
                        && claims.Count() == 1
                        && claims.Any(c => c.Type == "empresa_id" && c.Value == "1")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult.Failure("Erro ao adicionar claim"));

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.IdentityError, result.ErrorCode);
            Assert.Contains("Erro ao adicionar claim", result.Errors);

            _gateway.VerifyAll();
        }

        [Trait("Unit Usuario", "Registro Usuario")]
        [Fact(DisplayName = "RegistroUsuario - Dados válidos - Deve retornar OK e normalizar Roles/Claims")]
        public async Task ExecuteAsync_WhenEverythingSucceeds_ShouldReturnOk_WithNormalizedRolesAndClaims()
        {
            var req = CreateValidRequest(
                email: "  user@oslog.local  ",
                userName: "  benne  ",
                roles: new[] { "Admin", " admin ", "Master" },
                claims: new[]
                {
            new ClaimDto { Type = "empresa_id", Value = "1" },
            new ClaimDto { Type = "EMPRESA_ID", Value = "1" },
            new ClaimDto { Type = "unidade_id", Value = "2" }
                });

            _gateway
                .Setup(x => x.GetUserByEmailAsync(req.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IdentityUserData?)null);

            var created = new IdentityUserData("42", req.UserName.Trim(), req.Email.Trim(), req.EmailConfirmed);

            _gateway
                .Setup(x => x.CreateUserAsync(
                    req.UserName.Trim(),
                    req.Email.Trim(),
                    req.Password,
                    req.EmailConfirmed,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult<IdentityUserData>.Success(created));

            _gateway
                .Setup(x => x.EnsureRoleExistsAsync("Admin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult.Success());

            _gateway
                .Setup(x => x.EnsureRoleExistsAsync("Master", It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult.Success());

            _gateway
                .Setup(x => x.AddUserToRolesAsync(
                    created.Id,
                    It.Is<IEnumerable<string>>(roles => RolesAreExactly(roles, "Admin", "Master")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityOperationResult.Success());

            IEnumerable<Claim>? capturedClaims = null;

            _gateway
                .Setup(x => x.AddClaimsAsync(
                    created.Id,
                    It.IsAny<IEnumerable<Claim>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, IEnumerable<Claim>, CancellationToken>((_, c, __) => capturedClaims = c)
                .ReturnsAsync(IdentityOperationResult.Success());

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(req, CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.Equal(CreateUserErrorCode.None, result.ErrorCode);
            Assert.NotNull(result.Data);

            Assert.Equal("42", result.Data!.UserId);
            Assert.Equal("benne", result.Data.UserName);
            Assert.Equal("user@oslog.local", result.Data.Email);

            Assert.Equal(2, result.Data.Roles.Count);
            Assert.Contains("Admin", result.Data.Roles);
            Assert.Contains("Master", result.Data.Roles);

            Assert.Equal(2, result.Data.Claims.Count);
            Assert.Contains(result.Data.Claims, c => c.Type == "empresa_id" && c.Value == "1");
            Assert.Contains(result.Data.Claims, c => c.Type == "unidade_id" && c.Value == "2");

            // asserts do Callback (deduplicação)
            Assert.NotNull(capturedClaims);
            Assert.Equal(2, capturedClaims!.Count());
            Assert.Contains(capturedClaims!, c => c.Type == "empresa_id" && c.Value == "1");
            Assert.Contains(capturedClaims!, c => c.Type == "unidade_id" && c.Value == "2");

            _gateway.VerifyAll();
        }
    }
}
