namespace SurveillanceAPI.Inteface;

public interface IHashSenha
{
    string Hash(string senha);
    bool Validar(string hash, string senha);
    void SimularValidacao(string senha);
}