using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterObject
{
    /// <summary>
    /// Clase que representa un filtro de kendo
    /// </summary>
    public class FilterObject
    {
        private string _name;
        private object _value;
        private string _operator;

        /// <summary> Nombre de la propiedad por la que se desea filtrar </summary>
        public string Name 
        {
            get { return this._name; }
            set { this._name = value; } 
        }

        /// <summary> Objeto con el que debe de coincidir la propiedad a filtrar </summary>
        public object Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        /// <summary> Operador para la expresion con la que se filtrara </summary>
        public string Operator
        {
            get { return this._operator; }
            set { this._operator = value; }
        }
    }
}
