using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Domains
{
    public class Loja
    {
        public Guid LojaId { get; private set; }
        public string RazaoSocial { get; private set; } = null!;
        public string Cnpj { get; private set; } = null!;
        public DateTime DataCriacao { get; private set; }
        public bool StatusLoja { get; private set; } = true;
        
        public Guid AuditorUsuarioId { get; private set; }
        public Guid GerenteUsuarioId { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }

        public Usuario Auditor { get; private set; } = null!;
        public Usuario Gerente { get; private set; } = null!;
        public Usuario CriadoPorUsuario { get; private set; } = null!;
        
        private Loja() { }

        public Loja(
            string razaoSocial,
            string cnpj,
            Guid auditorUsuarioId,
            Guid criadoPorUsuarioId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(razaoSocial);
            ArgumentException.ThrowIfNullOrWhiteSpace(cnpj);

            if (razaoSocial.Trim().Length > 180)
                throw new DomainException("Razão social muito longa.");

            var cnpjLimpo = NormalizarCnpj(cnpj);
            
            if (!CnpjValido(cnpjLimpo))
                throw new DomainException("CNPJ inválido.");

            if (auditorUsuarioId == Guid.Empty)
                throw new DomainException("Auditor inválido.");

            if (criadoPorUsuarioId == Guid.Empty)
                throw new DomainException("Usuário criador inválido.");

            LojaId = Guid.NewGuid();
            RazaoSocial = razaoSocial.Trim();
            Cnpj = cnpjLimpo;
            AuditorUsuarioId = auditorUsuarioId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DataCriacao = DateTime.UtcNow;
        }

        public void TrocarAuditor(Guid novoAuditorUsuarioId)
        {
            if (novoAuditorUsuarioId == Guid.Empty)
                throw new DomainException("Auditor inválido.");

            AuditorUsuarioId = novoAuditorUsuarioId;
        }

        public void Desativar() => StatusLoja = false;
        public void Ativar() => StatusLoja = true;

        public bool CnpjCorresponde(string? outroCnpj) =>
            !string.IsNullOrWhiteSpace(outroCnpj) && Cnpj == NormalizarCnpj(outroCnpj);

        public static string NormalizarCnpj(string cnpj) =>
            new string(cnpj.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        private static bool CnpjValido(string cnpj)
        {
            if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
                return false;

            int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var digito1 = CalcularDigito(cnpj[..12], pesos1);
            var digito2 = CalcularDigito(cnpj[..12] + digito1, pesos2);

            return cnpj[12] - '0' == digito1 && cnpj[13] - '0' == digito2;
        }

        private static int CalcularDigito(string texto, int[] pesos)
        {
            var soma = 0;
            for (var i = 0; i < pesos.Length; i++)
                soma += (texto[i] - '0') * pesos[i];

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
    
        }
    }
}
