using ORBITAL.Dominio.Enumeraciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class Usuario
    {
        // Atributos
        private int id;
        private string nombreUsuario;
        private string contraseña;
        private RolUsuario rol;
        private EstadoUsuario estado;
        private List<Mision> misiones = new List<Mision>();

        // Getters y Setters
        public int Id { get => id; set => id = value; }
        public string NombreUsuario { get => nombreUsuario; set => nombreUsuario = value; }
        public string Contraseña { get => contraseña; set => contraseña = value; }
        public RolUsuario Rol { get => rol; set => rol = value; }
        public EstadoUsuario Estado { get => estado; set => estado = value; }
        public List<Mision> Misiones { get => misiones; }

        // Constructor público
        public Usuario(string nombreUsuario, string contraseña, RolUsuario rol)
        {
            this.nombreUsuario = nombreUsuario;
            this.contraseña = contraseña;
            this.rol = rol;
            this.estado = EstadoUsuario.Activo; // Activo por defecto
        }

        public Usuario(int id, string nombreUsuario, string contraseña, RolUsuario rol, EstadoUsuario estado)
        {
            this.id = id;
            this.nombreUsuario = nombreUsuario;
            this.contraseña = contraseña;
            this.rol = rol;
            this.estado = estado;
        }

        // Métodos públicos
        public bool IniciarSesion(string contraseña)
        {
            if (!EstaActivo())
            {
                return false;
            }
            return string.Equals(this.contraseña, contraseña, StringComparison.Ordinal);
        }

        public void CerrarSesion()
        {
            // Lógica de finalización de contexto del usuario
        }

        public bool EstaActivo()
        {
            return this.estado == EstadoUsuario.Activo;
        }

        public void CambiarEstado(EstadoUsuario nuevoEstado)
        {
            this.estado = nuevoEstado;
        }

        public override string ToString()
        {
            return $"{nombreUsuario} | Rol: {rol} | Estado: {estado}";
        }
    }
}
