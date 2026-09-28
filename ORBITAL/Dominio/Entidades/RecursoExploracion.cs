using ORBITAL.Dominio.Enumeraciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    internal abstract class RecursoExploracion
    {
        // Atributos
        protected int id;
        protected string codigo;
        protected string modelo;
        protected EstadoRecurso estado;

        // Getters y Setters
        public int Id { get => id; set => id = value; }
        public string Codigo { get => codigo; set => codigo = value; }
        public string Modelo { get => modelo; set => modelo = value; }
        public EstadoRecurso Estado { get => estado; set => estado = value; }

        // Constructores
        protected RecursoExploracion(string codigo, string modelo)
        {
            this.codigo = codigo;
            this.modelo = modelo;
            this.estado = EstadoRecurso.Disponible; // Estado predeterminado
        }
        protected RecursoExploracion(string codigo, string modelo, EstadoRecurso estado)
        {
            this.codigo = codigo;
            this.modelo = modelo;
            this.estado = estado;
        }

        // Métodos públicos
        public void Asignar(Mision mision)
        {
            // Falta la lógica básica para cambiar el estado al asignarse a una misión
            this.estado = EstadoRecurso.Asignado;
        }

        public void Liberar()
        {
            this.estado = EstadoRecurso.Disponible;
        }

        public bool EstaDisponible()
        {
            return this.estado == EstadoRecurso.Disponible;
        }

        public abstract decimal CalcularCostoOperacion(decimal cantidad);

        public void CambiarEstado(EstadoRecurso estado)
        {
            this.estado = estado;
        }

        public override string ToString()
        {
            return "Recurso: " + modelo + " [" + codigo + "] - Estado: " + estado;
        }
    }
}
