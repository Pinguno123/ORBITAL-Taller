using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class Dron : RecursoExploracion
    {
        // Atributos
        private decimal autonomiaVuelo;
        private decimal alcance;
        private decimal costoPorHora;

        // Getters y Setters
        public decimal AutonomiaVuelo { get => autonomiaVuelo; set => autonomiaVuelo = value; }
        public decimal Alcance { get => alcance; set => alcance = value; }
        public decimal CostoPorHora { get => costoPorHora; set => costoPorHora = value; }

        // Constructores
        public Dron(string codigo, string modelo) : base(codigo, modelo) { }

        public Dron(string codigo, string modelo, decimal autonomia, decimal alcance, decimal costoHora): base(codigo, modelo)
        {
            this.autonomiaVuelo = autonomia;
            this.alcance = alcance;
            this.costoPorHora = costoHora;
        }

        // Métodos públicos
        public override decimal CalcularCostoOperacion(decimal cantidad)
        {
            return cantidad * this.costoPorHora;
        }
    }
}
