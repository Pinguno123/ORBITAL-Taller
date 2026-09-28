using ORBITAL.Datos.Repositorios;
using ORBITAL.Dominio.Entidades;
using ORBITAL.Dominio.Enumeraciones;
using System;

namespace ORBITAL.App
{
    public class SesionUsuario
    {
        private Usuario usuarioActual;
        private readonly UsuarioRepositorio usuarioRepositorio;

        public static SesionUsuario Instancia { get; } = new SesionUsuario();

        public SesionUsuario()
        {
            this.usuarioRepositorio = new UsuarioRepositorio();
        }

        public SesionUsuario(UsuarioRepositorio repo)
        {
            this.usuarioRepositorio = repo ?? new UsuarioRepositorio();
        }

        public bool IniciarSesion(string usuario, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contraseña))
                return false;

            Usuario user = usuarioRepositorio.ValidarCredenciales(usuario, contraseña);
            if (user != null && user.EstaActivo())
            {
                this.usuarioActual = user;
                return true;
            }

            return false;
        }

        public void CerrarSesion()
        {
            if (this.usuarioActual != null)
            {
                this.usuarioActual.CerrarSesion();
                this.usuarioActual = null;
            }
        }

        public Usuario ObtenerUsuarioActual()
        {
            return this.usuarioActual;
        }

        public bool TienePermiso(RolUsuario rolRequerido)
        {
            if (this.usuarioActual == null || !this.usuarioActual.EstaActivo())
                return false;

            // El Administrador tiene acceso total a todos los módulos
            if (this.usuarioActual.Rol == RolUsuario.Administrador)
                return true;

            // Comprobación por rol específico
            return this.usuarioActual.Rol == rolRequerido;
        }
    }
}
