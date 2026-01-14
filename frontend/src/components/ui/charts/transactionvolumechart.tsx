import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from "recharts";

type MonthlyVolumeData = {
  month: string;
  volume: number;
};

type TransactionVolumeChartProps = {
  data: MonthlyVolumeData[];
  height?: number;
};

export function TransactionVolumeChart({
  data,
  height = 280,
}: TransactionVolumeChartProps) {
  const formatVolume = (value: number) => {
    if (value >= 1000000) {
      return `$${(value / 1000000).toFixed(1)}M`;
    }
    if (value >= 1000) {
      return `$${(value / 1000).toFixed(0)}K`;
    }
    return `$${value}`;
  };

  return (
    <div className="flex flex-col" style={{ height }}>
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} barSize={40}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} />
          <XAxis
            dataKey="month"
            axisLine={false}
            tickLine={false}
            tickMargin={10}
            fontSize={12}
          />
          <YAxis
            axisLine={false}
            tickLine={false}
            tickFormatter={formatVolume}
            fontSize={12}
          />
          <Tooltip
            formatter={(value) => [formatVolume(value as number), "Volume"]}
            labelFormatter={(label) => `Month: ${label}`}
          />
          <Bar
            dataKey="volume"
            fill="#22c55e"
            radius={[4, 4, 0, 0]}
          />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
