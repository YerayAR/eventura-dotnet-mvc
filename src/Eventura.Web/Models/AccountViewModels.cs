using System.ComponentModel.DataAnnotations;
using Eventura.Application.DTOs;

namespace Eventura.Web.Models;

/// <summary>
/// Capa: Web.
/// Proposito: Recoger datos del formulario de registro.
/// Responsabilidades: Definir validaciones UI (DataAnnotations) y transportar datos al controlador.
/// Dependencias/Puertos utilizados: Usa Roles definidos en Application.
/// Limites (lo que NO debe hacer): Incluir logica de negocio o exponer contrasenias en texto plano fuera de la vista.
/// Errores comunes: No marcar Email con EmailAddress provocando errores tardios.
/// </summary>
public sealed class RegisterViewModel
{
    [Required, StringLength(100)]
    public string UserName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string Role { get; init; } = Roles.User;
}

/// <summary>
/// Capa: Web.
/// Proposito: Modelar datos de inicio de sesion.
/// Responsabilidades: Trasladar credenciales y preferencia RememberMe al controlador.
/// Dependencias/Puertos utilizados: Ninguna adicional.
/// Limites (lo que NO debe hacer): Guardar contrasenias mas alla del ciclo de peticion.
/// Errores comunes: No marcar Password con DataType Password en Razor.
/// </summary>
public sealed class LoginViewModel
{
    [Required]
    public string UserNameOrEmail { get; init; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}
