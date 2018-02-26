using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpers {
    public static class HelperExtensions {
        /// <summary>
        /// Extension de IQueryable para filtrar el mismo dependiendo una serie de filtros
        /// </summary>
        /// <typeparam name="T">Tipo de los objetos que contiene el Iqueryable</typeparam>
        /// <param name="source">IQueryable que se desea filtrar</param>
        /// <param name="filtros">Listado de objetos de filtro</param>
        /// <param name="sOperator">Operador que se desea aplicar (AND, OR) al listado</param>
        /// <returns></returns>
        public static IQueryable<T> Filter<T>(this IQueryable<T> source, List<FilterObject> filtros, string sOperator) {
            #region method variables
            
            //obtenemos el tipo de los objetos que contiene el queryable
            Type type = typeof(T);            
            //Creamos una expresion con el nombre y el tipo de la propiedad            
            ParameterExpression parameterExp = Expression.Parameter(type, @"t");
            //Creamos una expresion default que siempre nos devuelva true por si
            //llegaramos a no crear una expresion en el ciclo
            Expression defaultExp = Expression.Equal(Expression.Constant(1, typeof(int)), Expression.Constant(1, typeof(int)));
            Expression<Func<T, bool>> lambdaFinal = Expression.Lambda<Func<T, bool>>(defaultExp, new[] { parameterExp });
            Expression<Func<T, bool>> lambdaItera = Expression.Lambda<Func<T, bool>>(defaultExp, new[] { parameterExp });
            //variables necesarias para las iteraciones
            PropertyInfo pName;            
            Type propType;            
            Expression propertyExp;            
            Expression valueExp;
            Expression e1;
            Type u;
            MethodInfo methodInfo;
            MethodCallExpression methodExp;

            #endregion
            
            //contador para llevar registro de las iteraciones
            int count = 0;            
            foreach (KendoFilter filtro in filtros) {
                //Si el tipo que se paso no contiene una expresion con el mismo nombre
                //del filtro saltamos esta iteracion
                if (!type.GetProperties().Where(xt => xt.Name == filtro.Name).Any()) {
                    continue;
                }
                
                //asignamos de nuevo la expresion por default
                lambdaItera = Expression.Lambda<Func<T, bool>>(defaultExp, new[] { parameterExp });
                //Obtenemos el tipo de la propiedad que se esta buscando
                pName = type.GetProperties().Where(xt => xt.Name == filtro.Name).FirstOrDefault();
                propType = pName.PropertyType;
                //Si el valor es una cadena nos aseguramos de que este en minusculas                
                
                //Creamos una expresion con el nombre y el tipo de la propiedad
                propertyExp = Expression.Property(parameterExp, filtro.Name);
                if (propType == typeof(string)) {
                    filtro.Value = Convert.ToString(filtro.Value).ToLower();
                    propertyExp = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                }
                
                //Creamos la constante con que se comparara el valor del objeto del queryable
                //con el nombre de la propiedad y el tipo de la misma
                valueExp = Expression.Constant(filtro.Value, propType);
                e1 = Expression.Equal(Expression.Constant(1, typeof(int)), Expression.Constant(1, typeof(int)));
                
                //Si el tipo de la propiedad es nulleable, debemos convertir la expresion del valor
                //al mismo tipo nulleable de la propiedad
                u = Nullable.GetUnderlyingType(propType);
                if (u != null && u.IsValueType) {
                    valueExp = Expression.Convert(valueExp, propType);
                }
                //en este case se forma la expresion de acuerdo al operador que se haya definino
                //para cada filtro, esto de acuerdo tambien al tipo de dato del filtro.
                
                #region Case de Operadores
                
                switch (filtro.Operator.ToLower())
                {
                    case "contains":
                        methodInfo = typeof(string).GetMethod(@"Contains", new[] { typeof(string) });
                        methodExp = Expression.Call(propertyExp, methodInfo, valueExp);
                        lambdaItera = Expression.Lambda<Func<T, bool>>(methodExp, parameterExp);
                        break;
                    case "doesnotcontain":
                        methodInfo = typeof(string).GetMethod(@"Contains", new[] { typeof(string) });
                        methodExp = Expression.Call(propertyExp, methodInfo, valueExp);
                        var dontContExp = Expression.Not(methodExp);
                        lambdaItera = Expression.Lambda<Func<T, bool>>(dontContExp, parameterExp);
                        break;
                    case "startswith":
                        methodInfo = typeof(string).GetMethod(@"StartsWith", new[] { typeof(string) });
                        methodExp = Expression.Call(propertyExp, methodInfo, valueExp);
                        lambdaItera = Expression.Lambda<Func<T, bool>>(methodExp, parameterExp);
                        break;
                    case "endswith":
                        methodInfo = typeof(string).GetMethod(@"EndsWith", new[] { typeof(string) });
                        methodExp = Expression.Call(propertyExp, methodInfo, valueExp);
                        lambdaItera = Expression.Lambda<Func<T, bool>>(methodExp, parameterExp);
                        break;
                    case "==":
                    case "isequalto":
                        e1 = Expression.Equal(propertyExp, valueExp);                        
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                    case "!=":
                    case "isnotequalto":
                        e1 = Expression.NotEqual(propertyExp, valueExp);                        
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                    case ">=":
                    case "isgreaterthanorequalto":
                        e1 = Expression.GreaterThanOrEqual(propertyExp, valueExp);                        
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                    case ">":
                    case "isgreaterthan":
                        e1 = Expression.GreaterThan(propertyExp, valueExp);                        
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                    case "<=":
                    case "islessthanorequalto":
                        e1 = Expression.LessThanOrEqual(propertyExp, valueExp);                        
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                    case "<":
                    case "islessthan":
                        e1 = Expression.LessThan(propertyExp, valueExp);
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                    case "isnull":
                        e1 = Expression.Equal(propertyExp, Expression.Constant(null, propType));
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                    case "isnotnull":
                        e1 = Expression.NotEqual(propertyExp, Expression.Constant(null, propType));
                        lambdaItera = Expression.Lambda<Func<T, bool>>(e1, parameterExp);
                        break;
                }
                
                #endregion
                
                #region operacion entre expresiones
                
                //Si es la primera iteracion, asignamos la expression creada a la expresion final
                if (count <= 0) {
                    lambdaFinal = lambdaItera;
                }else {
                    switch (sOperator.ToLower())
                    {
                        case "or":
                            lambdaFinal = Expression.Lambda<Func<T, bool>>(Expression.OrElse(lambdaFinal.Body, lambdaItera.Body), new[] { parameterExp });
                            break;
                        case "and":
                            lambdaFinal = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(lambdaFinal.Body, lambdaItera.Body), new[] { parameterExp });
                            break;
                        default:
                            lambdaFinal = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(lambdaFinal.Body, lambdaItera.Body), new[] { parameterExp });
                            break;
                    }
                }
                
                #endregion

                count++;                
            }
            return source.Where(lambdaFinal);
        }
    }
}