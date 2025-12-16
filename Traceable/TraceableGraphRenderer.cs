using System.Text;

namespace Traceable;

internal static class TraceableGraphRenderer
{
    public static string Render(GraphNode node, string prefix, bool isLast, bool isRoot)
    {
        var sb = new StringBuilder();
        string connector = isRoot ? "" : (isLast ? "└── " : "├── ");
        sb.AppendLine($"{prefix}{connector}{node.Description ?? node.Name} = {node.Value?.ToString() ?? "null"}");

        if (node.ArbitraryState != null || node.ValueState != null)
        {
            string statePrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));
            if (node.ArbitraryState != null)
                foreach (var kvp in node.ArbitraryState)
                    sb.AppendLine($"{statePrefix}  [arbitrary] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
            if (node.ValueState != null)
                foreach (var kvp in node.ValueState)
                    sb.AppendLine($"{statePrefix}  [value] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
        }

        if (node.Children.Count > 0)
        {
            string childPrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));
            for (int i = 0; i < node.Children.Count; i++)
                sb.Append(Render(node.Children[i], childPrefix, i == node.Children.Count - 1, false));
        }

        return sb.ToString().TrimEnd('\r', '\n');
    }
}
