import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
  Cell,
} from "recharts";

type SeverityData = {
  severity: string;
  count: number;
};

type AlertsBySeverityChartProps = {
  data: SeverityData[];
  height?: number;
};

const SEVERITY_COLORS: Record<string, string> = {
  Low: "#22c55e",
  Medium: "#f59e0b",
  High: "#ef4444",
  Critical: "#7c3aed",
};

export function AlertsBySeverityChart({
  data,
  height = 256,
}: AlertsBySeverityChartProps) {
  // Transform data to ensure we have all severities
  const chartData = [
    { severity: "Low", count: data.find(d => d.severity === "Low")?.count ?? 0 },
    { severity: "Medium", count: data.find(d => d.severity === "Medium")?.count ?? 0 },
    { severity: "High", count: data.find(d => d.severity === "High")?.count ?? 0 },
    { severity: "Critical", count: data.find(d => d.severity === "Critical")?.count ?? 0 },
  ];

  return (
    <div className="flex flex-col" style={{ height }}>
      <div className="flex justify-center flex-wrap text-sm gap-4 mb-2">
        {Object.entries(SEVERITY_COLORS).map(([severity, color]) => (
          <div key={severity} className="flex items-center gap-2">
            <span
              className="h-3 w-3 rounded-sm"
              style={{ backgroundColor: color }}
            />
            <span className="text-slate-600">{severity}</span>
          </div>
        ))}
      </div>

      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartData} barSize={60}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} />
          <XAxis
            dataKey="severity"
            axisLine={false}
            tickLine={false}
            tickMargin={10}
          />
          <YAxis
            axisLine={false}
            tickLine={false}
            allowDecimals={false}
          />
          <Tooltip
            formatter={(value) => [value, "Cases"]}
            labelFormatter={(label) => `Severity: ${label}`}
          />
          <Bar dataKey="count" radius={[4, 4, 0, 0]}>
            {chartData.map((entry) => (
              <Cell
                key={entry.severity}
                fill={SEVERITY_COLORS[entry.severity] ?? "#94a3b8"}
              />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
