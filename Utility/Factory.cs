using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using Wintellect.PowerCollections;

namespace Utility
{
    public class Factory<TProduct>
    {
        #region Fields
        private readonly Dictionary<object, Pair<Assembly, Type>> registry; // Key to type mappings for product creation
        #endregion

        #region Singleton Pattern
        private static volatile Factory<TProduct> instance;
        private static readonly object syncRoot = new object();

        public static Factory<TProduct> Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Factory<TProduct>();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Constructor
        private Factory()
        {
            // Perform variable initializations
            registry = new Dictionary<object, Pair<Assembly, Type>>();

            // Initialize the factory
            Initialize();
        }
        #endregion

        #region Properties
        public Dictionary<object, Pair<Assembly, Type>>.KeyCollection Keys
        {
            get { return registry.Keys; }
        }
        #endregion

        #region Methods
        public bool ContainsKey(object key)
        {
            return registry.ContainsKey(key);
        }

        public TProduct CreateDefault()
        {
            Pair<Assembly, Type> assemblyType = DefaultProduct();
            return CreateProduct(assemblyType, null);
        }

        public TProduct CreateDefault(params object[] args)
        {
            Pair<Assembly, Type> assemblyType = DefaultProduct();
            return CreateProduct(assemblyType, args);
        }

        public TProduct Create(object key)
        {
            Debug.Assert(registry.Count != 0);
            Pair<Assembly, Type> assemblyType = registry[key];
            return CreateProduct(assemblyType, null);
        }

        public TProduct Create(object key, params object[] args)
        {
            Debug.Assert(registry.Count != 0);
            Pair<Assembly, Type> assemblyType = registry[key];
            return CreateProduct(assemblyType, args);
        }

        private static TProduct CreateProduct(Pair<Assembly, Type> assemblyType, object[] args)
        {
            Assembly assembly = assemblyType.First;
            Type type = assemblyType.Second;
            if (type != null)
            {
                object inst = assembly.CreateInstance(type.FullName, true,
                                                      BindingFlags.CreateInstance,
                                                      null, args, null, null);

                if (inst == null)
                {
                    throw new NullReferenceException("Null product instance.  " +
                         "Unable to create neccesary product class.");
                }

                TProduct prod = (TProduct) inst;

                return prod;
            }
            else
            {
                return default(TProduct);
            }
        }

        private Pair<Assembly, Type> DefaultProduct()
        {
            Debug.Assert(registry.Count != 0);
            Pair<Assembly, Type> assemblyType = default(Pair<Assembly, Type>);
            if (registry.Count > 0)
            {
                IEnumerator<Pair<Assembly, Type>> p = registry.Values.GetEnumerator();
                p.MoveNext();
                assemblyType = p.Current;
            }
            return assemblyType;
        }

        // Find and map available product classes in the current AppDomain
        private void Initialize()
        {
            // Get the assembly that contains this code
            //Assembly assembly = Assembly.GetCallingAssembly();
            //ExamineAssembly(assembly);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                ExamineAssembly(assembly);
            }
        }

        private void ExamineAssembly(Assembly assembly)
        {
            // Get a list of all the types in the assembly
            Type[] allTypes = assembly.GetTypes();
            foreach (Type type in allTypes)
            {
                // Only scan classes that arn't abstract
                if (type.IsClass && !type.IsAbstract)
                {
                    // If a class implements the IFactoryProduct interface,
                    // which allows retrieval of the product class key...
                    Type iFactoryProduct = type.GetInterface("IFactoryProduct");
                    if (iFactoryProduct != null)
                    {
                        // Create a temporary instance of that class...
                        object inst = assembly.CreateInstance(type.FullName, true,
                            BindingFlags.CreateInstance, null, null, null, null);

                        if (inst != null)
                        {
                            // And generate the product classes key
                            IFactoryProduct keyDesc = (IFactoryProduct) inst;
                            object key = keyDesc.GetFactoryKey();
                            inst = null;

                            // Determine whether the product class implements
                            // one or more of the neccesary product interfaces
                            // and add mappings for each one thats implemented
                            Type tProduct = typeof(TProduct);
                            Type prodInterface = type.GetInterface(tProduct.ToString());
                            if (prodInterface != null)
                            {
                                registry.Add(key, new Pair<Assembly, Type>(assembly, type));
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
