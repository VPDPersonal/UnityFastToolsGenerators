using Unity.Profiling;

namespace Aspid.UnityFastTools
{
    public class ProfilerMarkerGeneratorTest
    {
        public ProfilerMarkerGeneratorTest()
        {
            this.Marker();
        }

        public bool IsActive
        {
            get
            {
                using var _ = this.Marker();
                return field;
            }

            set
            {
                using var _ = this.Marker();
                field = value;
            }
        }
    
        public void DoSomething1()
        {
            using var _ = this.Marker();
        }
    
        public void DoSomething2()
        {
            using (this.Marker()) { }
            
            using var marker2 = this.Marker();
        }
    
        public void DoSomething3()
        {
            using var marker1 = this.Marker();
            using var marker3 = this.Marker().WithName("Name");
        }
    }

    public static class ProfilerMarkerExtensionsForGenerator
    {
        public static ProfilerMarker.AutoScope Marker(this object _) => default;
        
        public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string _) => marker;
    }
}