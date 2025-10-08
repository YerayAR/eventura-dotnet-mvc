using System.Security.Cryptography;
using Eventura.Application.Abstractions;

namespace Eventura.Infrastructure.Security;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Proveer hashing seguro de contraseñas usando PBKDF2.
/// Responsabilidades: Generar hashes con sal aleatoria y verificar contraseñas.
/// Dependencias/Puertos utilizados: APIs criptográficas de .NET a través de RandomNumberGenerator y Rfc2898DeriveBytes.
/// Límites (lo que NO debe hacer): Almacenar contraseñas en texto plano o relajar parámetros de seguridad sin revisión.
/// Errores comunes: Reducir el número de iteraciones o reutilizar la misma sal en múltiples usuarios.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 600000;
    private const char Delimiter = ':';

    public string Hash(string password)
    {
        // Contexto: Invocado por AuthService durante el registro o cambio de contraseña.
        // Intención: Generar hash resistente a fuerza bruta con sal única.
        // Pasos: 1) Validar entrada; 2) Generar sal; 3) Derivar clave PBKDF2; 4) Serializar sal y hash.
        // Validaciones: Arroja ArgumentException si la contraseña es nula o vacía.
        // Manejo de errores: Excepciones criptográficas se propagan; deben tratarse en capas superiores si se requiere.
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, KeySize);
        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(key));
    }

    public bool Verify(string hash, string password)
    {
        // Contexto: Comprobación de credenciales durante inicio de sesión.
        // Intención: Validar que la contraseña proporcionada produce el mismo hash.
        // Pasos: 1) Validar insumos; 2) Separar sal y hash almacenado; 3) Recalcular hash con parámetros originales; 4) Comparar en tiempo constante.
        // Validaciones: Devuelve false si el formato no coincide o datos de entrada son inválidos.
        // Manejo de errores: Excepciones de conversión Base64 se propagan; la capa de aplicación captura y responde con OperationResult.
        if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var parts = hash.Split(Delimiter);
        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var key = Convert.FromBase64String(parts[1]);
        var attempt = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, KeySize);
        return CryptographicOperations.FixedTimeEquals(key, attempt);
    }
}
