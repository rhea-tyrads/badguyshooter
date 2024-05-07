namespace Watermelon.LevelSystem
{
    public class NavMeshCallback : INavMeshAgent
    {
        SimpleCallback onNavMeshInitialised;

        public NavMeshCallback(SimpleCallback onNavMeshInitialised)
        {
            this.onNavMeshInitialised = onNavMeshInitialised;
        }

        public void OnNavMeshUpdated()
        {
            onNavMeshInitialised?.Invoke();
        }
    }
}
