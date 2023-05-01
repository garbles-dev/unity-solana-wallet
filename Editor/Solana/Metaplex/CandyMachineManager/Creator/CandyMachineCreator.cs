using System.IO;
using UnityEditor;

namespace Solana.Unity.SDK.Editor
{
    /// <summary>
    /// A setup wizard used to create <see cref="CandyMachineConfiguration"/>s.
    /// </summary>
    internal class CandyMachineCreator : SolanaSetupWizard<CandyMachineConfiguration>
    {

        #region Properties

        private static string configDirectory;

        #endregion

        #region Static

        /// <summary>
        /// Opens a new copy of this window ready to create a new config.
        /// </summary>
        /// <param name="configDirectory">
        /// The directory in which to save the new config asset.
        /// </param>
        internal static void OpenNew(string configDirectory)
        {
            GetWindow(typeof(CandyMachineCreator), false, "Candy Machine Creator");
            CandyMachineCreator.configDirectory = configDirectory;
        }

        #endregion

        #region SolanaSetupWizard

        /// <inheritdoc/>
        private protected override void OnWizardFinished()
        {
            var assetPath = Path.Combine(configDirectory, "config.asset");
            AssetDatabase.CreateAsset(target.targetObject, assetPath);
            AssetDatabase.SaveAssets();
            Close();
        }

        #endregion
    }
}
