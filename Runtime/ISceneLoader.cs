using System.Threading.Tasks;

namespace MXRUS.SDK {
    /// <summary>
    /// Loads a .mxrus file
    /// </summary>
    public interface ISceneLoader {
        Task<bool> Load(string sourceFilePath, string extractLocation);
        void Unload();
    }
}
