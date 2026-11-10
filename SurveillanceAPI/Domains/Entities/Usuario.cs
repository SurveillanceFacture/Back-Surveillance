using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Domains
{
    public class Usuario
    {
        public Guid UsuarioId { get; private set; }
        public string NomeUsuario { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string HashSenha { get; private set; } = null!;
        public DateTime DataCriacao { get; private set; }
        public DateTime? UltimoAcesso { get; private set; } = null!;
        public bool StatusUsuario { get; private set; } = true;

        public int TipoUsuarioId { get; private set; }
        public TipoUsuario TipoUsuario { get; private set; }    

        private Usuario() { }

        public Usuario(
            string nomeUsuario,
            string email,
            string hashSenha,
            int tipoUsuarioId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nomeUsuario);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(hashSenha);


            if (nomeUsuario.Trim().Length > 150)
            {
                throw new DomainException("Nome muito longo.");
            }
            if (email.Trim().Length > 254)
                throw new DomainException("Email muito longo.");

            if (tipoUsuarioId <= 0)
                throw new DomainException("Tipo de usuário inválido.");

            UsuarioId = Guid.NewGuid();
            NomeUsuario = nomeUsuario.Trim();
            Email = email.Trim().ToLowerInvariant();
            HashSenha = hashSenha;
            TipoUsuarioId = tipoUsuarioId;
            DataCriacao = DateTime.UtcNow;
        }
        public void AlterarEmail(string novoEmail)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(novoEmail);

            if(novoEmail.Trim().Length > 254)
            {
                throw new DomainException("Email muito longo.");
            }

            Email = novoEmail.Trim().ToLowerInvariant();
        }
        
        public void RegistrarAcesso() => UltimoAcesso = DateTime.UtcNow;
        
        public bool EstaOnline(TimeSpan janela) => 
            UltimoAcesso.HasValue && DateTime.UtcNow - UltimoAcesso.Value <= janela;
        
        public void Desativar() => StatusUsuario = false;
        public void Ativar() => StatusUsuario = true;
    }
}
