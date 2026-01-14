import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from "recharts";

type AreaConfig<T> = {
  key: keyof T;
  label: string;
  color: string;
};

type AdaptiveAreaChartProps<T> = {
  data: T[]
  xKey: keyof T
  areas: AreaConfig<T>[];
  height?: number;
  legendGap?: number;
  showLegend?: boolean;
};

export function AdaptiveAreaChart<T>({
  data,
  xKey,
  areas,
  height = 256,
  legendGap = 8,
  showLegend = true,
}: AdaptiveAreaChartProps<T>) {
  return (
    <div className="flex flex-col" style={{ height }}>
      
      {showLegend && (
        <div
          className="flex justify-center flex-wrap text-sm"
          style={{ gap: legendGap, marginBottom: legendGap }}
        >
          {areas.map((area) => (
            <div key={String(area.key)} className="flex items-center gap-2">
              <span
                className="h-2 w-6 rounded-sm"
                style={{ backgroundColor: area.color }}
              />
              <span className="text-slate-600">{area.label}</span>
            </div>
          ))}
        </div>
      )}

      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={data}>
        
          <defs>
            {areas.map((area) => (
              <linearGradient
                key={String(area.key)}
                id={`gradient-${String(area.key)}`}
                x1="0"
                y1="0"
                x2="0"
                y2="1"
              >
                <stop offset="5%" stopColor={area.color} stopOpacity={0.6} />
                <stop offset="95%" stopColor={area.color} stopOpacity={0.05} />
              </linearGradient>
            ))}
          </defs>

          <CartesianGrid strokeDasharray="3 3" vertical={false} />

          <XAxis
            dataKey={xKey as string}
            axisLine={false}
            tickLine={false}
            tickMargin={15}
            interval={0}
          />

          <YAxis axisLine={false} tickLine={false} />
          <Tooltip />

          {areas.map((area) => (
            <Area
              key={String(area.key)}
              type="monotone"
              dataKey={area.key as string}
              stroke={area.color}
              fill={`url(#gradient-${String(area.key)})`}
              strokeWidth={2}
              dot={{ r: 3 }}
            />
          ))}
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
