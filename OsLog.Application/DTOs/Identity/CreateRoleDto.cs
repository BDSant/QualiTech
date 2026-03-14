using System.ComponentModel.DataAnnotations;

namespace OsLog.Application.DTOs.Identity;

public sealed class CreateRoleDto
{
    [Required(ErrorMessage = "O nome da role é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome da role pode ter no máximo {1} caracteres.")]
    public string RoleName { get; set; } = string.Empty;
}

public sealed class AddUserRoleDto
{
    [Required(ErrorMessage = "O UserId é obrigatório.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome da role é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome da role pode ter no máximo {1} caracteres.")]
    public string RoleName { get; set; } = string.Empty;
}

public sealed class ReplaceUserRolesDto
{
    [Required(ErrorMessage = "O UserId é obrigatório.")]
    public string UserId { get; set; } = string.Empty;

    public string[] Roles { get; set; } = Array.Empty<string>();
}

public sealed class UserClaimInputDto
{
    [Required(ErrorMessage = "O tipo da claim é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O tipo da claim pode ter no máximo {1} caracteres.")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor da claim é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O valor da claim pode ter no máximo {1} caracteres.")]
    public string Value { get; set; } = string.Empty;
}

public sealed class AddUserClaimDto
{
    [Required(ErrorMessage = "O UserId é obrigatório.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo da claim é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O tipo da claim pode ter no máximo {1} caracteres.")]
    public string ClaimType { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor da claim é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O valor da claim pode ter no máximo {1} caracteres.")]
    public string ClaimValue { get; set; } = string.Empty;
}

public sealed class RemoveUserClaimDto
{
    [Required(ErrorMessage = "O UserId é obrigatório.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo da claim é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O tipo da claim pode ter no máximo {1} caracteres.")]
    public string ClaimType { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor da claim é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O valor da claim pode ter no máximo {1} caracteres.")]
    public string ClaimValue { get; set; } = string.Empty;
}

public sealed class ReplaceUserClaimsDto
{
    [Required(ErrorMessage = "O UserId é obrigatório.")]
    public string UserId { get; set; } = string.Empty;

    public UserClaimInputDto[] Claims { get; set; } = Array.Empty<UserClaimInputDto>();
}