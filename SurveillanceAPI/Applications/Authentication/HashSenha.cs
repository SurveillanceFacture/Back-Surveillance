using Microsoft.AspNetCore.Identity;
using SurveillanceAPI.Inteface;

namespace SurveillanceAPI.Applications.Authentication;

public class HashSenha: IHashSenha
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Contexto = new();
    private static readonly string HashFalso = Hasher.HashPassword(Contexto, Guid.NewGuid().ToString("N"));

    public string Hash(string senha) => Hasher.HashPassword(Contexto, senha);

    public bool Validar(string hash, string senha) =>
        Hasher.VerifyHashedPassword(Contexto, hash, senha) != PasswordVerificationResult.Failed;

    public void SimularValidacao(string senha) =>
        Hasher.VerifyHashedPassword(Contexto, HashFalso, senha);    
}