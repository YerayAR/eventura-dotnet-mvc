using System.ComponentModel.DataAnnotations;
using Eventura.Domain.Enums;

namespace Eventura.Web.Models;

#region Aprendizaje
// Los ViewModels viven en la capa Web para separar presentacion de Application.
// Se alimentan de DTOs previamente mapeados, permitiendo sanitizacion y DataAnnotations especificas de UI.
// TODO(aprendizaje): agregar propiedad para ordenacion seleccionada por el usuario y sincronizar con controlador.
#endregion

/// <summary>
/// Capa: Web.
/// Proposito: Representar datos resumidos de un evento para listados.
/// Responsabilidades: Exponer informacion lista para renderizar sin logica adicional.
/// Dependencias/Puertos utilizados: Usa EventCategory como catalogo compartido.
/// Limites (lo que NO debe hacer): Contener logica de negocio o referencias a servicios.
/// Errores comunes: Mutar propiedades en la vista; deben permanecer inmutables.
/// </summary>
public sealed class EventListItemViewModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset StartDateTime { get; init; }
    public TimeSpan Duration { get; init; }
    public string City { get; init; } = string.Empty;
    public string AddressLine { get; init; } = string.Empty;
    public int RemainingCapacity { get; init; }
    public EventCategory Category { get; init; }
    public bool IsCancelled { get; init; }
}

/// <summary>
/// Capa: Web.
/// Proposito: Capturar datos del formulario de creacion/edicion de eventos.
/// Responsabilidades: Definir DataAnnotations especificas para la vista MVC.
/// Dependencias/Puertos utilizados: Ninguna; recibe datos desde DTOs.
/// Limites (lo que NO debe hacer): Implementar validaciones complejas; se delegan a validators de Application.
/// Errores comunes: Compartir esta clase fuera de la capa Web o exponer entidades directamente.
/// </summary>
public sealed class EventFormViewModel
{
    public Guid Id { get; init; }

    [Required, StringLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset StartDateTime { get; init; } = DateTimeOffset.UtcNow.AddDays(1);

    [Required]
    [Range(15, 1440)]
    public int DurationMinutes { get; init; } = 60;

    [Required]
    public string City { get; init; } = string.Empty;

    [Required]
    public string AddressLine { get; init; } = string.Empty;

    [Range(1, 10000)]
    public int Capacity { get; init; } = 10;

    [Required]
    public EventCategory Category { get; init; } = EventCategory.Other;
}

/// <summary>
/// Capa: Web.
/// Proposito: Encapsular filtros y resultados de recomendaciones.
/// Responsabilidades: Combinar filtros de busqueda con la lista de eventos para la vista.
/// Dependencias/Puertos utilizados: Se nutre de EventListItemViewModel.
/// Limites (lo que NO debe hacer): Ejecutar consultas; solo se usa para presentacion.
/// Errores comunes: No marcar City como Required, rompiendo la experiencia de filtrado.
/// </summary>
public sealed class RecommendationViewModel
{
    [Required]
    public string City { get; init; } = string.Empty;
    public EventCategory? Category { get; init; }
    public IReadOnlyCollection<EventListItemViewModel> Events { get; init; } = Array.Empty<EventListItemViewModel>();
}
