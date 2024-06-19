using Axegen;
public class PoolManager
{
    /*
     * This class is a singleton class that manages all the object pools in the game.
    */

    #region Singleton
    private static PoolManager instance;
    public static PoolManager Instance
    {
        get
        {
            if (instance == null) instance = new PoolManager();
            return instance;
        }
    }
    #endregion

    private ObjectPool<TerrainChunkObject> chunkPool;
    public PoolManager()
    {
        chunkPool = new ObjectPool<TerrainChunkObject>("Prefabs/TerrainChunkObject", 1000);
    }

    public TerrainChunkObject GetChunkObject()
    {
        return chunkPool.Get();
    }
}
