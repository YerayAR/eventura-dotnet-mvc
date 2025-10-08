namespace Eventura.Domain.ValueObjects;

/// <summary>
/// Capa: Domain.
/// Propósito: Clase base para value objects con igualdad estructural.
/// Responsabilidades: Establecer contrato para comparar componentes y calcular hash.
/// Dependencias/Puertos utilizados: Ninguno; funciona únicamente con .NET base.
/// Límites (lo que NO debe hacer): No debe almacenar estado mutable ni lógica dependiente de infraestructura.
/// Errores comunes: Olvidar incluir todos los componentes relevantes en GetEqualityComponents.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        // Contexto: Comparación necesaria para escenarios donde se usan value objects como claves.
        // Intención: Verificar igualdad por valor y no por referencia.
        // Pasos: 1) Verificar tipo; 2) Comparar secuencias de componentes.
        // Validaciones: Comprueba que el objeto comparado implemente ValueObject.
        // Manejo de errores: No lanza; retorna false cuando no coincide.
        if (obj is not ValueObject other)
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        // Contexto: Necesario para usar value objects en estructuras de hash.
        // Intención: Combinar componentes para un hash consistente.
        // Pasos: 1) Reducir componentes mediante Aggregate y HashCode.Combine.
        // Validaciones: Depende de la implementación correcta en clases derivadas.
        // Manejo de errores: No lanza; comportamiento determinista.
        return GetEqualityComponents()
            .Aggregate(0, (current, obj) => HashCode.Combine(current, obj));
    }
}
