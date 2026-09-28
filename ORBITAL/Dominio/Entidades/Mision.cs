using ORBITAL.Dominio.Enumeraciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class Mision
    {
        // Atributos
        private int id;
        private string codigo;
        private string nombre;
        private string descripcion;
        private DateTime fechaInicio;
        private DateTime fechaFinEstimada;
        private PrioridadMision prioridad;
        private EstadoMision estado;
        private Usuario responsable;
        private List<RecursoExploracion> recursos;

        // Getters y Setters
        public int Id { get => id; set => id = value; }
        public string Codigo { get => codigo; set => codigo = value; }
        public string Nombre { get => nombre; set => nombre = value; }
        public string Descripcion { get => descripcion; set => descripcion = value; }
        public DateTime FechaInicio { get => fechaInicio; set => fechaInicio = value; }
        public DateTime FechaFinEstimada { get => fechaFinEstimada; set => fechaFinEstimada = value; }
        public PrioridadMision Prioridad { get => prioridad; set => prioridad = value; }
        public EstadoMision Estado { get => estado; set => estado = value; }
        public Usuario Responsable { get => responsable; set => responsable = value; }
        public List<RecursoExploracion> Recursos { get => recursos; set => recursos = value; }

        // Constructor público
        public Mision(string codigo, string nombre, Usuario responsable)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.responsable = responsable;
            this.estado = EstadoMision.Planificada; // Estado inicial por defecto
        }
        public Mision(string codigo, string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFin, PrioridadMision prioridad, Usuario responsable)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.descripcion = descripcion;
            this.fechaInicio = fechaInicio;
            this.fechaFinEstimada = fechaFin;
            this.prioridad = prioridad;
            this.responsable = responsable;
            this.estado = EstadoMision.Planificada;
        }

        // Métodos públicos
        public void AsignarRecurso(RecursoExploracion recurso)
        {
            if (recurso != null && !recursos.Contains(recurso))
            {
                recursos.Add(recurso);
            }
        }

        public void RetirarRecurso(RecursoExploracion recurso)
        {
            if (recurso != null && recursos.Contains(recurso))
            {
                recursos.Remove(recurso);
            }
        }

        public void Iniciar()
        {
            this.estado = EstadoMision.EnEjecucion;
        }

        public void Finalizar()
        {
            this.estado = EstadoMision.Finalizada;
        }

        public void Cancelar()
        {
            this.estado = EstadoMision.Cancelada;
        }

        public bool ValidarProtocoloSeguridad()
        {
            // Falta la lógica para validar que se cumplan las condiciones de seguridad
            return true;
        }

        public decimal CalcularCostoEstimado()
        {
            decimal costoTotal = 0;
            // Falta la lógica para calcular el costo en base a los recursos asignados
            return costoTotal;
        }

        public override string ToString()
        {
            return "Misión: " + nombre + " [" + codigo + "] - Estado: " + estado + " - Responsable: " + (responsable != null ? responsable.NombreUsuario : "");
        }

    }
}
