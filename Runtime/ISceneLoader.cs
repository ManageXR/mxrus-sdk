using Cysharp.Threading.Tasks;

namespace MXRUS.SDK {
    /// <summary>
    /// Loads a .mxrus file
    /// </summary>
    public interface ISceneLoader {
        UniTask<bool> Load(string sourceFilePath, string extractLocation);
        void Unload();
    }
}
