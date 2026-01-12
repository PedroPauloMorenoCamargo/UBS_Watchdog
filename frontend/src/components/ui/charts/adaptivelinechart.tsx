import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from "recharts";

type LineConfig<T> = {
  key: keyof T
  label: string
  color: string
}

type AdaptiveLineChartProps<T> = {
  data: T[]
  xKey: keyof T
  lines: LineConfig<T>[]
  height?: number
  legendGap?: number
  showLegend?: boolean 
}

export function AdaptiveLineChart<T>({
  data,
  xKey,
  lines,
  height = 256,
  legendGap = 8,
  showLegend= true,
}: AdaptiveLineChartProps<T>) {
  return (
    <div className="flex flex-col" style={{ height }}>
      {showLegend && (
  <div
    className="flex justify-center flex-wrap text-sm"
    style={{ gap: legendGap, marginBottom: legendGap }}
  >
    {lines.map((line) => (
      <div key={String(line.key)} className="flex items-center gap-2">
        <span
          className="h-2 w-6 rounded-sm"
          style={{ backgroundColor: line.color }}
        />
        <span className="text-slate-600">{line.label}</span>
      </div>
    ))}
  </div>
)}

      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} />
          <XAxis dataKey={xKey as string} 
            axisLine={false}
            tickLine={false}
            tickMargin={15}
            interval={0}
          />
          <YAxis 
            axisLine={false}
            tickLine={false}
          />
          <Tooltip />

          {lines.map((line) => (
            <Line
              key={String(line.key)}
              type="monotone"
              dataKey={line.key as string}
              stroke={line.color}
              strokeWidth={2}
              dot={{ r: 3 }}
            />
          ))}
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
