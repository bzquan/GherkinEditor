// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Jamie Dixon Ltd">
//   Jamie Dixon
// </copyright>
// <summary>
//   Defines the static Enums type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GraphVizWrapper
{
    public static class Enums
    {
        public enum GraphReturnType
        {
            Pdf, Jpg, Png, Plain, PlainExt, Svg
        }

        public enum RenderingEngine
        {
            Dot, Neato, Twopi, Circo, Fdp, Sfdp, Patchwork, Osage 
        }
    }
}
