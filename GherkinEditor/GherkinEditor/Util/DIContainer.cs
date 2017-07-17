using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    /// <summary>
    /// Pure DI. It resoves all dependencies after registering interface implementation.
    /// Concrete classes are automatically registered if not yet registered.
    /// </summary>
    public class DIContainer
    {
        /// <summary>
        /// Mapping contract type to implementation type
        /// </summary>
        private readonly Dictionary<Type, Type> m_TypeMapping = new Dictionary<Type, Type>();
        /// <summary>
        /// Singleton implementation of contract type
        /// </summary>
        private readonly Dictionary<Type, object> m_TypeImpl = new Dictionary<Type, object>();

        public void Register<TContract, TImplementation>()
        {
            m_TypeMapping[typeof(TContract)] = typeof(TImplementation);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type contract)
        {
            if (m_TypeImpl.ContainsKey(contract))
            {
                return m_TypeImpl[contract];
            }

            if (!m_TypeMapping.ContainsKey(contract))
            {
                if (contract.IsInterface)
                    throw new ApplicationException(contract.Name + " must be registered before resoving it.");
                else
                    m_TypeMapping[contract] = contract;
            }

            return CreateObjectOf(contract);
        }

        private object CreateObjectOf(Type contract)
        {
            Type implType = m_TypeMapping[contract];
            ConstructorInfo[] constructors = implType.GetConstructors();
            if (constructors.Length == 0)
                throw new ApplicationException(implType.Name + " has no public constructors.");

            ConstructorInfo constructor = implType.GetConstructors()[0];    // create object by using 1st constructor

            ParameterInfo[] constructorParameters = constructor.GetParameters();
            List<object> parameters = new List<object>(constructorParameters.Length);
            foreach (ParameterInfo parameterInfo in constructorParameters)
            {
                if (Util.IsSimpleType(parameterInfo.ParameterType))
                {
                    // set default value for simple types, for string type null will be set
                    parameters.Add(null);
                }
                else
                {
                    // Recursively resolve parameters
                    parameters.Add(Resolve(parameterInfo.ParameterType));
                }
            }

            object impl = constructor.Invoke(parameters.ToArray());
            m_TypeImpl[contract] = impl;
            return impl;
        }
    }
}
