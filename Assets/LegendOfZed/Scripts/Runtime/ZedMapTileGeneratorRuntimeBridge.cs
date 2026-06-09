using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LegendOfZed.Runtime
{
    /// <summary>
    /// Runtime bridge between the imported legacy map tile generator and the current top-down controller scene flow.
    /// Keeps the legacy import isolated: waits for generation, finds a walkable spawn, moves/spawns player