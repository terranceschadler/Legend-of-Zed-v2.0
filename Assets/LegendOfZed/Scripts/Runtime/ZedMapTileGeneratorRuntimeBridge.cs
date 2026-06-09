using System.Collections;
using UnityEngine;

namespace LegendOfZed.Runtime
{
    /// <summary>
    /// Bridges the imported legacy map tile generator into the playable top-down scene without editing the legacy package.
    /// Attach this to the same scene as the legacy generator. It waits for tiles