using SurveillanceAPI.Applications.Authentication;
using SurveillanceAPI.Domains;
namespace SurveillanceAPI.Interface;

public interface IGeradorTokenJwt
{
    TokenGerado Gerar(Usuario usuario);
}