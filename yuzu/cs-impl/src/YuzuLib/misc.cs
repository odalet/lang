using System;

namespace Yuzu;

public sealed class ShouldNotHappenException(string message) : Exception(message) { }
