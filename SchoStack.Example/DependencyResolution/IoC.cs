using StructureMap;
namespace SchoStack.Example {
    public static class IoC {
        public static IContainer Initialize() {
            ObjectFactory.Initialize(x =>
                        {
                            x.Scan(scan =>
                                    {
                                        scan.TheCallingAssembly();
                                        scan.WithDefaultConventions();
                                    });
            //                x.For<IExample>().Use<Example>();
                        });
            return ObjectFactory.Container;
        }
    }
}