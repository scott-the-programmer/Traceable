using System;
using System.Collections.Generic;
using System.Threading;

namespace Traceable;

internal static class TraceableScope
{
    private static readonly AsyncLocal<Stack<ITraceableBase>> _scopeStack = new AsyncLocal<Stack<ITraceableBase>>();

    public static ITraceableBase[]? GetCurrentScope()
    {
        var stack = _scopeStack.Value;
        return stack?.Count > 0 ? stack.ToArray() : null;
    }

    public static IDisposable Push(ITraceableBase condition)
    {
        if (_scopeStack.Value == null)
            _scopeStack.Value = new Stack<ITraceableBase>();

        _scopeStack.Value.Push(condition);
        return new ScopeDisposer();
    }

    private class ScopeDisposer : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            var stack = _scopeStack.Value;
            if (stack?.Count > 0)
                stack.Pop();
        }
    }
}
